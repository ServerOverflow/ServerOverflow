using System.Reflection;
using System.Text.Json;

namespace MineProtocol;

/// <summary>
/// Minecraft related resources
/// </summary>
public static class Resources {
    /// <summary>
    /// Language key to text mapping
    /// </summary>
    public static Dictionary<string, string> Language { get; }
    
    /// <summary>
    /// Protocol version to name mapping
    /// </summary>
    public static Dictionary<int, string> Protocol { get; }
    
    /// <summary>
    /// Game version to protocol mapping
    /// </summary>
    public static Dictionary<string, int> Version { get; }

    /// <summary>
    /// Loads the resources
    /// </summary>
    static Resources() {
        var ass = Assembly.GetCallingAssembly();
        using var proto = ass.GetManifestResourceStream("protocol.json")!;
        using var lang = ass.GetManifestResourceStream("language.json")!;
        Language = JsonSerializer.Deserialize<Dictionary<string, string>>(lang)!;
        Protocol = JsonSerializer.Deserialize<Dictionary<int, string>>(proto)!;
        Version = Protocol.ToDictionary(x => x.Value, x => x.Key);
    }
}