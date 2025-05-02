using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// API key owned by an account
/// </summary>
public class ApiKey {
    /// <summary>
    /// API key identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// API key value
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Expiration date
    /// </summary>
    public DateTime? ExpireAt { get; set; }
    
    /// <summary>
    /// API key owner
    /// </summary>
    public ObjectId Owner { get; set; }
    
    /// <summary>
    /// Permissions granted
    /// </summary>
    public List<Permission> Permissions { get; set; } = [];

    /// <summary>
    /// Checks if the API key has a permission
    /// </summary>
    /// <returns>True if API key has permission</returns>
    public bool HasPermission(Permission perm)
        => Permissions.Contains(perm);
    
    /// <summary>
    /// Fetches an API key by its value
    /// </summary>
    /// <param name="key">API key</param>
    /// <returns>API key, may be null</returns>
    public static async Task<ApiKey?> GetByKey(string key)
        => await Database.ApiKeys.QueryFirst(x => x.Key == key);
    
    /// <summary>
    ///Fetches an API key by its identifier
    /// </summary>
    /// <param name="id">Object identifier</param>
    /// <returns>API key, may be null</returns>
    public static async Task<ApiKey?> Get(ObjectId id)
        => await Database.ApiKeys.QueryFirst(x => x.Id == id);
    
    /// <summary>
    /// FFetches an API key by its identifier
    /// </summary>
    /// <param name="id">Object identifier</param>
    /// <returns>API key, may be null</returns>
    public static async Task<ApiKey?> Get(string id)
        => await Database.ApiKeys.QueryFirst(x => x.Id.ToString() == id);

    /// <summary>
    /// Creates a new API key
    /// </summary>
    /// <param name="account">Account</param>
    /// <param name="name">Display name</param>
    /// <param name="perms">Permissions</param>
    /// <param name="expireAt">Expiration date</param>
    /// <returns>API key</returns>
    public static async Task<ApiKey> Create(Account account, string name, List<Permission> perms, DateTime expireAt) {
        var key = new ApiKey {
            Key = Extensions.RandomString(32),
            ExpireAt = expireAt,
            Permissions = perms,
            Owner = account.Id,
            Name = name
        };
        
        await Database.ApiKeys.InsertOneAsync(key);
        return key;
    }
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<ApiKey>.Filter.Eq(x => x.Id, Id);
        await Database.ApiKeys.ReplaceOneAsync(filter, this);
    }
}