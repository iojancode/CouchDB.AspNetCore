using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MyCouch;
using MyCouch.Net;
using Polly;

namespace CouchDB.AspNetCore.Client
{
    public class PolicyClientBootstrapper : MyCouchClientBootstrapper
    {
        private IAsyncPolicy<HttpResponseMessage> _policy;

        public PolicyClientBootstrapper(IAsyncPolicy<HttpResponseMessage> policy) 
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            _policy = policy;
        }

        protected override void ConfigureDbConnectionFn()
        {
            DbConnectionFn = cnInfo => new PolicyDbConnection(cnInfo, _policy);
        }
    }

    internal class PolicyDbConnection : DbConnection
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public PolicyDbConnection(DbConnectionInfo connectionInfo, IAsyncPolicy<HttpResponseMessage> policy) : base(connectionInfo)
        {
            _policy = policy;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequest httpRequest, CancellationToken cancellationToken = default)
        {
            return await _policy.ExecuteAsync(() => base.SendAsync(httpRequest, cancellationToken));
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequest httpRequest, HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
        {
            return await _policy.ExecuteAsync(() => base.SendAsync(httpRequest, completionOption, cancellationToken));
        }
    }

}
