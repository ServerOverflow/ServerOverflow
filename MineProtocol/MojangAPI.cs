using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
using MineProtocol.Authentication;
using MineProtocol.Schema;

namespace MineProtocol;

/// <summary>
/// Mojang API endpoints
/// </summary>
public static class MojangAPI {
    /// <summary>
    /// Generates server ID string for use with session API
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="secret">Shared Secret</param>
    /// <param name="publicKey">Public Key</param>
    /// <returns>Server ID</returns>
    public static string GetServerId(string serverId, byte[] secret, byte[] publicKey)
        => Encoding.ASCII.GetBytes(serverId).Concat(secret).Concat(publicKey).ToArray().MinecraftDigest();
    
    /// <summary>
    /// Sends a server join request
    /// </summary>
    /// <param name="profile">Profile</param>
    /// <param name="serverId">Server ID</param>
    /// <param name="proxy">Web Proxy</param>
    /// <returns>True on success</returns>
    public static async Task JoinServer(Profile profile, string serverId, WebProxy? proxy = null) {
        using var client = new HttpClient(new HttpClientHandler { Proxy = proxy });
        var req = new HttpRequestMessage {
            Content = JsonContent.Create(new JoinServerRequest(profile, serverId)),
            RequestUri = new Uri("https://sessionserver.mojang.com/session/minecraft/join"),
            Method = HttpMethod.Post
        };
        var res = await client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Attempting to login as {profile.Username} resulted in error code {res.StatusCode}");
    }

    /// <summary>
    /// Checks if specified player is authenticated
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="serverId">Server ID</param>
    /// <returns>Player information if true</returns>
    public static async Task<PlayerJoined?> HasJoined(string username, string serverId) {
        using var client = new HttpClient();
        username = HttpUtility.UrlEncode(username);
        serverId = HttpUtility.UrlEncode(serverId);
        var resp = await client.GetAsync($"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={username}&serverId=${serverId}");
        if (resp.StatusCode == HttpStatusCode.NoContent || resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<PlayerJoined>();
    }

    /// <summary>
    /// Player joined response
    /// </summary>
    public class PlayerJoined {
        [JsonPropertyName("id")] 
        public string Uuid { get; set; } = "";
        
        [JsonPropertyName("name")] 
        public string Username { get; set; } = "";

        [JsonPropertyName("properties")]
        public Property[] Properties { get; set; } = [];
        
        public class Property {
            [JsonPropertyName("name")] 
            public string Name { get; set; } = "";
            
            [JsonPropertyName("value")] 
            public string Value { get; set; } = "";
            
            [JsonPropertyName("signature")] 
            public string Signature { get; set; } = "";
        }
    }
}