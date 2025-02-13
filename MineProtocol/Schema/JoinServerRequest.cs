using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using MineProtocol.Authentication;

namespace MineProtocol.Schema;

/// <summary>
/// Minecraft join server request
/// </summary>t
public class JoinServerRequest {
    /// <summary>
    /// Profile access token
    /// </summary>
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
    
    /// <summary>
    /// Profile UUID without dashes
    /// </summary>
    [JsonPropertyName("selectedProfile")]
    public string UUID { get; set; }
    
    /// <summary>
    /// Custom SHA1 hash of ServerId + SharedSecret + PublicKey
    /// </summary>
    [JsonPropertyName("serverId")]
    public string JoinHash { get; set; }

    /// <summary>
    /// Creates a new join server request
    /// </summary>
    /// <param name="profile">Profile</param>
    /// <param name="serverId">Server ID</param>
    /// <param name="secret">Shared Secret</param>
    /// <param name="publicKey">Public Key</param>
    public JoinServerRequest(Profile profile, string serverId, byte[] secret, byte[] publicKey) {
        AccessToken = profile.Minecraft?.Token ?? ""; UUID = profile.UUID;
        JoinHash = Encoding.ASCII.GetBytes(serverId).Concat(secret).Concat(publicKey).ToArray().MinecraftDigest();
    }
}