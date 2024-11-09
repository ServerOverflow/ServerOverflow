using System.Diagnostics;
using System.Net.Sockets;
using Humanizer;
using MineProtocol;
using MongoDB.Driver;
using Overby.Extensions.AsyncBinaryReaderWriter;
using Serilog;
using ServerOverflow.Database;

namespace ServerOverflow;

/// <summary>
/// A simple bot that joins servers and checks if it has online mode enabled
/// </summary>
public static class JoinBot {
    /// <summary>
    /// Connects to a Minecraft server
    /// </summary>
    /// <param name="ip">IP address</param>
    /// <param name="port">Port</param>
    /// <param name="protocol">Protocol</param>
    /// <returns>Result</returns>
    public static async Task<Result> Connect(string ip, int port, int protocol) {
        const string uuid = "be7b89d7-efed-452d-a716-4c0eec4c8e2d";
        const string name = "ServerOverflow";
        var client = new TcpClient();
        try {
            await client.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromSeconds(5));
            await using var stream = client.GetStream();
            stream.ReadTimeout = 5000;
            using var writer = new AsyncBinaryWriter(stream);
            using var reader = new AsyncBinaryReader(stream);
            using var packMemory = new MemoryStream();
            using var packWriter = new AsyncBinaryWriter(packMemory);
            
            // handshake packet
            await packWriter.WriteVarInt(0x00);     // Packet ID
            await packWriter.WriteVarInt(protocol); // Protocol Version
            await packWriter.WriteString(ip);       // Server IP
            await writer.WriteAsync((short)port);    // Server Port
            await packWriter.WriteVarInt(2);        // Next State
            await writer.WriteVarInt((int) packMemory.Position);
            await writer.WriteAsync(packMemory.ToArray(), 0, (int)packMemory.Position);
            packMemory.Position = 0;
            
            // login start packet
            await packWriter.WriteVarInt(0x00);     // Packet ID
            await packWriter.WriteString(name);     // Username
            if (protocol is 759 or 760)
                await writer.WriteAsync((byte)0x00); // Has signature
            if (protocol > 758) {
                var guidBytes = new Guid(uuid).ToByteArray();            
                var uuidBytes = new[] {
                    guidBytes[6], guidBytes[7], guidBytes[4], guidBytes[5],
                    guidBytes[0], guidBytes[1], guidBytes[2], guidBytes[3],
                    guidBytes[15], guidBytes[14], guidBytes[13], guidBytes[12],
                    guidBytes[11], guidBytes[10], guidBytes[9], guidBytes[8]
                };
                await writer.WriteAsync((byte)1);    // Has UUID
                await writer.WriteAsync(uuidBytes);  // UUID
            }
            await writer.WriteVarInt((int) packMemory.Position);
            await writer.WriteAsync(packMemory.ToArray(), 0, (int)packMemory.Position);
            await reader.ReadVarInt(); // ignore packet length
            return await reader.ReadVarInt() switch {
                0x00 => throw new InvalidDataException("Player was abruptly disconnected"),
                0x01 => new Result { Success = true, OnlineMode = true },
                _ => new Result { Success = true, OnlineMode = false }
            };
        } catch (Exception e) {
            return new Result { Success = false, ErrorMessage = e.Message };
        }
    }

    /// <summary>
    /// Main worker thread
    /// </summary>
    public static async Task WorkerThread() {
        while (true) {
            try {
                using var cursor = await Controller.Servers.FindAsync(
                    Builders<Server>.Filter.Exists(x => x.JoinResult, false) | 
                    Builders<Server>.Filter.Eq(x => x.JoinResult, null) | (
                        Builders<Server>.Filter.Eq(x => x.JoinResult!.Success, false) & 
                        Builders<Server>.Filter.Gt(x => x.JoinResult!.Timestamp, DateTime.UtcNow + TimeSpan.FromDays(3))
                    ), new FindOptions<Server> { BatchSize = 250 });
                
                while (await cursor.MoveNextAsync()) {
                    var watch = new Stopwatch(); watch.Start();
                    Parallel.ForEach(cursor.Current, async x => {
                        x.JoinResult = await Connect(x.IP,
                            x.Port, x.Ping.Version?.Protocol ?? 47);
                        await x.Update();
                    });
                    
                    watch.Stop(); var count = cursor.Current.Count();
                    Log.Information("Joined {0} servers in {1}", count, watch.Elapsed.Humanize());
                }
                
                await Task.Delay(3600000);
            } catch (Exception e) {
                Log.Error("Join bot thread crashed: {0}", e);
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
        /// When was the result produced
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}