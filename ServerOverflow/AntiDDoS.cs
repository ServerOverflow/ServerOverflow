using ServerOverflow.Storage;

namespace ServerOverflow;

/// <summary>
/// Anti-DDoS proxy detection extensions
/// </summary>
public static class AntiDDoS {
    /// <summary>
    /// Server software names
    /// </summary>
    private static readonly string[] _software = [ 
        "COSMIC GUARD", "TCPShield.com",
        "⚠ Error", "â\u009a\u00a0 Error",
        "§l§6✘", "§l§9•", "§c§l⬤"
    ];
    
    /// <summary>
    /// Server descriptions
    /// </summary>
    private static readonly string[] _descriptions = [ 
        "Blad pobierania statusu. Polacz sie bezposrednio!", 
        "Start the server at FalixNodes.net/start", 
        "Serwer jest aktualnie wy", 
        "Hosted in GreatHost.es",
        "Server not found.",
        "\u00a8 |  "
    ];

    /// <summary>
    /// Is server an Anti-DDoS proxy
    /// </summary>
    /// <param name="server">Server</param>
    /// <returns>True if proxy</returns>
    public static bool IsAntiDDoS(this Server server) {
        if (server.Ping.Version?.Name != null)
            if (_software.Contains(server.Ping.Version.Name) || 
                _software.Any(x => server.Ping.Version.Name.Contains(x)))
                return true;
        if (server.Ping.CleanDescription != null)
            if (_descriptions.Any(x => server.Ping.CleanDescription.Contains(x)))
                return true;
        
        return false;
    }
}