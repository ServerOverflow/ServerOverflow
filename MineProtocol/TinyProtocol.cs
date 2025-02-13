using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using MineProtocol.Authentication;
using MineProtocol.Exceptions;

namespace MineProtocol;

/// <summary>
/// Tiny minecraft protocol implementation
/// </summary>
public class TinyProtocol : IDisposable {
    /// <summary>
    /// How long to wait for I/O operations until timing out
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState State { get; private set; } = ConnectionState.Handshake;
    
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
    /// Forge protocol version
    /// </summary>
    public int ForgeProtocol { get; init; }

    /// <summary>
    /// TCP client connection
    /// </summary>
    private readonly TcpClient _client = new();
    
    /// <summary>
    /// Serverbound network stream
    /// </summary>
    private Stream? _input;
    
    /// <summary>
    /// Clientbound network stream
    /// </summary>
    private Stream? _output;

    /// <summary>
    /// Compression threshold
    /// </summary>
    private int _compressThreshold = -1;

    /// <summary>
    /// Creates a new TinyProtocol connection
    /// </summary>
    /// <param name="ip">Server IP</param>
    /// <param name="port">Server port</param>
    /// <param name="protocol">Protocol version</param>
    /// <param name="forge">Is Forge enabled</param>
    /// <param name="fmlProtocol">FML protocol version</param>
    public TinyProtocol(string ip, int port, int protocol = 47, bool forge = false, int fmlProtocol = 0)
        : this(new IPEndPoint(IPAddress.Parse(ip), port), protocol, forge, fmlProtocol) { }

    /// <summary>
    /// Creates a new TinyProtocol connection
    /// </summary>
    /// <param name="endpoint">Destination endpoint</param>
    /// <param name="protocol">Protocol version</param>
    /// <param name="forge">Is Forge enabled</param>
    /// <param name="fmlProtocol">FML protocol version</param>
    public TinyProtocol(IPEndPoint endpoint, int protocol = 47, bool forge = false, int fmlProtocol = 0) {
        EndPoint = endpoint; Protocol = protocol; Forge = forge; ForgeProtocol = fmlProtocol;
    }
    
    /// <summary>
    /// Connects to the server
    /// </summary>
    public async Task Connect() {
        await _client.ConnectAsync(EndPoint).WaitAsync(Timeout);
        _input = _output = _client.GetStream();
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
        if (_input == null) throw new InvalidOperationException("Connect to the server first");
        if (State != ConnectionState.Handshake) throw new InvalidOperationException($"Invalid connection state {State}");
        var ip = EndPoint.Address + (Forge ? $"\0FML{ForgeProtocol}\0FML\0FORGE{ForgeProtocol}\0" : "");
        var port = EndPoint.Port;
        
        using var packet = new MemoryStream();
        await packet.WriteVarInt((int)PacketId.Handshake); // Packet ID
        await packet.WriteVarInt(Protocol);                // Protocol Version
        await packet.WriteString(ip);                      // Server IP
        await packet.WriteShort((ushort)port);             // Server Port
        await packet.WriteVarInt(slp ? 1 : 2);             // Next State
        await Send(packet);
        State = ConnectionState.Login;
    }

    /// <summary>
    /// Performs login and proceeds to play state
    /// </summary>
    /// <param name="profile">Login</param>
    public async Task Login(Profile profile) {
        if (State != ConnectionState.Login) throw new InvalidOperationException($"Invalid connection state {State}");
        await LoginStart(profile);
        Packet? packet = null;
        while (packet == null)
            packet = await Receive();
        if (packet.Id == PacketId.EncryptionRequest) {
            // TODO: handle encryption request
        }
    }
    
    /// <summary>
    /// Sends login start packet
    /// </summary>
    /// <param name="profile">Profile</param>
    public async Task LoginStart(Profile profile) {
        if (State != ConnectionState.Login) throw new InvalidOperationException($"Invalid connection state {State}");
        if (_input == null) throw new InvalidOperationException("Connect to the server first");
        using var packet = new MemoryStream();
        await packet.WriteVarInt((int)PacketId.LoginStart); // Packet ID
        await packet.WriteString(profile.Username);         // Username
        if (Protocol is >= 759 and <= 760)
            await packet.WriteBytes(0);                     // Has signature
        if (Protocol >= 760) {
            var guidBytes = new Guid(profile.UUID).ToByteArray();            
            var uuidBytes = new[] {
                guidBytes[6], guidBytes[7], guidBytes[4], guidBytes[5],
                guidBytes[0], guidBytes[1], guidBytes[2], guidBytes[3],
                guidBytes[15], guidBytes[14], guidBytes[13], guidBytes[12],
                guidBytes[11], guidBytes[10], guidBytes[9], guidBytes[8]
            };
            if (Protocol <= 763)
                await packet.WriteBytes(1);                 // Has UUID
            await packet.WriteBytes(uuidBytes);             // UUID
        }

        await Send(packet);
    }

    /// <summary>
    /// Enables encryption and sends encryption response packet
    /// </summary>
    /// <param name="secretKey">Secret Key (16)</param>
    /// <param name="publicKey">Public Key</param>
    /// <param name="verifyKey">Verify Key</param>
    public async Task Encrypt(byte[] secretKey, byte[] publicKey, byte[] verifyKey) {
        if (_input == null || _output == null) throw new InvalidOperationException("Connect to the server first");
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
        var encSecret = rsa.Encrypt(secretKey, RSAEncryptionPadding.Pkcs1);
        var encVerify = rsa.Encrypt(verifyKey, RSAEncryptionPadding.Pkcs1);
        
        using var packet = new MemoryStream();
        await packet.WriteVarInt((int)PacketId.EncryptionResponse); // Packet ID
        await packet.WriteVarInt(encSecret.Length);                 // Shared secret length
        packet.Write(encSecret, 0, encSecret.Length);               // Shared secret
        await packet.WriteVarInt(encVerify.Length);                 // Verify key length
        packet.Write(encVerify, 0, encVerify.Length);               // Verify key
        await Send(packet);
        
        using var aes = Aes.Create();
        aes.Key = secretKey;
        aes.IV = secretKey;
        aes.Mode = CipherMode.CFB;
        aes.Padding = PaddingMode.None;
        aes.FeedbackSize = 8;
        _input = new CryptoStream(_input, aes.CreateEncryptor(), CryptoStreamMode.Write);
        _output = new CryptoStream(_output, aes.CreateDecryptor(), CryptoStreamMode.Read);
    }

    /// <summary>
    /// Acknowledges login success
    /// </summary>
    public async Task LoginAck() {
        using var packet = new MemoryStream();
        await packet.WriteVarInt((int)PacketId.LoginAcknowledged); // Packet ID
        await Send(packet);
    }

    /// <summary>
    /// Respond to a plugin request during login
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <param name="data">Payload</param>
    public async Task LoginPluginResponse(int id, byte[]? data = null) {
        if (_input == null) throw new InvalidOperationException("Connect to the server first");
        if (State != ConnectionState.Login) throw new InvalidOperationException($"Invalid connection state {State}");

        using var packet = new MemoryStream();
        await packet.WriteVarInt((int)PacketId.LoginPluginResponse); // Packet ID
        await packet.WriteVarInt(id);             // Message ID
        await packet.WriteBoolean(data != null);  // Has payload
        if (data != null)
            await packet.WriteAsync(data);      // Payload
        await Send(packet);
    }

    /// <summary>
    /// Respond to a cookie request
    /// </summary>
    /// <param name="key">Identifier</param>
    /// <param name="data">Payload</param>
    public async Task CookieResponse(string key, byte[]? data = null) {
        if (_input == null) throw new InvalidOperationException("Connect to the server first");
        if (State != ConnectionState.Login) throw new InvalidOperationException($"Invalid connection state {State}");

        using var packet = new MemoryStream();
        await packet.WriteVarInt((int)PacketId.CookieResponse); // Packet ID
        await packet.WriteString(key);            // Key identifier
        await packet.WriteBoolean(data != null);  // Has payload
        if (data != null)
            await packet.WriteAsync(data);      // Payload
        await Send(packet);
    }
    
    /// <summary>
    /// Sends an arbitrary packet
    /// </summary>
    /// <param name="packet">Packet</param>
    public async Task Send(MemoryStream packet) {
        if (_input == null) throw new InvalidOperationException("Connect to the server first");
        int? uncompressed = null;
        if (_compressThreshold >= 0)
            if (packet.Length >= _compressThreshold) {
                await using var stream = new ZLibStream(packet, CompressionLevel.Fastest);
                packet = new MemoryStream();
                await stream.CopyToAsync(packet);
                uncompressed = (int)packet.Position;
            } else uncompressed = 0;

        await _input.WriteVarInt((int)packet.Position).WaitAsync(Timeout);        // Packet length
        if (uncompressed.HasValue)
            await _input.WriteVarInt(uncompressed.Value).WaitAsync(Timeout);      // Uncompressed length
        await _input.WriteAsync(packet.ToArray()).AsTask().WaitAsync(Timeout);  // Packet data
    }
    
    /// <summary>
    /// Receives a packet and returns the stream.
    /// Some packets during the login stage are automatically processed.
    /// </summary>
    /// <returns>Packet</returns>
    public async Task<Packet> Receive() {
        if (_output == null) throw new InvalidOperationException("Connect to the server first");
        var length = await _output.ReadVarInt(Timeout);
        
        var stream = _output;
        if (_compressThreshold >= 0) {
            var compressed = await _output.ReadVarInt(Timeout);
            if (compressed != 0) {
                stream = new ZLibStream(stream, CompressionMode.Decompress);
                length = compressed;
            }
        }
        
        var id = (PacketId)await stream.ReadVarInt(Timeout);
        length -= 1;
        if (length < 0)
            throw new InvalidOperationException($"Invalid packet payload length of {length}");
        var buf = new byte[length];
        if (length >= 2097152)
            throw new InvalidOperationException($"Payload of size {length} is too large");
        await stream.ReadExactlyAsync(buf, 0, buf.Length).AsTask().WaitAsync(Timeout);
        var packet = new Packet(this, new MemoryStream(buf), length, id);

        if (State == ConnectionState.Login) {
            if (packet.Id == PacketId.SetCompression) {
                _compressThreshold = await packet.Stream.ReadVarInt();
                await packet.Skip();
            }

            if (packet.Id == PacketId.LoginSuccess && Protocol >= 764) {
                await packet.Skip();
                await LoginAck();
            }

            if (packet.Id == PacketId.LoginPluginRequest) {
                await HandlePluginRequest(packet);
            }
            
            if (packet.Id == PacketId.CookieRequest) {
                var key = await packet.Stream.ReadString();
                await packet.Skip();
                await CookieResponse(key);
            }

            if (packet.Id == PacketId.Disconnect) {
                var raw = await packet.Stream.ReadString();
                Disconnect(); throw new DisconnectedException(raw);
            }
        }

        return packet;
    }

    /// <summary>
    /// Handles plugin request (mostly to support modern FML)
    /// </summary>
    /// <param name="packet"></param>
    private async Task HandlePluginRequest(Packet packet) {
        var msgId = await packet.Stream.ReadVarInt();
        var channel = await packet.Stream.ReadString();
        switch (channel) {
            case "fml:loginwrapper":
                var pluginChannel = await packet.Stream.ReadString();
                if (pluginChannel != "fml:handshake") {
                    await packet.Skip();
                    await LoginPluginResponse(msgId);
                    return;
                }

                // we don't care about the length
                _ = await packet.Stream.ReadVarInt();
                var id = await packet.Stream.ReadVarInt();
                switch (id) {
                    case 1: { // Mod list
                        using var payload = new MemoryStream();
                        await payload.WriteVarInt(2);
                        
                        var modCount = await packet.Stream.ReadVarInt();
                        await payload.WriteVarInt(modCount);
                        for (var i = 0; i < modCount; i++) {
                            var name = await packet.Stream.ReadString();
                            await payload.WriteString(name);
                        }
                        
                        var channelCount = await packet.Stream.ReadVarInt();
                        await payload.WriteVarInt(channelCount);
                        for (var i = 0; i < channelCount; i++) {
                            var name = await packet.Stream.ReadString();
                            var marker = await packet.Stream.ReadString();
                            await payload.WriteString(name);
                            await payload.WriteString(marker);
                        }
                        
                        var registryCount = await packet.Stream.ReadVarInt();
                        await payload.WriteVarInt(registryCount);
                        for (var i = 0; i < registryCount; i++) {
                            var name = await packet.Stream.ReadString();
                            await payload.WriteString(name);
                            await payload.WriteString("");
                        }
                        
                        var buf = await WrapForgePayload(payload);
                        await LoginPluginResponse(msgId, buf);
                        return;
                    } 
                    case 3:   // Server registry
                    case 4: { // Configuration data
                        await packet.Skip();
                        using var payload = new MemoryStream();
                        await payload.WriteVarInt(99);
                        
                        var buf = await WrapForgePayload(payload);
                        await LoginPluginResponse(msgId, buf);
                        return;
                    }
                }
                
                await packet.Skip();
                await LoginPluginResponse(msgId);
                return;
            default:
                await packet.Skip();
                await LoginPluginResponse(msgId);
                return;
        }
    }
    
    /// <summary>
    /// Wraps FML plugin payload
    /// </summary>
    /// <param name="packet">Packet</param>
    public async Task<byte[]> WrapForgePayload(MemoryStream packet) {
        using var wrapper = new MemoryStream();
        await wrapper.WriteString("fml:handshake");
        await wrapper.WriteVarInt((int)packet.Length);
        wrapper.Write(packet.ToArray());
        return wrapper.ToArray();
    }
    
    /// <summary>
    /// Releases all unmanaged resources
    /// </summary>
    public void Dispose() {
        try {
            Disconnect(); 
            _input?.Dispose();
            if (_output != _input)
                _output?.Dispose();
            _client.Dispose();
        } catch { /* Ignore */ }
    }

    /// <summary>
    /// Incoming packet info
    /// </summary>
    public class Packet {
        /// <summary>
        /// Tiny protocol instance
        /// </summary>
        public TinyProtocol Parent { get; set; }
        
        /// <summary>
        /// Payload stream
        /// </summary>
        public MemoryStream Stream { get; set; }
        
        /// <summary>
        /// Packet length
        /// </summary>
        public int Length { get; set; }
        
        /// <summary>
        /// Packet ID
        /// </summary>
        public PacketId Id { get; set; }

        /// <summary>
        /// Was this packet handled internally
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Skips the payload
        /// </summary>
        public async Task Skip() {
            if (Handled) return;
            var buf = new byte[Length - Stream.Position];
            await Stream.ReadExactlyAsync(buf, 0, buf.Length).AsTask().WaitAsync(Parent.Timeout);
            Handled = true;
        }

        /// <summary>
        /// Creates a new packet
        /// </summary>
        /// <param name="proto">Tiny Protocol</param>
        /// <param name="stream">Stream</param>
        /// <param name="length">Length</param>
        /// <param name="id">Packet ID</param>
        public Packet(TinyProtocol proto, MemoryStream stream, int length, PacketId id) {
            Parent = proto; Stream = stream; Length = length; Id = id;
        }
    }

    /// <summary>
    /// Common packet IDs
    /// </summary>
    public enum PacketId {
        // Handshake (serverbound)
        Handshake = 0x00,
        
        // Login (clientbound)
        Disconnect = 0x00,
        EncryptionRequest = 0x01,
        LoginSuccess = 0x02,
        SetCompression = 0x03,
        LoginPluginRequest = 0x04,
        CookieRequest = 0x05,
        
        // Login (serverbound)
        LoginStart = 0x00,
        EncryptionResponse = 0x01,
        LoginAcknowledged = 0x03,
        LoginPluginResponse = 0x02,
        CookieResponse = 0x04
    }

    /// <summary>
    /// Connection states
    /// </summary>
    public enum ConnectionState {
        Handshake,
        Login,
        Configure,
        Play
    }
}