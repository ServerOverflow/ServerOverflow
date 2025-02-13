using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ServerOverflow.Storage;

/// <summary>
/// Minecraft player
/// </summary>
public class Player {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// Player's unique UUID
    /// </summary>
    public string Uuid { get; set; } = "";
    
    /// <summary>
    /// Player's username
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// Last seen online on any server
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// List of player sightings
    /// </summary>
    public List<Sighting> Sightings { get; set; } = [];

    /// <summary>
    /// List of players with same UUID or username
    /// </summary>
    public List<ObjectId> Duplicates { get; set; } = [];

    /// <summary>
    /// Fetches players by UUID or username
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <param name="name">Username</param>
    /// <returns>List of players matched</returns>
    public static async Task<List<Player>> GetAll(string uuid, string name)
        => await Database.Players.QueryAll(x => x.Uuid == uuid || x.Username == name);
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Player>.Filter.Eq(account => account.Id, Id);
        await Database.Players.ReplaceOneAsync(filter, this);
    }
}

/// <summary>
/// Player sighting data
/// </summary>
public class Sighting {
    /// <summary>
    /// Identifier of the server
    /// </summary>
    [BsonId] public ObjectId Server { get; set; }
    
    /// <summary>
    /// Last seen online on server
    /// </summary>
    public DateTime LastSeen { get; set; }
}