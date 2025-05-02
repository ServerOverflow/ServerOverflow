namespace ServerOverflow.Backend.Models;

/// <summary>
/// Basic statistics model
/// </summary>
public class StatisticsModel {
    /// <summary>
    /// Total servers
    /// </summary>
    public int TotalServers { get; set; }
    
    /// <summary>
    /// Total online servers
    /// </summary>
    public int OnlineServers { get; set; }
    
    /// <summary>
    /// Total not configured servers
    /// </summary>
    public int NotConfiguredServers { get; set; }
}