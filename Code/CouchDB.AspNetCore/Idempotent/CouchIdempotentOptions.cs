using System;

namespace CouchDB.AspNetCore.Idempotent
{
    public class CouchIdempotentOptions
    {
        public string ServerAddress { get; set; }
        public string DbName { get; set; }
    }
}