using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using MineProtocol.Authentication;

namespace MineProtocol;

/// <summary>
/// Tiny minecraft protocol implementation.
/// </summary>
public class TinyProtocol : IDisposable {
    /// <summary>
    /// How long to wait for I/O operations until timing out
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Destination endpoint
    /// </summary>
    public IPEndPoint EndPoint { get; init; }
    
    /// <summary>
    /// Protocol version
    /// </summary>
    public int Protocol { get; init; }
    
    /// <summary>
    /// Is Forge enabled
    /// </summary>
    public bool Forge { get; init; }

    /// <summary>
    /// TCP client connection
    /// </summary>
    private readonly TcpClient _client = new();
    
    /// <summary>
    /// Underlying TCP network stream
    /// </summary>
    private NetworkStream? _stream;

    /// <summary>
    /// Compression threshold
    /// </summary>
    private int? _compressThreshold;

    /// <summary>
    /// Creates a new TinyProtocol connection
    /// </summary>
    /// <param name="ip">Server IP</param>
    /// <param name="port">Server port</param>
    /// <param name="protocol">Protocol version</param>
    /// <param name="forge">Is Forge enabled</param>
    public TinyProtocol(string ip, int port, int protocol = 47, bool forge = false)
        : this(new IPEndPoint(IPAddress.Parse(ip), port), protocol, forge) { }

    /// <summary>
    /// Creates a new TinyProtocol connection
    /// </summary>
    /// <param name="endpoint">Destination endpoint</param>
    /// <param name="protocol">Protocol version</param>
    /// <param name="forge">Is Forge enabled</param>
    public TinyProtocol(IPEndPoint endpoint, int protocol = 47, bool forge = false) {
        EndPoint = endpoint; Protocol = protocol; Forge = forge;
    }

    /// <summary>
    /// Connects to the server
    /// </summary>r
    public async Task Connect() {
        await _client.ConnectAsync(EndPoint).WaitAsync(Timeout);
        _stream = _client.GetStream();
    }

    /// <summary>
    /// Disconnects from the server
    /// </summary>
    public void Disconnect()
        => _client.Close();
    
    /// <summary>
    /// Sends handshake packet
    /// </summary>
    /// <param name="slp">Perform SLP</param>
    public async Task Handshake(bool slp = false) {
        if (_stream == null) throw new InvalidOperationException("Connect to the server first");
        var ip = EndPoint.Address + (Forge ? "\0FML\0" : "");
        var port = EndPoint.Port;
        
        using var packet = new MemoryStream();
        await packet.WriteVarInt(0x00);         // Packet ID
        await packet.WriteVarInt(Protocol);     // Protocol Version
        await packet.WriteString(ip);           // Server IP
        await packet.WriteShort((ushort)port);  // Server Port
        await packet.WriteVarInt(slp ? 1 : 2);  // Next State
        await Send(packet);
    }

    /// <summary>
    /// Sends login start packet
    /// </summary>
    /// <param name="profile">Profile</param>
    public async Task LoginStart(Profile profile) {
        if (_stream == null) throw new InvalidOperationException("Connect to the server first");
        using var packet = new MemoryStream();
        await packet.WriteVarInt(0x00);              // Packet ID
        await packet.WriteString(profile.Username);  // Username
        if (Protocol is >= 759 and <= 760)
            await packet.WriteBytes(0);              // Has signature
        if (Protocol >= 760) {
            var guidBytes = new Guid(profile.UUID).ToByteArray();            
            var uuidBytes = new[] {
                guidBytes[6], guidBytes[7], guidBytes[4], guidBytes[5],
                guidBytes[0], guidBytes[1], guidBytes[2], guidBytes[3],
                guidBytes[15], guidBytes[14], guidBytes[13], guidBytes[12],
                guidBytes[11], guidBytes[10], guidBytes[9], guidBytes[8]
            };
            if (Protocol <= 763)
                await packet.WriteBytes(1);          // Has UUID
            await packet.WriteBytes(uuidBytes);      // UUID
        }

        await Send(packet);
    }

    /// <summary>
    /// Sends an arbitrary packet
    /// </summary>
    /// <param name="packet">Packet</param>
    public async Task Send(MemoryStream packet) {
        if (_stream == null) throw new InvalidOperationException("Connect to the server first");
        int? uncompressed = null;
        if (_compressThreshold.HasValue)
            if (packet.Length >= _compressThreshold) {
                await using var stream = new ZLibStream(packet, CompressionLevel.Fastest);
                packet = new MemoryStream();
                await stream.CopyToAsync(packet);
                uncompressed = (int)packet.Position;
            } else uncompressed = 0;
        
        await _stream.WriteVarInt((int)packet.Position).WaitAsync(Timeout);
        if (uncompressed.HasValue)
            await _stream.WriteVarInt(uncompressed.Value).WaitAsync(Timeout);
        await _stream.WriteAsync(packet.ToArray()).AsTask().WaitAsync(Timeout);
    }

    /// <summary>
    /// Receives a packet
    /// </summary>
    /// <returns>Packet</returns>
    public async Task<Packet> Receive() {
        if (_stream == null) throw new InvalidOperationException("Connect to the server first");
        var length = await _stream.ReadVarInt().WaitAsync(Timeout);
        
        Stream stream = _stream;
        if (_compressThreshold.HasValue) {
            var compressed = await _stream.ReadVarInt().WaitAsync(Timeout);
            if (compressed != 0) {
                stream = new ZLibStream(stream, CompressionMode.Decompress);
                length = compressed;
            }
        }

        var id = await stream.ReadVarInt().WaitAsync(Timeout);
        if (id == 0x03) _compressThreshold = await stream.ReadVarInt().WaitAsync(Timeout);
        return new Packet(stream, length, id);
    }
    
    /// <summary>
    /// Releases all unmanaged resources
    /// </summary>
    public void Dispose() {
        Disconnect();
        _stream?.Dispose();
        _client.Dispose();
    }

    /// <summary>
    /// Incoming packet info
    /// </summary>
    public class Packet {
        /// <summary>
        /// Payload stream
        /// </summary>
        public Stream Stream { get; set; }
        
        /// <summary>
        /// Packet length
        /// </summary>
        public int Length { get; set; }
        
        /// <summary>
        /// Packet ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Creates a new packet
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="length">Length</param>
        /// <param name="id">Packet ID</param>
        public Packet(Stream stream, int length, int id) {
            Stream = stream; Length = length; Id = id;
        }
    }
}