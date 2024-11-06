using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ServerOverflow.Database; 

/// <summary>
/// An abstract document with an ObjectId
/// </summary>
public abstract class Document {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
}