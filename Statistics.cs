using System.Diagnostics;
using System.Text.Json;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Database;

namespace ServerOverflow;

/// <summary>
/// Statistics processor
/// </summary>
public class Statistics {
    /// <summary>
    /// Load saved statistics
    /// </summary>
    static Statistics() {
        if (File.Exists("stats.json")) {
            Log.Information("Loading statistics file...");
            var content = File.ReadAllText("stats.json");
            try {
                Stats = JsonSerializer.Deserialize<Statistics>(content)!;
            } catch (Exception e) {
                Log.Fatal("Failed to load statistics: {0}", e);
                Environment.Exit(-1);
            }
            return;
        }

        Log.Warning("Statistics file is missing, starting from scratch!");
        Stats = new Statistics(); Stats.Save();
    }
    
    /// <summary>
    /// Statistics instance
    /// </summary>
    public static Statistics Stats;
    
    /// <summary>
    /// How many servers are there
    /// </summary>
    public List<int> TotalServers { get; set; } = [0];
    
    /// <summary>
    /// How many servers have chat reporting enabled
    /// </summary>
    public List<int> ChatReporting { get; set; } = [0];
    
    /// <summary>
    /// How many servers have online mode enabled
    /// </summary>
    public List<int> OnlineMode { get; set; } = [0];
    
    /// <summary>
    /// How many servers use forge
    /// </summary>
    public List<int> ForgeServers { get; set; } = [0];
    
    /// <summary>
    /// How many servers have custom software
    /// </summary>
    public List<int> CustomSoftware { get; set; } = [0];
    
    /// <summary>
    /// How many servers are anti-DDoS proxies
    /// </summary>
    public List<int> AntiDDoS { get; set; } = [0];
    
    /// <summary>
    /// What custom software are the most popular
    /// </summary>
    public List<string> SoftwarePopularity { get; set; } = ["Not populated yet, please wait for a bit :3"];
    
    /// <summary>
    /// What versions are the most popular
    /// </summary>
    public List<string> VersionPopularity { get; set; } = ["Not populated yet, please wait for a bit :3"];
    
    /// <summary>
    /// What forge mods are the most popular
    /// </summary>
    public List<string> ForgeModsPopularity { get; set; } = ["Not populated yet, please wait for a bit :3"];
    
    /// <summary>
    /// When should statistics be collected again
    /// </summary>
    public DateTime CollectAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Statistics processor thread
    /// </summary>
    public static async Task ProcessorThread() {
        while (true) {
            try {
                if (Stats.CollectAt > DateTime.UtcNow)
                    await Task.Delay(Stats.CollectAt - DateTime.UtcNow);
                Log.Information("Collecting server statistics");
                var watch = new Stopwatch(); watch.Start();

                Stats.TotalServers.Add((int)await Controller.Servers.Count(x => true));
                Stats.ChatReporting.Add((int)await Controller.Servers.Count(x => x.Ping.ChatReporting));
                Stats.OnlineMode.Add((int)await Controller.Servers.Count(x => x.OnlineModeGuess == Database.OnlineMode.Online));
                Stats.ForgeServers.Add((int)await Controller.Servers.Count(x => x.Ping.IsForge));
                Stats.CustomSoftware.Add((int)await Controller.Servers.Count(x => 
                    x.Ping.Version != null && x.Ping.Version.Name != null &&
                    x.Ping.Version.Name.Contains("Vanilla")));
                Stats.AntiDDoS.Add((int)await Controller.Servers.Count(x => 
                    (x.Ping.Description != null && (
                        x.Ping.Description.Contains("Blad pobierania statusu. Polacz sie bezposrednio!") ||
                        x.Ping.Description.Contains("This server is offline Powered by FalixNodes.net") ||
                        x.Ping.Description.Contains("Ochrona DDoS: Przekroczono limit polaczen.") ||
                        x.Ping.Description.Contains("Start the server at FalixNodes.net/start") ||
                        x.Ping.Description.Contains("Serwer jest aktualnie wy") ||
                        x.Ping.Description.Contains("\u00a8 |  ")
                    )) || (x.Ping.Version != null && x.Ping.Version.Name != null && (
                        x.Ping.Version.Name.Contains("COSMIC GUARD") ||
                        x.Ping.Version.Name.Contains("TCPShield.com") ||
                        x.Ping.Version.Name.Contains("â\u009a\u00a0 Error") ||
                        x.Ping.Version.Name.Contains("\u26a0 Error")
                    ))));
                
                var software = new Dictionary<string, int>();
                var versions = new Dictionary<string, int>();
                var mods = new Dictionary<string, int>();
                
                var filter = Builders<Server>.Filter.Empty;
                using var cursor = await Controller.Servers.FindAsync(filter,
                    new FindOptions<Server> { BatchSize = 100 });
                while (await cursor.MoveNextAsync())
                    foreach (var server in cursor.Current) {
                        if (server.Ping.Version?.Name != null) {
                            var split = server.Ping.Version.Name.Split(" ");
                            var version = split.Length > 1 ? split[0] : "Vanilla";
                            if (!software.TryGetValue(version, out _))
                                software.Add(version, 1);
                            else software[version] += 1;
                        }

                        if (server.Ping.Version?.Protocol != null && 
                            Protocol.Mapping.TryGetValue(server.Ping.Version.Protocol.Value, out var mapping)) {
                            if (!versions.TryGetValue(mapping, out _))
                                versions.Add(mapping, 1);
                            else versions[mapping] += 1;
                        }
                        
                        if (server.Ping.ModernForgeMods?.ModList != null)
                            foreach (var mod in server.Ping.ModernForgeMods.ModList) {
                                if (mod.ModId == null) continue;
                                if (!mods.TryGetValue(mod.ModId, out _))
                                    mods.Add(mod.ModId, 1);
                                else mods[mod.ModId] += 1;
                            }
                        
                        if (server.Ping.LegacyForgeMods?.ModList != null)
                            foreach (var mod in server.Ping.LegacyForgeMods.ModList) {
                                if (mod.ModId == null) continue;
                                if (!mods.TryGetValue(mod.ModId, out _))
                                    mods.Add(mod.ModId, 1);
                                else mods[mod.ModId] += 1;
                            }
                    }

                Stats.SoftwarePopularity = software.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                Stats.VersionPopularity = versions.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                Stats.ForgeModsPopularity = mods.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                Stats.CollectAt = DateTime.UtcNow + TimeSpan.FromHours(1); watch.Stop();
                Log.Information("Took {0} to collect statistics", watch.Elapsed);
                Stats.Save();
            } catch (Exception e) {
                Log.Error("Statistics processor thread crashed: {0}", e);
            }
        }
    }
    
    /// <summary>
    /// Save statistics updates
    /// </summary>
    private void Save() => File.WriteAllText("stats.json", JsonSerializer.Serialize(Stats));
}