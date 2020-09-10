using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CouchDB.AspNetCore.Idempotent
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IdempotentKeyAttribute : Attribute, IFilterFactory
    {
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new InternalIdempotentKeyFilter(serviceProvider.GetService<IIdempotentManager>());
        }

        private class InternalIdempotentKeyFilter : IAsyncActionFilter
        {
            private const string IDEMPOTENT_HEADER = "IdempotentKey";
            private readonly IIdempotentManager _manager;

            public InternalIdempotentKeyFilter(IIdempotentManager manager)
            {
                _manager = manager;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                string theKey = context.HttpContext.Request.Headers[IDEMPOTENT_HEADER];

                if (!string.IsNullOrEmpty(theKey)) context.Result = await _manager.GetIdempotent(theKey);
                if (context.Result != null) return;

                var excecuted = await next();
                var executedResult = excecuted.Result as ObjectResult;
                
                if (executedResult == null || string.IsNullOrEmpty(theKey)) return; 
                var backgroundTask = _manager.SetIdempotent(theKey, executedResult);
            }
        }

        public bool IsReusable => true;
    }
}