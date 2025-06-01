using System.Drawing;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ServerOverflow.Shared.Converters;

namespace ServerOverflow.Shared;

/// <summary>
/// Webhook schema and sender
/// </summary>
public class Webhook {
    /// <summary>
    /// Webhook URL
    /// </summary>
    private static string? WebhookUrl { get; set; }
    
    /// <summary>
    /// Initializes the webhook url
    /// </summary>
    /// <param name="url">Webhook URL</param>
    public static void Initialize(string url) 
        => WebhookUrl = url;

    /// <summary>
    /// Sends this webhook
    /// </summary>
    public async Task Send() {
        if (WebhookUrl == null)
            throw new InvalidOperationException("Call Initialize first");
        using var client = new HttpClient();
        var resp = await client.PostAsync(WebhookUrl, JsonContent.Create(this));
        if (!resp.IsSuccessStatusCode)
            throw new Exception(await resp.Content.ReadAsStringAsync());
    }
    
    /// <summary>
    /// Webhook message body
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Webhook embeds
    /// </summary>
    [JsonPropertyName("embeds")]
    public List<Embed> Embeds { get; set; } = [];
}

/// <summary>
/// Discord embed
/// </summary>
public class Embed {
    /// <summary>
    /// Embed display title
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
        
    /// <summary>
    /// Embed border color
    /// </summary>
    [JsonPropertyName("color")]
    [JsonConverter(typeof(DiscordColorConverter))]
    public Color? Color { get; set; }

    /// <summary>
    /// List of fields
    /// </summary>
    [JsonPropertyName("fields")]
    public List<Field>? Fields { get; set; }
        
    /// <summary>
    /// List of fields
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
        
    /// <summary>
    /// List of fields
    /// </summary>
    [JsonPropertyName("footer")]
    public Footer? Footer { get; set; }
}
    
/// <summary>
/// Discord embed field
/// </summary>
public class Field {
    /// <summary>
    /// Field name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
            
    /// <summary>
    /// Field value
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }
            
    /// <summary>
    /// Webhook message body
    /// </summary>
    [JsonPropertyName("inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// Creates a new field
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="value">Value</param>
    /// <param name="inline">Inline</param>
    public Field(string name, string value, bool inline = false) {
        Name = name; Value = value; Inline = inline;
    }
}

/// <summary>
/// Embed footer
/// </summary>
public class Footer {
    /// <summary>
    /// Footer text
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; }

    /// <summary>
    /// Creates a new footer
    /// </summary>
    /// <param name="text">Text</param>
    public Footer(string text)
        => Text = text;
}