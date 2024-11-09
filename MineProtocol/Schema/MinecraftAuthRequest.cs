using System.Text.Json.Serialization;

namespace MineProtocol.Schema;

/// <summary>
/// Minecraft authentication request
/// </summary>
public class MinecraftAuthRequest {
    /// <summary>
    /// XSTS identity token
    /// </summary>
    [JsonPropertyName("identityToken")] 
    public string IdentityToken { get; }

    /// <summary>
    /// Creates a new minecraft authentication request
    /// </summary>
    /// <param name="userHash">User Hash</param>
    /// <param name="token">XSTS token</param>
    public MinecraftAuthRequest(string userHash, string token)
        => IdentityToken = $"XBL3.0 x={userHash};{token}";
}