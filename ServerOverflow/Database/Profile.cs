using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace ServerOverflow.Database;

/// <summary>
/// Minecraft profile
/// </summary>
public class Profile {
    /// <summary>
    /// Document's object identifier
    /// </summary>
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