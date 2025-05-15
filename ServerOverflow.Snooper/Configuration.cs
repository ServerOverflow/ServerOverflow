namespace ServerOverflow.Snooper; 

/// <summary>
/// Global configuration file
/// </summary>
public class Configuration {
    /// <summary>
    /// MongoDB connection URI
    /// </summary>
    public static string MongoUri { get; set; } = Environment.GetEnvironmentVariable("MONGO_URI") ?? "mongodb://127.0.0.1:27017?maxPoolSize=5000";
    
    /// <summary>
    /// Residential proxy URL
    /// </summary>
    public static string ProxyUrl { get; set; } = Environment.GetEnvironmentVariable("PROXY_URL") ?? "edit_me";
    
    /// <summary>
    /// Residential proxy username
    /// </summary>
    public static string ProxyUsername { get; set; } = Environment.GetEnvironmentVariable("PROXY_USERNAME") ?? "edit_me";
        
    /// <summary>
    /// Residential proxy password
    /// </summary>
    public static string ProxyPassword { get; set; } = Environment.GetEnvironmentVariable("PROXY_PASSWORD") ?? "edit_me";
}