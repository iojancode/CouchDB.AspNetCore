namespace CouchDB.AspNetCore.SignalR
{
    public class CouchHubOptions
    {
        public string ServerAddress { get; set; }
        public string DbName { get; set; }
        public bool AvoidStorm { get; set; }
    }
}