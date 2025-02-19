namespace MineProtocol.Protocol;

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