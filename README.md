# CouchDB.AspNetCore #

AspNet Core extension services backed by CouchDB.

Configure at your Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // data protection, out of the box
    services.AddDataProtection().PersistKeysToCouch("http://localhost:5984", "keysdb");

    // signalr backplane, out of the box
    services.AddSignalR().AddCouchBackplane("http://localhost:5984", "signaldb", avoidStorm: true);

    // idempotency 
    services.AddCouchIdempotentManager("http://localhost:5984", "idempotentdb");
    
    // consensus 
    services.AddCouchConsensusManager("http://localhost:5984", "consensusdb");
}
```

Use at your API method with "IdempotentKey" attribute:

```csharp
[IdempotentKey]
[Authorize]
[HttpPost]
public async Task<IActionResult> PostData(CustomViewModel model)
{
    /* important work that should not be invoked more than once if "IdempotentKey" HTTP header matches */
}
```

Inject consensus to your controller and check leadership four your task:

```csharp
public CustomController(IConsensusManager consensus)
{
    _consensus = consensus;
}

public async Task DoWork()
{
    bool leader = await _consensus.CheckLeadership("election_" + DateTime.UtcNow.ToString("yyyyMMdd"));
    if (leader) { /* important work if today this node is the leader of your cluster */ }
}
```