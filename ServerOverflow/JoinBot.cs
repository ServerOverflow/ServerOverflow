using System.Net.Sockets;
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
            if (!client.ConnectAsync(ip, port).Wait(5000))
                throw new TimeoutException("Connect timed out");
            await using var stream = client.GetStream();
            stream.ReadTimeout = 5000;
            await using var writer = new BinaryWriter(stream);
            using var reader = new BinaryReader(stream);
            using var packMemory = new MemoryStream();
            await using var packWriter = new BinaryWriter(packMemory);
            
            // handshake packet
            packWriter.WriteVarInt(0x00);     // Packet ID
            packWriter.WriteVarInt(protocol); // Protocol Version
            packWriter.WriteString(ip);       // Server IP
            packWriter.Write((short)port);    // Server Port
            packWriter.WriteVarInt(2);        // Next State
            writer.WriteVarInt((int) packMemory.Position);
            writer.Write(packMemory.ToArray(), 0, (int)packMemory.Position);
            packMemory.Position = 0;
            
            // login start packet
            packWriter.WriteVarInt(0x00);     // Packet ID
            packWriter.WriteString(name);     // Username
            if (protocol is 759 or 760)
                packWriter.Write((byte)0x00); // Has signature
            if (protocol > 758) {
                var guidBytes = new Guid(uuid).ToByteArray();            
                var uuidBytes = new[] {
                    guidBytes[6], guidBytes[7], guidBytes[4], guidBytes[5],
                    guidBytes[0], guidBytes[1], guidBytes[2], guidBytes[3],
                    guidBytes[15], guidBytes[14], guidBytes[13], guidBytes[12],
                    guidBytes[11], guidBytes[10], guidBytes[9], guidBytes[8]
                };
                packWriter.Write((byte)1);    // Has UUID
                packWriter.Write(uuidBytes);  // UUID
            }
            writer.WriteVarInt((int) packMemory.Position);
            writer.Write(packMemory.ToArray(), 0, (int)packMemory.Position);
            reader.ReadVarInt(); // ignore packet length
            return reader.ReadVarInt() switch {
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
                    ), new FindOptions<Server> { BatchSize = 100 });
                while (await cursor.MoveNextAsync()) {
                    var tasks = cursor.Current.ToDictionary(x => x, 
                        x => Connect(x.IP, x.Port, x.Ping.Version?.Protocol ?? 47));
                    await Task.WhenAll(tasks.Values);
                    foreach (var (server, result) in tasks) {
                        server.JoinResult = result.Result;
                        await server.Update();
                    }
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