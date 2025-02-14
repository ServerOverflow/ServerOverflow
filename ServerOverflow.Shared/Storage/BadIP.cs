using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// Bad IP address entry
/// </summary>
public class BadIP {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// The IP address in question
    /// </summary>
    [BsonElement("ip")]
    public string IP { get; set; } = "";
}