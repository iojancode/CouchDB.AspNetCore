using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CouchDB.AspNetCore.Idempotent
{
    public interface IIdempotentManager
    {
        Task<ObjectResult> GetIdempotent(string idempotentKey);

        Task SetIdempotent(string idempotentKey, ObjectResult result);
    }
}