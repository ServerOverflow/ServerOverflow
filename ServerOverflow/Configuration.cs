using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace ServerOverflow; 

/// <summary>
/// Global configuration file
/// </summary>
public class Configuration {
    /// <summary>
    /// JSON serializer options
    /// </summary>
    public static readonly JsonSerializerOptions Options = new() {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true, IncludeFields = true
    };
    
    /// <summary>
    /// Static object instance
    /// </summary>
    public static readonly Configuration Config;

    /// <summary>
    /// Load the configuration
    /// </summary>
    static Configuration() {
        if (File.Exists("config.json")) {
            Log.Information("Loading configuration file...");
            var content = File.ReadAllText("config.json");
            try {
                Config = JsonSerializer.Deserialize<Configuration>(content, Options)!;
            } catch (Exception e) {
                Log.Fatal("Failed to load config: {0}", e);
                Environment.Exit(-1);
            }
            
            Config.Save();
            return;
        }

        Config = new Configuration(); Config.Save();
        Log.Fatal("Configuration file doesn't exist, created a new one!");
        Log.Fatal("Please fill it with all the necessary information.");
        Environment.Exit(-1);
    }

    /// <summary>
    /// MongoDB connection URI
    /// </summary>
    public string MongoUri { get; set; } = "mongodb://127.0.0.1:27017?maxPoolSize=5000";
    
    /// <summary>
    /// Save configuration changes
    /// </summary>
    public void Save() => File.WriteAllText("config.json",
        JsonSerializer.Serialize(Config, Options));
}