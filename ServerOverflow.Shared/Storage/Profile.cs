using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ServerOverflow.Shared.Converters;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// Minecraft profile
/// </summary>
public class Profile {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    [BsonId] public ObjectId Id { get; set; }

    /// <summary>
    /// Underlying profile instance
    /// </summary>
    public MineProtocol.Authentication.Profile Instance { get; set; } = null!;
    
    /// <summary>
    /// Is the account still valid
    /// </summary>
    public bool Valid { get; set; }
    
    /// <summary>
    /// Fetches a profile by its identifier
    /// </summary>
    /// <param name="id">Object identifier</param>
    /// <returns>Profile, may be null</returns>
    public static async Task<Profile?> Get(string id)
        => await Database.Profiles.QueryFirst(x => x.Id.ToString() == id);
    
    /// <summary>
    /// Fetches all profiles
    /// </summary>
    /// <returns>List of profiles</returns>
    public static async Task<List<Profile>> GetAll()
        => await Database.Profiles.QueryAll(x => true);
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Profile>.Filter.Eq(x => x.Id, Id);
        await Database.Profiles.ReplaceOneAsync(filter, this);
    }
}