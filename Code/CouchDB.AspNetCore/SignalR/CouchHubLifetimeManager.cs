using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using CouchDB.AspNetCore.Client;

namespace CouchDB.AspNetCore.SignalR
{
    public class CouchHubLifetimeManager<THub> : DefaultHubLifetimeManager<THub> where THub : Hub
    {
        private const int MESSAGE_LIMIT = 1000;
        private string _lastSeq;
        private Task _listenTask;
        private Timer _healthTimer;

        private readonly CouchProxy _couch;
        private readonly CouchHubOptions _options;
        private readonly ILogger _logger;

        public CouchHubLifetimeManager(IOptions<CouchHubOptions> options, ILogger<CouchHubLifetimeManager<THub>> logger)
            : base(logger)
        {
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.ServerAddress)) throw new NullReferenceException("configuration not found for 'ServerAddress'");
            if (string.IsNullOrWhiteSpace(_options.DbName)) throw new NullReferenceException("configuration not found for 'DbName'");

            _couch = new CouchProxy(_options.ServerAddress, _options.DbName);
            _healthTimer = new Timer(_ => _logger.LogDebug($"checking singals {_listenTask?.Status}"), null, 0, 10 * 60 * 1000);
            _listenTask = Task.Run(ListenBroadcast);
        }    

        private async Task ListenBroadcast() 
        {
            if (_couch == null) return;
            _logger.LogInformation($"start pulling broadcast messages");

            while (true) 
            {
                try
                {
                    var messages = await _couch.PullChanges<SignalMessage>(_lastSeq, msecTimeout: 60000, limit: MESSAGE_LIMIT);
                    _logger.LogDebug($"messages pulled {messages?.Count}");

                    if (_options.AvoidStorm && messages?.Count == MESSAGE_LIMIT) 
                    {
                        _logger.LogWarning($"message storm avoided, please check logs for contention");
                        _lastSeq = null;
                        continue;
                    }
                    
                    if (messages?.Count > 0)
                    {
                        var stopwatch = Stopwatch.StartNew();
                        foreach (var m in messages)
                        {
                            _lastSeq = m.Seq;
                            await BroadcastReceived(m, new CancellationTokenSource(30000).Token);
                        }
                        _logger.LogDebug($"messages broadcast {stopwatch.Elapsed}");
                    }
                }
                catch (Exception ex) 
                { 
                    _logger.LogError(ex, "failure listen for messages"); 
                    await Task.Delay(30000);
                }
            }
        }

        private Task BroadcastReceived(SignalMessage msg, CancellationToken cancellationToken = default)
        {
            switch (msg.Call)
            {
                case "SendAllAsync": return base.SendAllAsync(msg.MethodName, msg.Args, cancellationToken); 
                case "SendAllExceptAsync": return base.SendAllExceptAsync(msg.MethodName, msg.Args, msg.ExcludedConnectionIds?.ToList(), cancellationToken);
                case "SendConnectionAsync": return base.SendConnectionAsync(msg.ConnectionId, msg.MethodName, msg.Args, cancellationToken);
                case "SendConnectionsAsync": return base.SendConnectionsAsync(msg.ConnectionIds?.ToList(), msg.MethodName, msg.Args, cancellationToken);
                case "SendGroupAsync": return base.SendGroupAsync(msg.GroupName, msg.MethodName, msg.Args, cancellationToken);
                case "SendGroupExceptAsync": return base.SendGroupExceptAsync(msg.GroupName, msg.MethodName, msg.Args, msg.ExcludedConnectionIds?.ToList(), cancellationToken);
                case "SendGroupsAsync": return base.SendGroupsAsync(msg.GroupNames?.ToList(), msg.MethodName, msg.Args, cancellationToken);
                case "SendUserAsync": return base.SendUserAsync(msg.UserId, msg.MethodName, msg.Args, cancellationToken);
                case "SendUsersAsync": return base.SendUsersAsync(msg.UserIds?.ToList(), msg.MethodName, msg.Args, cancellationToken);
            }

            _logger.LogError($"Messsage not matched {msg.Call}");
            return Task.CompletedTask;
        }

        private async Task SendBroadcast(SignalMessage message)
        {
            if (_couch == null) return;
            try 
            {
                await _couch.Upsert(message);
            } 
            catch (Exception ex) { _logger.LogError(ex, "Failure sending broadcast"); }
        }

        public override Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendAllAsync",
                MethodName = methodName,
                Args = args
            };
            return SendBroadcast(msg);
        }

        public override Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendAllExceptAsync",
                MethodName = methodName,
                Args = args,
                ExcludedConnectionIds = excludedConnectionIds
            };
            return SendBroadcast(msg);
        }

        public override Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendConnectionAsync",
                ConnectionId = connectionId,
                MethodName = methodName,
                Args = args
            };
            return SendBroadcast(msg);
        }

        public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendConnectionsAsync",
                ConnectionIds = connectionIds,
                MethodName = methodName,
                Args = args
            };            
            return SendBroadcast(msg);
        }

        public override Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendGroupAsync",
                GroupName = groupName,
                MethodName = methodName,
                Args = args
            };            
            return SendBroadcast(msg);
        }

        public override Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendGroupExceptAsync",
                GroupName = groupName,
                MethodName = methodName,
                Args = args,
                ExcludedConnectionIds = excludedConnectionIds
            };            
            return SendBroadcast(msg);
        }

        public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendGroupsAsync",
                GroupNames = groupNames,
                MethodName = methodName,
                Args = args,
            };                      
            return SendBroadcast(msg);
        }

        public override Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendUserAsync",
                UserId = userId,
                MethodName = methodName,
                Args = args,
            };             
            return SendBroadcast(msg);
        }

        public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = new SignalMessage {
                Call = "SendUsersAsync",
                UserIds = userIds,
                MethodName = methodName,
                Args = args,
            };   
            return SendBroadcast(msg);
        }
    }
}