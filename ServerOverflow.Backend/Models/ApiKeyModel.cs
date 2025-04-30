using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Models;

/// <summary>
/// API key model
/// </summary>
public class ApiKeyModel {
    /// <summary>
    /// API key identifier
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// API key value
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Expiration date
    /// </summary>
    public DateTime? ExpireAt { get; set; }
    
    /// <summary>
    /// Permissions granted
    /// </summary>
    public List<Permission>? Permissions { get; set; }
    
    /// <summary>
    /// Empty constructor for JSON
    /// </summary>
    public ApiKeyModel() {}

    /// <summary>
    /// Creates a new API key model
    /// </summary>
    /// <param name="key">API key</param>
    /// <param name="includeKey">Include key</param>
    public ApiKeyModel(ApiKey key, bool includeKey = false) {
        Permissions = key.Permissions;
        ExpireAt = key.ExpireAt;
        Id = key.Id.ToString();
        Name = key.Name;
        if (includeKey)
            Key = key.Key;
    }
}