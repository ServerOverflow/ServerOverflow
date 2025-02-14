using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ServerOverflow.Shared.Serializers;

/// <summary>
/// Custom string deserializer that handles invalid types
/// </summary>
public class MongoStringSerializer : SerializerBase<string?> {
    /// <summary>
    /// Deserializes either a string or the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Argument</param>
    /// <returns>Deserialized type</returns>
    public override string? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
        var reader = context.Reader;
        var type = reader.GetCurrentBsonType();
        if (type == BsonType.Null) {
            reader.ReadNull();
            return null;
        }
        
        return reader.GetCurrentBsonType() switch {
            BsonType.String => reader.ReadString(),
            BsonType.Int32 => reader.ReadInt32().ToString(),
            BsonType.Int64 => reader.ReadInt64().ToString(),
            BsonType.Double => reader.ReadDouble().ToString(),
            BsonType.Boolean => reader.ReadBoolean().ToString(),
            BsonType.ObjectId => reader.ReadObjectId().ToString(),
            _ => default
        };
    }

    /// <summary>
    /// Serializes to the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Arguments</param>
    /// <param name="value">Value</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string? value) {
        if (value != null) context.Writer.WriteString(value);
        else context.Writer.WriteNull();
    }
}