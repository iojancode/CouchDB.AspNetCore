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
            var result = await _couch.QueryView<string>("_all_docs");
            return result.Select(json => JsonConvert.DeserializeXNode(json).Root).ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            Task.Run(() => StoreElementAsync(element)).GetAwaiter().GetResult();
        }

        private async Task StoreElementAsync(XElement element)
        {
            string json = JsonConvert.SerializeXNode(element);
            await _couch.Upsert<string>(json);
        }
    }
}