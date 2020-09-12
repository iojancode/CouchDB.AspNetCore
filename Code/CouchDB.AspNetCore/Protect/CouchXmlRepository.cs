using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using CouchDB.AspNetCore.Client;
using Newtonsoft.Json;

namespace CouchDB.AspNetCore.Protect
{
    public class CouchXmlRepository : IXmlRepository
    {
        private readonly CouchProxy _couch;
        
        public CouchXmlRepository(string couchAddress, string couchDbName) 
        {
            _couch = new CouchProxy(couchAddress, couchDbName);
        }
    
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var elements = Task.Run(() => GetAllElementsAsync()).GetAwaiter().GetResult();
            return new ReadOnlyCollection<XElement>(elements);
        }

        private async Task<IList<XElement>> GetAllElementsAsync()
        {
            var result = await _couch.QueryView<ProtectedKey>("_all_docs");
            return result.Select(pk => XElement.Parse(pk.Data)).ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            Task.Run(() => StoreElementAsync(element, friendlyName)).GetAwaiter().GetResult();
        }

        private async Task StoreElementAsync(XElement element, string friendlyName)
        {
            var data = new ProtectedKey { Id = friendlyName, Data = element.ToString() };
            await _couch.Insert(data);
        }
    }
}