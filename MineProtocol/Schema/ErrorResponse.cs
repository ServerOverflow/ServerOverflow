using System.Text.Json.Serialization;

namespace MineProtocol.Schema;

/// <summary>
/// Mojang API error response
/// </summary>
public class ErrorResponse {
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("path")]
    public string? Path { get; set; }
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}