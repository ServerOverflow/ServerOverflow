using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ServerOverflow.Shared.Converters;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// Honeypot event entry
/// </summary>
public class HoneypotEvent {
    /// <summary>
    /// Event entry identifier
    /// </summary>
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// Honeypot event type
    /// </summary>
    public HoneypotEventType Type { get; set; }
    
    /// <summary>
    /// Event entry creation date
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Operating system detected via p0f
    /// </summary>
    public string OperatingSystem { get; set; } = "";
    
    /// <summary>
    /// Protocol version used by the perpetrator
    /// </summary>
    public int Protocol { get; set; }
    
    /// <summary>
    /// IP address of the perpetrator
    /// </summary>
    public string SourceIp { get; set; } = "";
    
    /// <summary>
    /// Port of the perpetrator
    /// </summary>
    public int SourcePort { get; set; }
    
    /// <summary>
    /// IP address of the honeypot server
    /// </summary>
    public string TargetIp { get; set; } = "";
    
    /// <summary>
    /// Port of the honeypot server
    /// </summary>
    public int TargetPort { get; set; }
    
    /// <summary>
    /// Username of the perpetrator
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// UUID of the perpetrator
    /// </summary>
    public string? Uuid { get; set; }
    
    /// <summary>
    /// Short log entry description
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// Fetches a honeypot event entry by its identifier
    /// </summary>
    /// <param name="id">Object identifier</param>
    /// <returns>Honeypot event entry, may be null</returns>
    public static async Task<HoneypotEvent?> Get(string id)
        => await Database.HoneypotEvents.QueryFirst(x => x.Id.ToString() == id);
    
    /// <summary>
    /// Converts log entry to readable string
    /// </summary>
    /// <returns>Humanized log entry</returns>
    public override string ToString()
        => Type switch {
            HoneypotEventType.Pinged => $"{SourceIp}:{SourcePort} pinged {TargetIp}:{TargetPort} (protocol: {Protocol}, os: {OperatingSystem})",
            HoneypotEventType.Joined => $"{SourceIp}:{SourcePort} joined {TargetIp}:{TargetPort} (protocol: {Protocol}, os: {OperatingSystem}, username: {Username}, uuid: {Uuid})",
            HoneypotEventType.Left => $"{SourceIp}:{SourcePort} left {TargetIp}:{TargetPort} (protocol: {Protocol}, os: {OperatingSystem}, username: {Username}, uuid: {Uuid})",
            _ => "Unknown honeypot event type? 🤔"
        };
}

/// <summary>
/// Honeypot event type
/// </summary>
public enum HoneypotEventType {
    Pinged = 0,
    Joined = 1,
    Left = 2
}