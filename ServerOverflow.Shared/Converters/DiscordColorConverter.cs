using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerOverflow.Shared.Converters;

public class DiscordColorConverter : JsonConverter<Color> {
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetInt32();
        var r = (value >> 16) & 0xFF;
        var g = (value >> 8) & 0xFF;
        var b = value & 0xFF;
        return Color.FromArgb(r, g, b);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) {
        var decimalColor = (value.R << 16) | (value.G << 8) | value.B;
        writer.WriteNumberValue(decimalColor);
    }
}