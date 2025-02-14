using System.Text.Json.Serialization;

namespace ServerOverflow.Snooper; 

[JsonSourceGenerationOptions(WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [ typeof(JsonStringEnumConverter) ])]
[JsonSerializable(typeof(Configuration))]
public partial class JsonContext : JsonSerializerContext;