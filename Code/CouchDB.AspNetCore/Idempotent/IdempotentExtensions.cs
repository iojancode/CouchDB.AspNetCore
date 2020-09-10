using System;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.AspNetCore.Idempotent
{
    public static class IdempotentExtensions
    {
        public static void AddCouchIdempotentManager(this IServiceCollection services, string couchServerAddress, string couchDbName)
        {
            if (couchServerAddress == null) throw new ArgumentNullException(nameof(couchServerAddress));
            if (couchDbName == null) throw new ArgumentNullException(nameof(couchDbName));

            services.Configure<CouchIdempotentOptions>(options =>
            {
                options.ServerAddress = couchServerAddress;
                options.DbName = couchDbName;
            });
            services.AddSingleton<IIdempotentManager, CouchIdempotentManager>();
        }

    }
}