using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using Humanizer;
using MineProtocol;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Database;

namespace ServerOverflow;

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
    /// <returns>Result</returns>
    public static async Task Connect(Server server, ConcurrentBag<WriteModel<Server>> requests) {
        const string uuid = "be7b89d7-efed-452d-a716-4c0eec4c8e2d";
        const string name = "ServerOverflow";
        var client = new TcpClient();
        try {
            var timeout = TimeSpan.FromSeconds(5);
            await client.ConnectAsync(server.IP, server.Port).WaitAsync(timeout);
            await using var stream = client.GetStream();
            using var packet = new MemoryStream();
            
            // handshake packet
            var protocol = server.Ping.Version?.Protocol ?? 47;
            var ip = server.IP + (server.Ping.IsForge ? "\0FML\0" : "");
            await packet.WriteVarInt(0x00);              // Packet ID
            await packet.WriteVarInt(protocol);          // Protocol Version
            await packet.WriteString(ip);                // Server IP
            await packet.WriteShort((short)server.Port); // Server Port
            await packet.WriteVarInt(2);                 // Next State
            await stream.WriteVarInt((int) packet.Position).WaitAsync(timeout);
            await stream.WriteAsync(packet.ToArray().AsMemory(0, (int)packet.Position)).AsTask().WaitAsync(timeout);
            packet.Position = 0;
            
            // login start packet
            await packet.WriteVarInt(0x00);    // Packet ID
            await packet.WriteString(name);    // Username
            if (protocol is >= 759 and <= 762)
                await packet.WriteBytes(0x00); // Has signature
            if (protocol >= 760) {
                var guidBytes = new Guid(uuid).ToByteArray();            
                var uuidBytes = new[] {
                    guidBytes[6], guidBytes[7], guidBytes[4], guidBytes[5],
                    guidBytes[0], guidBytes[1], guidBytes[2], guidBytes[3],
                    guidBytes[15], guidBytes[14], guidBytes[13], guidBytes[12],
                    guidBytes[11], guidBytes[10], guidBytes[9], guidBytes[8]
                };
                if (protocol <= 763)
                    await packet.WriteBytes(1);     // Has UUID
                await packet.WriteBytes(uuidBytes); // UUID
            }
            await stream.WriteVarInt((int) packet.Position).WaitAsync(timeout);
            await stream.WriteAsync(packet.ToArray().AsMemory(0, (int)packet.Position)).AsTask().WaitAsync(timeout);
            await stream.ReadVarInt().WaitAsync(timeout); // ignore packet length
            server.JoinResult = await stream.ReadVarInt().WaitAsync(timeout) switch {
                0x00 => new Result { Success = true, Whitelist = true, DisconnectReason = await stream.ReadString() },
                0x01 => new Result { Success = true, OnlineMode = true },
                _ => new Result { Success = true, OnlineMode = false }
            };
            
            Interlocked.Increment(ref _servers);
            requests.Add(new ReplaceOneModel<Server>(
                Builders<Server>.Filter.Eq(y => y.Id, server.Id), server));
        } catch (Exception e) {
            server.JoinResult = new Result { Success = false, ErrorMessage = e.Message };
            Interlocked.Increment(ref _servers);
            requests.Add(new ReplaceOneModel<Server>(
                Builders<Server>.Filter.Eq(y => y.Id, server.Id), server));
        }
    }

    /// <summary>
    /// Main worker thread
    /// </summary>
    public static async Task WorkerThread() {
        _ = Task.Run(LoggerThread);
        while (true) {
            try {
                var builder = Builders<Server>.Filter;
                var query = builder.Eq(x => x.JoinResult, null) |
                            builder.Gt(x => x.JoinResult!.Timestamp,
                                DateTime.UtcNow + TimeSpan.FromDays(1));
                var total = await Controller.Servers.CountDocumentsAsync(query);
                if (total == 0) continue;
                
                Log.Information("Bulk joining {0} servers", total);
                var watch = new Stopwatch(); watch.Start();
                using var cursor = await Controller.Servers.FindAsync(
                    query, new FindOptions<Server> { BatchSize = 500 });

                _active = true;
                while (await cursor.MoveNextAsync()) {
                    var requests = new ConcurrentBag<WriteModel<Server>>();
                    var tasks = cursor.Current.Select(x => Connect(x, requests)).ToArray();
                    await Task.WhenAll(tasks);
                    await Controller.Servers.BulkWriteAsync(requests);
                }
                
                watch.Stop(); _active = false;
                Log.Information("Completed bulk join in {0}", watch.Elapsed);
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
            while (!_active)
                await Task.Delay(1000);
            while (_active) {
                _serversAvg.Add(_servers - _serversAvg[-1]);
                if (_serversAvg.Count >= 10) {
                    Log.Information("Joined {0} servers ({1} per second)", _servers, _serversAvg.Average());
                    Interlocked.Exchange(ref _servers, 0); _serversAvg = [0];
                }
                
                await Task.Delay(1000);
            }
        }
    }
    
    /// <summary>
    /// Join result
    /// </summary>
    public class Result {
        /// <summary>
        /// Was the attempt successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Is online mode enabled
        /// </summary>
        public bool? OnlineMode { get; set; }
        
        /// <summary>
        /// Is whitelist enabled
        /// </summary>
        public bool? Whitelist { get; set; }
        
        /// <summary>
        /// Reason for the disconnect
        /// </summary>
        public string? DisconnectReason { get; set; }

        /// <summary>
        /// When was the result produced
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Encodes the description into HTML
        /// </summary>
        /// <returns>Raw HTML</returns>
        public string? ReasonToHtml() {
            if (DisconnectReason == null) return null;
            
            try {
                return TextComponent.Parse(DisconnectReason).ToHtml();
            } catch {
                return "<b>Failed to deserialize the chat component!</b>";
            }
        }
    }
}