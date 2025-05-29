using Prometheus;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Snooper;

/// <summary>
/// Statistics class
/// </summary>
public static class Statistics {
    /// <summary>
    /// Total servers joined gauge (online, offline)
    /// </summary>
    public static readonly Counter ServersTotal = Metrics.CreateCounter("so_bot_joined_total", "Total servers joined by the scanner", "mode", "status");

    /// <summary>
    /// Converts join result to status description
    /// </summary>
    /// <param name="result">Join result</param>
    /// <returns>Status description</returns>
    public static string ToStatus(this JoinResult result) {
        if (result.Success) return "Success";
        if (result.ErrorMessage!.Contains("The operation has timed out"))
            return "Timed out";
        if (result.ErrorMessage!.Contains("No route to host"))
            return "No route to host";
        if (result.ErrorMessage!.Contains("Connection refused"))
            return "Connection refused";
        if (result.ErrorMessage!.Contains("Connection reset by peer") 
            || result.ErrorMessage!.Contains("end of the stream"))
            return "Network error";
        if (result.ErrorMessage!.Contains("Failed to authenticate with Mojang"))
            if (result.Exception!.Contains("TooManyRequests"))
                return "Ratelimited";
            else if (result.Exception!.Contains("Forbidden"))
                return "Forbidden";
            else return "Unknown";
        return "Protocol bug";
    }
}