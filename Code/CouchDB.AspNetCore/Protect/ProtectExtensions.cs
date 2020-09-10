using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.AspNetCore.Protect
{
    public static class ProtectExtensions
    {
        public static void PersistKeysToCouch(this IDataProtectionBuilder builder, string couchServerAddress, string couchDbName)
        {
            if (couchServerAddress == null) throw new ArgumentNullException(nameof(couchServerAddress));
            if (couchDbName == null) throw new ArgumentNullException(nameof(couchDbName));

            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new CouchXmlRepository(couchServerAddress, couchDbName);
            });
        }

    }
}