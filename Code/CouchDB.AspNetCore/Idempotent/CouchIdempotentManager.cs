using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CouchDB.AspNetCore.Client;

namespace CouchDB.AspNetCore.Idempotent
{
    public class CouchIdempotentManager : IIdempotentManager
    {
        private readonly CouchProxy _couch;
        private readonly CouchIdempotentOptions _options;
        private readonly ILogger _logger;

        public CouchIdempotentManager(IOptions<CouchIdempotentOptions> options, ILogger<CouchIdempotentManager> logger)
        {
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.ServerAddress)) throw new NullReferenceException("configuration not found for 'ServerAddress'");
            if (string.IsNullOrWhiteSpace(_options.DbName)) throw new NullReferenceException("configuration not found for 'DbName'");

            _couch = new CouchProxy(_options.ServerAddress, _options.DbName);
        }

        public async Task<ObjectResult> GetIdempotent(string idempotentKey)
        {
            try
            {
                var idem = await _couch.GetById<Idempotent>(idempotentKey);
                if (idem != null)
                {
                    _logger.LogDebug($"Idempotent found for key {idempotentKey}");
                    return idem.Result;
                }
            }
            catch (Exception ex) 
            { 
                _logger?.LogWarning($"Failure getting idempotent: {ex.Message}"); 
            }
            return null;
        }

        public async Task SetIdempotent(string idempotentKey, ObjectResult result)
        {
            try
            {
                await _couch.Upsert(new Idempotent { Id = idempotentKey, Result = result });
            }
            catch (Exception ex) 
            { 
                _logger?.LogWarning($"Failure setting idempotent: {ex.Message}"); 
            }
        }
        
    }
}