using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using MineProtocol;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Database;

namespace ServerOverflow.Processors;

/// <summary>
/// A simple bot that joins servers and checks if it has online mode enabled
/// </summary>
public static class JoinBot {
    /// <summary>
    /// A list for calculating average servers per second
    /// </summary>
    private static List<int> _serversAvg = [0];
    
    /// <summary>
    /// Servers per second value
    /// </summary>
    private static int _servers;

    /// <summary>
    /// Is currently active
    /// </summary>
    private static bool _active;

    /// <summary>
    /// Connects to a Minecraft server
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="requests">Requests</param>
    /// <param name="timeoutSecs">Timeout in seconds</param>
    /// <returns>Result</returns>
    private static async Task Connect(Server server, ConcurrentBag<WriteModel<Server>> requests, int timeoutSecs = 5) {
        var result = await TinyProtocol.Join(server.IP, server.Port, server.Ping.Version?.Protocol ?? 47, server.Ping.IsForge);
        Interlocked.Increment(ref _servers);
        requests.Add(new UpdateOneModel<Server>(
            Builders<Server>.Filter.Eq(y => y.Id, server.Id),
            Builders<Server>.Update.Set(x => x.JoinResult, result)));
    }

    /// <summary>
    /// Joins a server and returns the modified values
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="timeout">Timeout in seconds</param>
    public static async Task<Server> JoinServer(Server server, int timeout = 5) {
        var result = await TinyProtocol.Join(server.IP, server.Port, server.Ping.Version?.Protocol ?? 47, server.Ping.IsForge);
        await Controller.Servers.UpdateOneAsync(
            Builders<Server>.Filter.Eq(y => y.Id, server.Id),
            Builders<Server>.Update.Set(x => x.JoinResult, result));
        server.JoinResult = result;
        return server;
    }

    /// <summary>
    /// Main worker thread
    /// </summary>
    public static async Task MainThread() {
        _ = Task.Run(LoggerThread);
        while (true) {
            try {
                var builder = Builders<Server>.Filter;
                var query = builder.Eq(x => x.JoinResult, null) |
                            builder.Lt(x => x.JoinResult!.Timestamp,
                                DateTime.UtcNow - TimeSpan.FromDays(1));
                var total = await Controller.Servers.CountDocumentsAsync(query);
                if (total == 0) continue;
                
                Log.Information("Bulk joining {0} servers", total);
                using var cursor = await Controller.Servers.FindAsync(
                    query, new FindOptions<Server> { BatchSize = 1000 });

                _active = true;
                var exclusions = await Exclusion.GetAll();
                while (await cursor.MoveNextAsync()) {
                    var requests = new ConcurrentBag<WriteModel<Server>>();
                    var tasks = cursor.Current
                        .Where(x => exclusions.All(y => !y.IsExcluded(x.IP)))
                        .Where(x => !x.IsAntiDDoS())
                        .Select(x => Connect(x, requests)).ToArray();
                    await Task.WhenAll(tasks);
                    if (requests.Count != 0)
                        await Controller.Servers.BulkWriteAsync(requests);
                }
                
                _active = false;
                await Task.Delay(3600000);
            } catch (Exception e) {
                Log.Error("Join bot thread crashed: {0}", e);
                _active = false;
            }
        }
    }

    /// <summary>
    /// Basic speed logger thread
    /// </summary>
    private static async Task LoggerThread() {
        while (true) {
            while (!_active) await Task.Delay(1000);
            var watch = new Stopwatch(); watch.Start();
            while (_active) {
                _serversAvg.Add(_servers - _serversAvg[^1]);
                if (_serversAvg.Count >= 10) {
                    Log.Information("Joined {0} servers ({1} per second)", _servers, _serversAvg.Average());
                    Interlocked.Exchange(ref _servers, 0); _serversAvg = [0];
                }
                
                await Task.Delay(1000);
            }
            
            watch.Stop();
            if (_serversAvg.Count != 0) {
                Log.Information("Joined {0} servers ({1} per second)", _servers, _serversAvg.Average());
                Interlocked.Exchange(ref _servers, 0); _serversAvg = [0];
            }
            
            Log.Information("Completed bulk join in {0}", watch.Elapsed);
        }
    }
}
