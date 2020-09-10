using System;

namespace CouchDB.AspNetCore.Client
{
    public class CouchProxyException : Exception
    {
        public CouchProxyException(string message) : base(message) { }
        public CouchProxyException(string message, Exception inner) : base(message, inner) { }
    }
}