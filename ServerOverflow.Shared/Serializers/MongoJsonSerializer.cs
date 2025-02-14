using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ServerOverflow.Shared.Serializers;

/// <summary>
/// Handles cases where instead of the expected object a JSON serialized string is passed
/// </summary>
public class MongoJsonSerializer<T> : SerializerBase<T> {
    /// <summary>
    /// Deserializes either a string or the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Argument</param>
    /// <returns>Deserialized type</returns>
    public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.GetCurrentBsonType() == BsonType.String 
            ? BsonSerializer.Deserialize<T>(context.Reader.ReadString())
            : BsonSerializer.Deserialize<T>(context.Reader);

    /// <summary>
    /// Serializes to the specified type
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="args">Arguments</param>
    /// <param name="value">Value</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        => BsonSerializer.Serialize(context.Writer, value);
}