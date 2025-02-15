using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ServerOverflow.Shared.Serializers;

/// <summary>
/// Custom int deserializer that handles invalid types
/// </summary>
public class MongoIntSerializer : SerializerBase<int?>, IBsonArraySerializer {
    /// <summary>
    /// Deserializes either a string or the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Argument</param>
    /// <returns>Deserialized type</returns>
    public override int? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
        var reader = context.Reader;
        var type = reader.GetCurrentBsonType();
        if (type == BsonType.Null) {
            reader.ReadNull();
            return null!;
        }
        
        return reader.GetCurrentBsonType() switch {
            BsonType.Int32 => reader.ReadInt32(),
            BsonType.Int64 => (int)reader.ReadInt64(),
            BsonType.Double => (int)reader.ReadDouble(),
            _ => default
        };
    }

    /// <summary>
    /// Serializes to the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Arguments</param>
    /// <param name="value">Value</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int? value) {
        if (value.HasValue) context.Writer.WriteInt32(value.Value);
        else context.Writer.WriteNull();
    }
    
    /// <summary>
    /// Tries to get the serialization info for the individual items of the array
    /// </summary>
    /// <param name="serializationInfo">Information</param>
    /// <returns>True if info exists</returns>
    public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo) {
        serializationInfo = default!;
        return true;
    }
}