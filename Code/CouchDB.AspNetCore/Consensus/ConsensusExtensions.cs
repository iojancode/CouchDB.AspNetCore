using System;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.AspNetCore.Consensus
{
    public static class ConsensusExtensions
    {
        public static void AddSoloConsensusManager(this IServiceCollection services)
        {
            services.AddSingleton<IConsensusManager, SoloConsensusManager>();
        }

        public static void AddCouchConsensusManager(this IServiceCollection services, string couchServerAddress, string couchDbName)
        {
            if (couchServerAddress == null) throw new ArgumentNullException(nameof(couchServerAddress));
            if (couchDbName == null) throw new ArgumentNullException(nameof(couchDbName));

            services.Configure<CouchConsensusOptions>(options => {
                options.ServerAddress = couchServerAddress;
                options.DbName = couchDbName;                
            });
            services.AddSingleton<IConsensusManager, CouchConsensusManager>();
        }

    }
}