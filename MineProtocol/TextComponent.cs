using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace MineProtocol; 

/// <summary>
/// A Minecraft text component
/// </summary>
public class TextComponent {
    [JsonPropertyName("extra")]
    [JsonConverter(typeof(ChatComponentConverter))]
    public List<TextComponent> Extra { get; set; } = [];
    
    [JsonPropertyName("with")]
    [JsonConverter(typeof(ChatComponentConverter))]
    public List<TextComponent> With { get; set; } = [];
    
    [JsonPropertyName("strikethrough")]
    public bool? Strikethrough { get; set; }
    
    [JsonPropertyName("underlined")]
    public bool? Underlined { get; set; }
    
    [JsonPropertyName("obfuscated")]
    public bool? Obfuscated { get; set; }
    
    [JsonPropertyName("italic")]
    public bool? Italic { get; set; }
    
    [JsonPropertyName("bold")]
    public bool? Bold { get; set; }
    
    [JsonPropertyName("translate")]
    public string? Translate { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    /// <summary>
    /// Color code to CSS color dictionary
    /// </summary>
    private static readonly Dictionary<string, string> _mapping = new() {
        { "black", "#000000" },
        { "dark_blue", "#0000AA" },
        { "dark_green", "#00AA00" },
        { "dark_aqua", "#00AAAA" },
        { "dark_red", "#AA0000" },
        { "dark_purple", "#AA00AA" },
        { "gold", "#FFAA00" },
        { "gray", "#AAAAAA" },
        { "dark_gray", "#555555" },
        { "blue", "#5555FF" },
        { "green", "#55FF55" },
        { "aqua", "#55FFFF" },
        { "red", "#FF5555" },
        { "light_purple", "#FF55FF" },
        { "yellow", "#FFFF55" },
        { "white", "#FFFFFF" },
        { "reset", "#FFFFFF" }
    };
    
    /// <summary>
    /// Inherit properties of a parent
    /// </summary>
    /// <param name="component">Component</param>
    /// <returns>Text component</returns>
    public TextComponent Inherit(TextComponent component) {
        Strikethrough ??= component.Strikethrough;
        Obfuscated ??= component.Obfuscated;
        Underlined ??= component.Underlined;
        Italic ??= component.Italic;
        Color ??= component.Color;
        Bold ??= component.Bold;
        return this;
    }
    
    /// <summary>
    /// Returns a translated text component
    /// </summary>
    /// <returns>Text component</returns>
    public TextComponent GetTranslated() {
        if (Translate == null) return this;
        var components = new List<TextComponent>();
        var text = Resources.Language.GetValueOrDefault(Translate, Translate).Replace("%%", "%");
        var index = 0; int index2; var idx = 0; var buf = "";
        while ((index2 = text.IndexOf('%', index)) != -1) {
            buf += text[index..index2];
            index = index2;
            if (index + 1 < text.Length && text[index + 1] == '%') {
                buf += "%";
                index += 2;
                continue;
            }
            
            var sidx = text.IndexOf('s', index);
            if (sidx == -1) {
                buf += '%';
                index += 1;
                continue;
            }
            
            var placeholder = text[index..(sidx+1)];
            if (placeholder == "%s") {
                if (With == null || idx >= With.Count) {
                    buf += placeholder;
                    index += placeholder.Length;
                    continue;
                }
                
                if (!string.IsNullOrWhiteSpace(buf))
                    components.Add(new TextComponent { Text = buf });
                components.Add(With[idx]);
                buf = ""; idx += 1;
            }
            
            index += 2;
        }
        
        buf += text[index..];
        if (!string.IsNullOrWhiteSpace(buf))
            components.Add(new TextComponent { Text = buf });
        return new TextComponent { Extra = components };
    }

    /// <summary>
    /// Removes all decorations from this text component
    /// </summary>
    /// <returns>Chat Component</returns>
    public TextComponent Clean() {
        var component = new TextComponent {
            Text = Text, Translate = Translate, Extra = []
        };
        
        foreach (var extra in Extra)
            component.Extra.Add(extra.Clean());
        
        return component;
    }
    
    /// <summary>
    /// Converts this text component and it's children to HTML
    /// </summary>
    /// <returns>Converted HTML</returns>
    public string ToHtml() => 
        Extra.Aggregate(GetComponentHtml(), (str, i) => str + i.Inherit(this).ToHtml());
    
    /// <summary>
    /// Converts this text component and it's children to raw text
    /// </summary>
    /// <returns>Converted text</returns>
    public string ToText() => 
        Extra.Aggregate(GetComponentText(), (str, i) => str + i.ToText());

    /// <summary>
    /// Converts this text component to HTML
    /// </summary>
    /// <returns>Converted HTML</returns>
    private string GetComponentHtml() {
        if (Translate != null)
            Extra.InsertRange(0, GetTranslated().Extra);
        
        Text ??= ""; Color ??= "#FFFFFF";
        if (Text.Contains('§') || Text.Contains('&')) {
            Extra.InsertRange(0, ColorEncoding.Parse(Text).Extra);
            Text = "";
        }
        
        var enc = HttpUtility.HtmlEncode(Text).Replace("\n", "<br>");
        if (string.IsNullOrWhiteSpace(enc)) return "";
        var code = "#FFFFFF";
        if (_mapping.TryGetValue(Color, out var value)) code = value;
        var output = $"<span style=\"color: {code};\">{enc}</span>";
        if (Obfuscated.HasValue && Obfuscated.Value) output = $"<obf>{output}</obf>";
        if (Strikethrough.HasValue && Strikethrough.Value) output = $"<s>{output}</s>";
        if (Underlined.HasValue && Underlined.Value) output = $"<u>{output}</u>";
        if (Italic.HasValue && Italic.Value) output = $"<em>{output}</em>";
        if (Bold.HasValue && Bold.Value) output = $"<b>{output}</b>";
        return output;
    }

    /// <summary>
    /// Converts this text component to raw text
    /// </summary>
    /// <returns>Converted text</returns>
    private string GetComponentText() {
        if (Translate != null)
            Extra.InsertRange(0, GetTranslated().Extra);
        
        Text ??= "";
        if (Text.Contains('§') || Text.Contains('&')) {
            Extra.InsertRange(0, ColorEncoding.Parse(Text).Extra);
            Text = "";
        }

        return Text;
    }
    
    /// <summary>
    /// Creates a text component from a JSON string
    /// </summary>
    /// <param name="value">JSON string</param>
    /// <returns>Text Component</returns>
    public static TextComponent Parse(string? value) {
        if (value == null) return new TextComponent();
        try { return new TextComponent { Text = JsonSerializer.Deserialize<string>(value) }; } catch { /* Ignore */ }
        return JsonSerializer.Deserialize<TextComponent>(value) ?? new TextComponent();
    }
    
    /// <summary>
    /// Creates a text component from a JSON element
    /// </summary>
    /// <param name="element">JSON element</param>
    /// <returns>Text Component</returns>
    public static TextComponent Parse(JsonElement? element) {
        if (element == null) return new TextComponent();
        try { return new TextComponent { Text = element.Value.Deserialize<string>() }; } catch { /* Ignore */ }
        return element.Value.Deserialize<TextComponent>() ?? new TextComponent();
    }
    
    /// <summary>
    /// Creates an empty text component
    /// </summary>
    public TextComponent() { }
}

/// <summary>
/// Chat component list converter
/// </summary>
public class ChatComponentConverter : JsonConverter<List<TextComponent>> {
    /// <summary>
    /// Reads a chat component list
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="typeToConvert">Type</param>
    /// <param name="options">Options</param>
    /// <returns>Chat component list</returns>
    public override List<TextComponent> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var elements = new List<TextComponent>();
        switch (reader.TokenType) {
            case JsonTokenType.StartArray: {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                    switch (reader.TokenType) {
                        case JsonTokenType.String: {
                            elements.Add(ColorEncoding.Parse(reader.GetString()));
                            break;
                        }
                        case JsonTokenType.StartObject: {
                            elements.Add(JsonSerializer.Deserialize<TextComponent>(ref reader, options)!);
                            break;
                        }
                        default: {
                            throw new InvalidDataException($"Unexpected JSON token {reader.TokenType}");
                        }
                    }
                }

                return elements;
            }
            case JsonTokenType.Null: {
                return elements;
            }
            default: {
                throw new InvalidDataException($"Unexpected JSON token {reader.TokenType}");
            }
        }
    }

    /// <summary>
    /// Writes a chat component list
    /// </summary>
    /// <param name="writer">Writer</param>
    /// <param name="value">Value</param>
    /// <param name="options">Options</param>
    public override void Write(Utf8JsonWriter writer, List<TextComponent> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);
}