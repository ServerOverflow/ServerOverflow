using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// Audit log entry
/// </summary>
public class LogEntry {
    /// <summary>
    /// Log entry identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// User action performed
    /// </summary>
    public UserAction Action { get; set; }
    
    /// <summary>
    /// Log entry creation date
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Additional data
    /// </summary>
    public Dictionary<string, string> Data { get; set; } = [];

    /// <summary>
    /// Short log entry description
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// Fetches an account by its identifier
    /// </summary>
    /// <param name="id">Object identifier</param>
    /// <returns>Account, may be null</returns>
    public static async Task<LogEntry?> Get(string id)
        => await Database.AuditLog.QueryFirst(x => x.Id.ToString() == id);

    /// <summary>
    /// Creates a new log entry
    /// </summary>
    /// <param name="action">User action</param>
    /// <param name="data">Log entry data</param>
    /// <returns>Log entry</returns>
    public static async Task<LogEntry> Create(UserAction action, Dictionary<string, string> data) {
        var logEntry = new LogEntry {
            Action = action, Data = data,
            Timestamp = DateTime.UtcNow
        };

        logEntry.Description = logEntry.ToString();
        await Database.AuditLog.InsertOneAsync(logEntry);
        return logEntry;
    }
    
    /// <summary>
    /// Converts log entry to readable string
    /// </summary>
    /// <returns>Humanized log entry</returns>
    public override string ToString()
        => Action switch {
            UserAction.CreatedApiKey => $"{Data["perpetrator_name"]} created an API key with permissions {Data["target_permissions"]}",
            UserAction.UpdatedApiKey => $"{Data["perpetrator_name"]} updated an API key with permissions {Data["target_permissions"]}",
            UserAction.DeletedApiKey => $"{Data["perpetrator_name"]} deleted an API key with permissions {Data["target_permissions"]}",
            UserAction.CreatedInvitation => $"{Data["perpetrator_name"]} created an invitation code with badge name {Data["target_badge"]}",
            UserAction.UpdatedInvitation => $"{Data["perpetrator_name"]} updated an invitation code with badge name {Data["target_badge"]}",
            UserAction.DeletedInvitation => $"{Data["perpetrator_name"]} deleted an invitation code with badge name {Data["target_badge"]}",
            UserAction.CreatedExclusion => $"{Data["perpetrator_name"]} created an exclusion with {Data["target_ips"]} IP addresses",
            UserAction.UpdatedExclusion => $"{Data["perpetrator_name"]} updated an exclusion with {Data["target_ips"]} IP addresses",
            UserAction.DeletedExclusion => $"{Data["perpetrator_name"]} deleted an exclusion with {Data["target_ips"]} IP addresses",
            UserAction.CreatedProfile => $"{Data["perpetrator_name"]} added a minecraft profile with username {Data["target_username"]}",
            UserAction.DeletedProfile => $"{Data["perpetrator_name"]} deleted a minecraft profile with username {Data["target_username"]}",
            UserAction.DeletedAccount => $"{Data["perpetrator_name"]} deleted account {Data["target_name"]} with {Data["target_badge"]} badge",
            UserAction.UpdatedAccount => $"{Data["perpetrator_name"]} updated account {Data["target_name"]} with {Data["target_badge"]} badge",
            UserAction.Registered => $"{Data["perpetrator_name"]} created a new account on ServerOverflow with {Data["perpetrator_badge"]} badge",
            UserAction.LoggedIn => $"{Data["perpetrator_name"]} with {Data["perpetrator_badge"]} badge logged into their ServerOverflow account",
            UserAction.SearchedServers => $"{Data["perpetrator_name"]} searched for servers with query {Data["target_query"]} and got {Data["target_results"]} results",
            _ => "Unknown user action? 🤔"
        };
}