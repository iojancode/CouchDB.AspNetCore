using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CouchDB.AspNetCore.Client;

namespace CouchDB.AspNetCore.Consensus
{
    public class CouchConsensusManager : IConsensusManager
    {
        private readonly CouchProxy _couch;
        private readonly CouchConsensusOptions _options;
        private readonly ILogger _logger;

        public CouchConsensusManager(IOptions<CouchConsensusOptions> options, ILogger<CouchConsensusManager> logger)
        {
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.ServerAddress)) throw new NullReferenceException("configuration not found for 'ServerAddress'");
            if (string.IsNullOrWhiteSpace(_options.DbName)) throw new NullReferenceException("configuration not found for 'DbName'");

            _couch = new CouchProxy(_options.ServerAddress, _options.DbName);
        }


        public async Task<bool> CheckLeadership(string assignment)
        {
            try 
            {
                var found = await _couch.GetById<Assignment>(assignment);
                if (found != null) return found.Node == Environment.MachineName;

                await _couch.Insert(new Assignment { Id = assignment, Node = Environment.MachineName });
                return true;
            } 
            catch (Exception ex) 
            { 
                _logger.LogWarning($"Failure checking leadership: {ex.Message}"); // concurrency most of the time
                return false;
            }
        }
    }
}