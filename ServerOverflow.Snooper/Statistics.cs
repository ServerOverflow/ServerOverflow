using Prometheus;

namespace ServerOverflow.Snooper;

/// <summary>
/// Statistics class
/// </summary>
public static class Statistics {
    /// <summary>
    /// Total servers joined gauge (online, offline)
    /// </summary>
    public static readonly Counter ServersTotal = Metrics.CreateCounter("so_bot_joined_total", "Total servers joined by the scanner", "mode", "success");
}