using System.Text.Json.Serialization;

namespace MineProtocol.Schema;

/// <summary>
/// Device authentication response
/// </summary>
public class DeviceAuthResponse {
    /// <summary>
    /// A long string used to verify the session between the client and the authorization server.
    /// The client uses this parameter to request the access token from the authorization server.
    /// </summary>
    [JsonPropertyName("device_code")]
    public string? DeviceCode { get; set; }
    
    /// <summary>
    /// A short string shown to the user used to identify the session on a secondary device.
    /// </summary>
    [JsonPropertyName("user_code")]
    public string? UserCode { get; set; }
    
    /// <summary>
    /// The URI the user should go to with the user_code in order to sign in.
    /// </summary>
    [JsonPropertyName("verification_uri")]
    public string? VerificationUri { get; set; }
    
    /// <summary>
    /// The number of seconds before the device_code and user_code expire.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
    
    /// <summary>
    /// The number of seconds the client should wait between polling requests.
    /// </summary>
    [JsonPropertyName("interval")]
    public int? Interval { get; set; }
    
    /// <summary>
    /// A human-readable string with instructions for the user.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}