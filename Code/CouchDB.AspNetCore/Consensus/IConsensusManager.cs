using System;
using System.Threading.Tasks;

namespace CouchDB.AspNetCore.Consensus
{
    public interface IConsensusManager
    {
        Task<bool> CheckLeadership(string assignment);
    }
}