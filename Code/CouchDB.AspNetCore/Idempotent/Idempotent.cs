using System;
using Microsoft.AspNetCore.Mvc;

namespace CouchDB.AspNetCore.Idempotent
{
    public class Idempotent
    {
        public string Id { get; set; }
        public ObjectResult Result { get; set; }
    }
}