using System;
using System.Threading.Tasks;

namespace CouchDB.AspNetCore.Consensus
{
    public class SoloConsensusManager : IConsensusManager
    {
        public Task<bool> CheckLeadership(string assignment)
        {
            return Task.FromResult(true);
        }
    }
}