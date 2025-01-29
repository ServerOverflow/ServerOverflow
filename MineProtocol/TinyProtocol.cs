using System.Net.Sockets;

namespace MineProtocol;

/// <summary>
/// Tiny protocol implementations
/// </summary>
public static class TinyProtocol {
    /// <summary>
    /// Joins to a Minecraft server and returns the result
    /// </summary>
    /// <param name="ip">Server's IP</param>
    /// <param name="port">Server's port</param>
    /// <param name="protocol">Protocol version</param>
    /// <param name="forge">Perform forge handshake</param>
    /// <param name="timeoutSecs">Timeout in seconds</param>
    /// <returns>Result</returns>
    public static async Task<Result> Join(string ip, int port, int protocol = 47, bool forge = false, int timeoutSecs = 5) {
        const string uuid = "be7b89d7-efed-452d-a716-4c0eec4c8e2d";
        const string name = "ServerOverflow";
        var client = new TcpClient();
        try {
            var timeout = TimeSpan.FromSeconds(timeoutSecs);
            await client.ConnectAsync(ip, port).WaitAsync(timeout);
            await using var stream = client.GetStream();
            using var packet = new MemoryStream();
            
            // handshake packet
            ip += forge ? "\0FML\0" : "";
            await packet.WriteVarInt(0x00);         // Packet ID
            await packet.WriteVarInt(protocol);     // Protocol Version
            await packet.WriteString(ip);           // Server IP
            await packet.WriteShort((ushort)port);  // Server Port
            await packet.WriteVarInt(2);            // Next State
            await stream.WriteVarInt((int) packet.Position).WaitAsync(timeout);
            await stream.WriteAsync(packet.ToArray().AsMemory(0, (int)packet.Position)).AsTask().WaitAsync(timeout);
            packet.Position = 0;
            
            // login start packet
            await packet.WriteVarInt(0x00);    // Packet ID
            await packet.WriteString(name);    // Username
            if (protocol is >= 759 and <= 760)
                await packet.WriteBytes(0);    // Has signature
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
            return await stream.ReadVarInt().WaitAsync(timeout) switch {
                0x00 => new Result { Success = true, Whitelist = true, DisconnectReason = await stream.ReadString() },
                0x01 => new Result { Success = true, OnlineMode = true },
                _ => new Result { Success = true, OnlineMode = false }
            };
        } catch (Exception e) {
            return new Result { Success = false, ErrorMessage = e.Message };
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