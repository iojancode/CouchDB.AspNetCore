using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Polly;
using MyCouch;
using MyCouch.Requests;

namespace CouchDB.AspNetCore.Client
{
    public class CouchProxy : IDisposable
    {
        internal static PolicyClientBootstrapper BOOT = new PolicyClientBootstrapper(
            Policy<HttpResponseMessage>
            .HandleResult(r => r.StatusCode == HttpStatusCode.PaymentRequired)
            .WaitAndRetryAsync(retryCount: 4,
                sleepDurationProvider: c => TimeSpan.FromMilliseconds(500),
                onRetry: (r, s) => Console.WriteLine($"*** resilient retry = {r.Result.StatusCode} ***"))
        );
        private MyCouchClient _client;

        public CouchProxy(string url, string db)
        {
            if (url == null || db == null) throw new NullReferenceException("url or db shall not be null");
            _client = new MyCouchClient(url, db, BOOT);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<T> GetById<T>(string id) where T : class
        {
            var result = await _client.Entities.GetAsync<T>(id);
            if (!result.IsSuccess && result.Reason != "missing" && result.Reason != "deleted")
                throw new CouchProxyException($"Failure looking record by {id}: {result.Reason}");
            return result.Content;
        }

        public async Task Insert<T>(T entity) where T : class
        {
            var result = await _client.Entities.PostAsync(entity);
            if (!result.IsSuccess) throw new CouchProxyException($"Insert failure: {result.Reason}");
        }
        
        public async Task<List<T>> QueryView<T>(string viewName, object key = null, bool descending = false, int limit = 100, int page = 0) where T : class
        {
            var query = new QueryViewRequest(viewName).Configure(q => q
                .IncludeDocs(true)
                .Descending(descending)
                .Reduce(false));
            if (limit > 0) query.Limit = limit;
            if (page > 0 && limit > 0) query.Skip = (page * limit) + 1;
            if (key != null) query.Key = key;

            var result = await _client.Views.QueryAsync<string, T>(query);
            if (!result.IsSuccess) 
                throw new CouchProxyException($"Failure looking for {viewName} by {key}: {result.Reason}");
            return result.Rows.Select(d => d.IncludedDoc).ToList();
        }

        public async Task<List<T>> PullChanges<T>(string sinceSeq = null, int? msecTimeout = null, int limit = 100) where T : class, ICouchChange
        {
            var query = new GetChangesRequest()
            {
                Feed = ChangesFeed.Longpoll,
                IncludeDocs = true,
                Since = sinceSeq ?? "now",
                Timeout = msecTimeout,
                Limit = limit
            };

            var result = await _client.Changes.GetAsync<T>(query);
            if (!result.IsSuccess) 
                throw new CouchProxyException($"Failure looking for changes since {sinceSeq}: {result.Reason}");
            foreach (var d in result.Results) d.IncludedDoc.Seq = d.Seq;
            return result.Results.Select(d => d.IncludedDoc).ToList();
        }

    }
}