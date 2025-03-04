using System.Collections.Concurrent;
using System.Diagnostics;
using MongoDB.Driver;
using Prometheus;
using Serilog;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;
using static ServerOverflow.Snooper.Configuration;

namespace ServerOverflow.Snooper;

/// <summary>
/// Minecraft bot joiner worker
/// </summary>
public class BotWorker {
    /// <summary>
    /// Total servers joined gauge (online, offline)
    /// </summary>
    private static readonly Counter _serversTotal = Metrics.CreateCounter("so_bot_joined_total", "Total servers joined by the scanner", "mode", "success");
    
    
    /// <summary>
    /// Servers per second value
    /// </summary>
    private static int _servers;

    /// <summary>
    /// Failed joins per second value
    /// </summary>
    private static int _failed;
    
    /// <summary>
    /// Connects to a Minecraft server
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="requests">Requests</param>
    /// <param name="profile">Profile</param>
    /// <returns>Result</returns>
    private static async Task Connect(Server server, ConcurrentBag<WriteModel<Server>> requests, Profile? profile = null) {
        var result = await MinecraftBot.Join(server, profile?.Instance);
        if (!result.Success) Interlocked.Increment(ref _failed);
        _serversTotal.WithLabels(profile == null ? "offline" : "online", result.Success ? "true" : "false").Inc();
        Interlocked.Increment(ref _servers);
        result.LastSeen ??= server.JoinResult?.LastSeen;
        result.Whitelist ??= server.JoinResult?.Whitelist;
        result.OnlineMode ??= server.JoinResult?.OnlineMode;
        result.OnlineTimestamp ??= server.JoinResult?.OnlineTimestamp;
        if (profile != null) result.OnlineTimestamp = DateTime.UtcNow;
        requests.Add(new UpdateOneModel<Server>(
            Builders<Server>.Filter.Eq(y => y.Id, server.Id),
            Builders<Server>.Update.Set(x => x.JoinResult, result)));
    }
    
    /// <summary>
    /// Main worker thread
    /// </summary>
    public static async Task MainThread() {
        while (true) {
            await BulkOffline();
            await BulkOnline();
            await Task.Delay(3600000);
        }
    }

    /// <summary>
    /// Offline mode bulk joining
    /// </summary>
    private static async Task BulkOffline() {
        try {
            var builder = Builders<Server>.Filter;
            var query = builder.Eq(x => x.JoinResult, null) |
                        builder.Lt(x => x.JoinResult!.Timestamp,
                            DateTime.UtcNow - TimeSpan.FromDays(1));
            var total = await Database.Servers.CountDocumentsAsync(query);
            if (total == 0) return;
                
            Log.Information("Bulk joining {0} servers (offline mode)", total);
            using var cursor = await Database.Servers.FindAsync(
                query, new FindOptions<Server> { BatchSize = Config.BatchSize });
            
            var exclusions = await Exclusion.GetAll();
            var requests = new ConcurrentBag<WriteModel<Server>>();
            while (await cursor.MoveNextAsync()) {
                var tasks = cursor.Current
                    .Where(x => exclusions.All(y => !y.IsExcluded(x.IP)))
                    .Select(x => Connect(x, requests)).ToArray();
                Log.Information("Fetched next batch with {0} servers", tasks.Length);
                await Task.WhenAll(tasks);
                if (requests.Count != 0)
                    await Database.Servers.BulkWriteAsync(requests);
            }
        } catch (Exception e) {
            Log.Error("Offline mode bulk joiner crashed: {0}", e);
        }
    }
    
    /// <summary>
    /// Online mode bulk joining
    /// </summary>
    private static async Task BulkOnline() {
        try {
            var builder = Builders<Server>.Filter;
            var query = builder.Ne(x => x.JoinResult, null)
                & builder.Eq(x => x.JoinResult!.OnlineMode, true)
                & builder.Eq(x => x.JoinResult!.Whitelist, null)
                & builder.Eq(x => x.JoinResult!.Success, true)
                & (builder.Eq(x => x.JoinResult!.OnlineTimestamp, null) | 
                   builder.Lt(x => x.JoinResult!.OnlineTimestamp, 
                       DateTime.UtcNow - TimeSpan.FromDays(1)));
            var total = await Database.Servers.CountDocumentsAsync(query);
            if (total == 0) return;
            
            var profiles = await Profile.GetAll();
            var batch = profiles.Count * 3;
            Log.Information("Using {0} profiles ({1} servers per batch)",
                profiles.Count, batch);
            
            Log.Information("Bulk joining {0} servers (online mode)", total);
            using var cursor = await Database.Servers.FindAsync(
                query, new FindOptions<Server> { BatchSize = Config.BatchSize });
            
            var exclusions = await Exclusion.GetAll();
            Server[]? carry = null;
            while (await cursor.MoveNextAsync()) {
                var requests = new ConcurrentBag<WriteModel<Server>>();
                var servers = cursor.Current
                    .Where(x => exclusions.All(y => !y.IsExcluded(x.IP)))
                    .Where(x => !x.IsAntiDDoS())
                    .Concat(carry ?? [])
                    .ToArray();
                Log.Information("Fetched next batch with {0} servers", servers.Length);
                carry = null;
                
                for (var i = 0; i < servers.Length; i += batch) {
                    if (servers.Length - i < batch) {
                        carry = servers[i..];
                        break;
                    }
                    
                    var tasks = servers[i..(i+batch)]
                        .Zip(profiles.SelectMany(x => (Profile[])[x, x, x]),
                            (server, profile) => Connect(server, requests, profile))
                        .ToArray();
                    await Task.WhenAll(tasks);
                    if (requests.Count != 0)
                        await Database.Servers.BulkWriteAsync(requests);
                
                    // wait out the ratelimit
                    await Task.Delay(15000);
                }
            }
        } catch (Exception e) {
            Log.Error("Online mode bulk joiner crashed: {0}", e);
        }
    }
}