using MongoDB.Bson.Serialization.Attributes;

namespace ServerOverflow.Database;

/// <summary>
/// Bad IP address entry
/// </summary>
public class BadIP : Document {
    /// <summary>
    /// The IP address in question
    /// </summary>
    [BsonElement("ip")]
    public string IP { get; set; } = "";
}