using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace ServerOverflow.Database;

/// <summary>
/// Minecraft profile
/// </summary>
public class Profile : MineProtocol.Authentication.Profile {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// Is the account still valid
    /// </summary>
    public bool Valid { get; set; }
    
    /// <summary>
    /// Fetches all profiles
    /// </summary>
    /// <returns>List of profiles</returns>
    public static async Task<List<Profile>> GetAll()
        => await Controller.Profiles.QueryAll(x => true);
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Profile>.Filter.Eq(x => x.Id, Id);
        await Controller.Profiles.ReplaceOneAsync(filter, this);
    }
}