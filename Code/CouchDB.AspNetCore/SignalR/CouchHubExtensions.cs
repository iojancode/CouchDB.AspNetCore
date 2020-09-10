using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.AspNetCore.SignalR
{
    public static class CouchHubExtensions
    {
        public static ISignalRServerBuilder AddCouchBackplane(this ISignalRServerBuilder signalrBuilder, string couchServerAddress, string couchDbName, bool avoidStorm = false)
        {
            if (couchServerAddress == null) throw new ArgumentNullException(nameof(couchServerAddress));
            if (couchDbName == null) throw new ArgumentNullException(nameof(couchDbName));

            signalrBuilder.Services.Configure<CouchHubOptions>(options => {
                options.ServerAddress = couchServerAddress;
                options.DbName = couchDbName;
                options.AvoidStorm = avoidStorm;
            });
            signalrBuilder.Services.AddSingleton(typeof(HubLifetimeManager<>), typeof(CouchHubLifetimeManager<>));
            return signalrBuilder;
        }        
    }
}