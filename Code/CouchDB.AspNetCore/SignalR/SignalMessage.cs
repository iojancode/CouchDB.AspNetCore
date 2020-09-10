using System.Collections.Generic;
using CouchDB.AspNetCore.Client;
using Newtonsoft.Json;

namespace CouchDB.AspNetCore.SignalR
{
    public class SignalMessage : ICouchChange
    {
        public string Call { get; set; }
        public string MethodName { get; set; }  
        public object[] Args { get; set; }
        public string ConnectionId { get; set; }
        public IEnumerable<string> ConnectionIds { get; set; }
        public IEnumerable<string> ExcludedConnectionIds { get; set; }
        public string GroupName { get; set; }
        public IEnumerable<string> GroupNames { get; set; }
        public string UserId { get; set; }
        public IEnumerable<string> UserIds { get; set; }

        [JsonIgnore]
        public string Seq { get; set; }
    }
}