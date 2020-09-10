namespace CouchDB.AspNetCore.Client
{
    public interface ICouchChange
    {
        string Seq { get; set; }
    }
}