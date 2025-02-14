using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ServerOverflow.Shared.Serializers;

/// <summary>
/// Handles data from the SLP and returns the default value if an invalid type is provided
/// </summary>
public class MongoSafeSerializer<T> : SerializerBase<T> {
    /// <summary>
    /// Deserializes either a string or the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Argument</param>
    /// <returns>Deserialized type</returns>
    public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
        try {
            return BsonSerializer.LookupSerializer<T>().Deserialize(context, args);
        } catch (Exception) {
            return default!;
        }
    }

    /// <summary>
    /// Serializes to the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Arguments</param>
    /// <param name="value">Value</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        => BsonSerializer.LookupSerializer<T>().Serialize(context, args, value);
}