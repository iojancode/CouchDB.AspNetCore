using System;
using System.Xml.Linq;

namespace CouchDB.AspNetCore.Protect
{
    public class ProtectedKey
    {
        public string Id { get; set; }
        public string Data { get; set; }
    }
}