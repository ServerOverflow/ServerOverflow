using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MineProtocol.Schema;

namespace MineProtocol.Authentication; 

/// <summary>
/// Minecraft account
/// </summary>
public class Profile {
    /// <summary>
    /// Microsoft token pair
    /// </summary>
    public TokenPair? Microsoft { get; set; }

    /// <summary>
    /// The Minecraft access token
    /// </summary>
    public GenericToken? Minecraft { get; set; }

    /// <summary>
    /// Username of the account
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// UUID of the account
    /// </summary>
    public string UUID { get; set; } = null!;

    /// <summary>
    /// Validates the current profile and refreshes tokens if necessary.
    /// Will throw an exception in case a validation error occurs.
    /// </summary>
    public async Task Refresh() {
        if (Microsoft == null) return;
        if (Microsoft.RefreshToken!.ExpireAt < DateTime.UtcNow)
            throw new InvalidDataException("Refresh token has expired");
        if (Minecraft?.ExpireAt > DateTime.UtcNow) return;
        if (Microsoft.AccessToken!.ExpireAt < DateTime.UtcNow)
            await OAuth2.Refresh(Microsoft);
        using var client = new HttpClient();
        var res = await client.SendAsync(
            new HttpRequestMessage {
                RequestUri = new Uri("https://user.auth.xboxlive.com/user/authenticate"),
                Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
                Content = JsonContent.Create(new XboxAuthRequest(
                        Microsoft.AccessToken!.Token), options:
                    new JsonSerializerOptions { IncludeFields = true }),
                Method = HttpMethod.Post
            });
        if (!res.IsSuccessStatusCode)
            throw new InvalidDataException("Failed to obtain XBOX token");
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("Token").GetString()!;
        res = await client.SendAsync(
            new HttpRequestMessage {
                RequestUri = new Uri("https://xsts.auth.xboxlive.com/xsts/authorize"),
                Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
                Content = JsonContent.Create(new XstsAuthRequest(token),
                    options: new JsonSerializerOptions { IncludeFields = true }),
                Method = HttpMethod.Post
            });
        if (!res.IsSuccessStatusCode)
            throw new InvalidDataException("Failed to obtain XSTS token");
        json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var hash = json.RootElement.GetProperty("DisplayClaims")
            .GetProperty("xui")[0].GetProperty("uhs").GetString()!;
        token = json.RootElement.GetProperty("Token").GetString()!;
        res = await client.SendAsync(
            new HttpRequestMessage {
                RequestUri = new Uri("https://api.minecraftservices.com/authentication/login_with_xbox"),
                Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
                Content = JsonContent.Create(new MinecraftAuthRequest(hash, token),
                    options: new JsonSerializerOptions { IncludeFields = true }),
                Method = HttpMethod.Post
            });
        if (!res.IsSuccessStatusCode)
            throw new InvalidDataException("Failed to authenticate Minecraft user");
        json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        Minecraft = new GenericToken(
            json.RootElement.GetProperty("access_token").GetString()!,
            DateTime.UtcNow + TimeSpan.FromSeconds(json.RootElement.GetProperty("expires_in").GetInt32()));
        res = await client.SendAsync(
            new HttpRequestMessage {
                RequestUri = new Uri("https://api.minecraftservices.com/minecraft/profile"),
                Headers = {
                    Authorization = new AuthenticationHeaderValue("Bearer", Minecraft.Token),
                    Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
                },
                Method = HttpMethod.Get
            });
        json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        if (!res.IsSuccessStatusCode)
            throw new InvalidDataException("Failed to verify ownership of Minecraft");
        Username = json.RootElement.GetProperty("name").GetString()!;
        UUID = json.RootElement.GetProperty("id").GetString()!;
    }

    /// <summary>
    /// Sends a server join request
    /// </summary>
    /// <param name="serverId">Server ID</param>
    /// <param name="secret">Shared Secret</param>
    /// <param name="publicKey">Public Key</param>
    /// <returns>True on success</returns>
    public async Task Join(string serverId, byte[] secret, byte[] publicKey) {
        using var client = new HttpClient();
        var req = new HttpRequestMessage {
            Content = JsonContent.Create(new JoinServerRequest(this, serverId, secret, publicKey)),
            RequestUri = new Uri("https://sessionserver.mojang.com/session/minecraft/join"),
            Method = HttpMethod.Post
        };
        var rng = new Random();
        req.Headers.Add("X-ForWarDed-For", $"{rng.Next(1, 255)}.{rng.Next(1, 255)}.{rng.Next(1, 255)}.{rng.Next(1, 255)}");
        var res = await client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException("You have been ratelimited");
    }

    /// <summary>
    /// Creates a new Minecraft profile
    /// </summary>
    /// <param name="microsoft">Microsoft Token Pair</param>
    public Profile(TokenPair microsoft) {
        Microsoft = microsoft;
        Refresh().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Creates an offline profile
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="uuid">UUID</param>
    public Profile(string username, string uuid) {
        Username = username; UUID = uuid;
    }

    /// <summary>
    /// Creates an empty profile
    /// </summary>
    public Profile() { }
}