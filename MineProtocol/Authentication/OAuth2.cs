using System.Text.Json;

namespace MineProtocol.Authentication;

/// <summary>
/// Barebones microsoft OAuth2 implementation
/// </summary>
public static class OAuth2 {
    /// <summary>
    /// Static HTTP client instance
    /// </summary>
    private static readonly HttpClient _client = new();

    /// <summary>
    /// OAuth2 client ID
    /// </summary>
    private const string _clientId = "c36a9fb6-4f2a-41ff-90bd-ae7cc92031eb";
    
    /// <summary>
    /// Returns a token pair for an OAuth2 access token
    /// </summary>
    /// <param name="code">Access Token</param>
    /// <returns>Token pair</returns>
    public static async Task<TokenPair> GetAccessToken(string code) {
        var res = await _client.PostAsync(
            "https://login.microsoftonline.com/consumers/oauth2/v2.0/token",
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("code", code)
            ]));
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        if (json.RootElement.TryGetProperty("error", out var error))
            throw new Exception($"Failed to get Microsoft access token: {error}");
        return new TokenPair {
            RefreshToken = new GenericToken(
                json.RootElement.GetProperty("refresh_token").GetString()!, 
                DateTime.UtcNow + TimeSpan.FromDays(90)),
            AccessToken = new GenericToken(
                json.RootElement.GetProperty("access_token").GetString()!, 
                DateTime.UtcNow + TimeSpan.FromSeconds(json.RootElement.GetProperty("expires_in").GetInt32()))
        };
    }

    /// <summary>
    /// Refreshes an existing token pair
    /// </summary>
    /// <param name="pair">Token Pair</param>
    public static async Task Refresh(TokenPair pair) {
        var res = await _client.PostAsync(
            "https://login.microsoftonline.com/consumers/oauth2/v2.0/token",
            new FormUrlEncodedContent(new [] {
                new KeyValuePair<string, string>("refresh_token", pair.RefreshToken.Token),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _clientId)
            }));
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        if (json.RootElement.TryGetProperty("error", out var error))
            throw new Exception($"Failed to refresh token pair: {error}");
        pair.RefreshToken = new GenericToken(
            json.RootElement.GetProperty("refresh_token").GetString()!,
            DateTime.UtcNow + TimeSpan.FromDays(90));
        pair.AccessToken = new GenericToken(json.RootElement.GetProperty("access_token").GetString()!,
            DateTime.UtcNow + TimeSpan.FromSeconds(json.RootElement.GetProperty("expires_in").GetInt32()));
    }
}