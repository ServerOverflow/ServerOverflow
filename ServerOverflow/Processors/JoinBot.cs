using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MineProtocol;
using MineProtocol.Exceptions;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Storage;
using Profile = MineProtocol.Authentication.Profile;

namespace ServerOverflow.Processors;

/// <summary>
/// A simple bot that joins servers and checks if it has online mode enabled
/// </summary>
public static class JoinBot {
    /// <summary>
    /// Offline mode profile
    /// </summary>
    private static readonly Profile _offline = new("ServerOverflow", "be7b89d7-efed-452d-a716-4c0eec4c8e2d");
    
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
    /// Joins a server and returns the result
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="profile">Profile</param>
    /// <param name="protocol">Protocol</param>
    /// <param name="depth">Depth</param>
    /// <returns>Result</returns>
    public static async Task<JoinResult> Join(Server server, Profile? profile = null, int? protocol = null, int depth = 0) {
        profile ??= _offline;
        try {
            if (depth > 3)
                throw new InvalidOperationException("Detected outdated client cycle");
            using var proto = new TinyProtocol(server.IP, server.Port,
                protocol ?? server.Ping.Version?.Protocol ?? 47, server.Ping.IsForge,
                server.Ping.ModernForgeMods?.ProtocolVersion ?? 0);
            await proto.Connect();
            await proto.Handshake();
            await proto.LoginStart(profile);
            while (true) {
                var packet = await proto.Receive();
                if (packet == null) throw new Exception("Unexpected packet received");
                switch (packet.Id) {
                    case TinyProtocol.PacketId.EncryptionRequest:
                        if (profile.Minecraft != null) {
                            var serverId = await packet.Stream.ReadString();
                            var publicKeyLen = await packet.Stream.ReadVarInt();
                            var publicKey = new byte[publicKeyLen];
                            _ = packet.Stream.Read(publicKey, 0, publicKey.Length);
                            var verifyTokenLen = await packet.Stream.ReadVarInt();
                            var verifyToken = new byte[verifyTokenLen];
                            _ = packet.Stream.Read(verifyToken, 0, verifyToken.Length);
                            var secretKey = RandomNumberGenerator.GetBytes(16);
                            if (packet.Stream.Position == packet.Stream.Length || await packet.Stream.ReadBoolean())
                                await profile.Join(serverId, secretKey, publicKey);
                            await packet.Skip();
                            await proto.Encrypt(secretKey, publicKey, verifyToken);
                            break;
                        }
                        
                        proto.Disconnect();
                        return new JoinResult { 
                            RealProtocol = protocol ?? server.Ping.Version?.Protocol ?? 47,
                            Success = true, OnlineMode = true, LastSeen = DateTime.UtcNow
                        };
                    case TinyProtocol.PacketId.LoginSuccess:
                        proto.Disconnect();
                        return new JoinResult {
                            RealProtocol = protocol ?? server.Ping.Version?.Protocol ?? 47, 
                            Success = true, OnlineMode = profile.Minecraft != null,
                            Whitelist = false, LastSeen = DateTime.UtcNow
                        };
                    default:
                        await packet.Skip();
                        break;
                }
            }
        } catch (DisconnectedException e) {
            if (e.Message.Contains("1.13 and above"))
                return await Join(server, profile, 393, depth + 1);
            var match = Regex.Match(e.Message, @"Outdated client! Please use (\d\.\d+\.\d+)");
            if (match.Success && Resources.Version.TryGetValue(match.Groups[1].Value, out var newProto))
                return await Join(server, profile, newProto, depth + 1);
            return new JoinResult {
                RealProtocol = protocol ?? server.Ping.Version?.Protocol ?? 47,
                Success = true, OnlineMode = profile.Minecraft != null,
                Whitelist = true, DisconnectReason = e.Message, LastSeen = DateTime.UtcNow
            };
        } catch (Exception e) {
            return new JoinResult { Success = false, ErrorMessage = e.Message };
        }
    }

    /// <summary>
    /// Connects to a Minecraft server
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="requests">Requests</param>
    /// <param name="profile">Profile</param>
    /// <returns>Result</returns>
    private static async Task Connect(Server server, ConcurrentBag<WriteModel<Server>> requests, Profile? profile = null) {
        var result = await Join(server, profile);
        Interlocked.Increment(ref _servers);
        result.LastSeen ??= server.JoinResult?.LastSeen;
        result.Whitelist ??= server.JoinResult?.Whitelist;
        result.OnlineMode ??= server.JoinResult?.OnlineMode;
        requests.Add(new UpdateOneModel<Server>(
            Builders<Server>.Filter.Eq(y => y.Id, server.Id),
            Builders<Server>.Update.Set(x => x.JoinResult, result)));
    }

    /// <summary>
    /// Main worker thread
    /// </summary>
    public static async Task MainThread() {
        _ = Task.Run(LoggerThread);
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
                    await Database.Servers.BulkWriteAsync(requests);
            }
                
            _active = false;
        } catch (Exception e) {
            Log.Error("Offline mode bulk joiner crashed: {0}", e);
            _active = false;
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
                & builder.Gt(x => x.JoinResult!.LastSeen,
                    DateTime.UtcNow - TimeSpan.FromDays(1));
            var total = await Database.Servers.CountDocumentsAsync(query);
            if (total == 0) return;
            
            var profiles = await Storage.Profile.GetAll();
            var batch = profiles.Count * 3;
            Log.Information("Using {0} profiles ({1} servers per batch)",
                profiles.Count, batch);
            
            Log.Information("Bulk joining {0} servers (online mode)", total);
            using var cursor = await Database.Servers.FindAsync(
                query, new FindOptions<Server> { BatchSize = 1000 });

            _active = true;
            var exclusions = await Exclusion.GetAll();
            Server[]? carry = null;
            while (await cursor.MoveNextAsync()) {
                var requests = new ConcurrentBag<WriteModel<Server>>();
                var servers = cursor.Current
                    .Where(x => exclusions.All(y => !y.IsExcluded(x.IP)))
                    .Where(x => !x.IsAntiDDoS())
                    .Concat(carry ?? [])
                    .ToArray();
                carry = null;
                
                for (var i = 0; i < servers.Length; i += batch) {
                    if (servers.Length - i < batch) {
                        carry = servers[i..];
                        continue;
                    }
                    
                    var tasks = servers[i..(i+batch)].Select(x => Connect(x, requests)).ToArray();
                    await Task.WhenAll(tasks);
                    if (requests.Count != 0)
                        await Database.Servers.BulkWriteAsync(requests);
                
                    // wait out the ratelimit
                    await Task.Delay(15000);
                }
            }
                
            _active = false;
        } catch (Exception e) {
            Log.Error("Online mode bulk joiner crashed: {0}", e);
            _active = false;
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
