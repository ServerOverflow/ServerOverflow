using System.Diagnostics;
using System.Text.Json;
using MineProtocol;
using MongoDB.Driver;
using Prometheus;
using Serilog;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Frontend.Processors;

/// <summary>
/// Statistics processor
/// </summary>
public static class Statistics {
    /// <summary>
    /// Servers gauge (total, chat_reporting, online_mode, whitelist, forge, custom, anti_ddos)
    /// </summary>
    private static readonly Gauge _servers = Metrics.CreateGauge("so_servers", "Servers found by the scanner", "type");
    
    /// <summary>
    /// Server software gauge
    /// </summary>
    private static readonly Gauge _software = Metrics.CreateGauge("so_software", "Software brand popularity", "brand");
    
    /// <summary>
    /// Minecraft version gauge
    /// </summary>
    private static readonly Gauge _versions = Metrics.CreateGauge("so_versions", "Minecraft versions popularity", "name");
    
    /// <summary>
    /// Forge mods gauge
    /// </summary>
    private static readonly Gauge _forgeMods = Metrics.CreateGauge("so_forge_mods", "Forge mods popularity", "id");
    
    /// <summary>
    /// Statistics processor thread
    /// </summary>
    public static async Task MainThread() {
        while (true) {
            try {
                var watch = new Stopwatch(); watch.Start();
                var customSoftware = 0;
                var antiDDoS = 0;
                
                var software = new Dictionary<string, int>();
                var versions = new Dictionary<string, int>();
                var forgeMods = new Dictionary<string, int>();
                
                var filter = Builders<Server>.Filter.Empty;
                using var cursor = await Database.Servers.FindAsync(filter,
                    new FindOptions<Server> { BatchSize = 1000 });
                while (await cursor.MoveNextAsync())
                    foreach (var server in cursor.Current) {
                        if (server.IsAntiDDoS()) {
                            antiDDoS++;
                            continue;
                        }
                        
                        if (server.Ping.Version?.Name != null) {
                            var split = server.Ping.Version.Name.Split(" ");
                            var version = split.Length > 1 ? split[0] : "Vanilla";
                            if (version != "Vanilla") customSoftware++;
                            if (version.All(char.IsDigit)) version = $"{version} (fuck JS sorting)";
                            if (!software.TryGetValue(version, out _))
                                software.Add(version, 1);
                            else software[version] += 1;
                        }

                        if (server.Ping.Version?.Protocol != null && 
                            Resources.Protocol.TryGetValue(server.Ping.Version.Protocol.Value, out var mapping)) {
                            if (!versions.TryGetValue(mapping, out _))
                                versions.Add(mapping, 1);
                            else versions[mapping] += 1;
                        }
                        
                        if (server.Ping.ModernForgeMods?.ModList != null)
                            foreach (var mod in server.Ping.ModernForgeMods.ModList) {
                                if (mod.ModId == null) continue;
                                if (mod.ModId.All(char.IsDigit)) mod.ModId = $"{mod.ModId} (fuck JS sorting)";
                                if (mod.ModId is not "minecraft" and not "mcp" and not "forge" and not "FML")
                                    if (!forgeMods.TryGetValue(mod.ModId, out _))
                                        forgeMods.Add(mod.ModId, 1);
                                    else forgeMods[mod.ModId] += 1;
                            }
                        
                        if (server.Ping.LegacyForgeMods?.ModList != null)
                            foreach (var mod in server.Ping.LegacyForgeMods.ModList) {
                                if (mod.ModId == null) continue;
                                if (mod.ModId.All(char.IsDigit)) mod.ModId = $"{mod.ModId} (fuck JS sorting)";
                                if (mod.ModId is not "minecraft" and not "mcp" and not "forge" and not "FML")
                                    if (!forgeMods.TryGetValue(mod.ModId, out _))
                                        forgeMods.Add(mod.ModId, 1);
                                    else forgeMods[mod.ModId] += 1;
                            }
                    }
                
                _servers.WithLabels("total").Set((int)await Database.Servers.Count(x => true));
                _servers.WithLabels("chat_reporting").Set((int)await Database.Servers.Count(x => x.Ping.ChatReporting));
                _servers.WithLabels("online_mode").Set((int)await Database.Servers.Count(x => x.JoinResult != null && x.JoinResult.OnlineMode == true));
                _servers.WithLabels("whitelist").Set((int)await Database.Servers.Count(x => x.JoinResult != null && x.JoinResult.Whitelist == true));
                _servers.WithLabels("forge").Set((int)await Database.Servers.Count(x => x.Ping.IsForge));
                _servers.WithLabels("custom").Set(customSoftware);
                _servers.WithLabels("anti_ddos").Set(antiDDoS);
                foreach (var item in software) _software.WithLabels(item.Key).Set(item.Value);
                foreach (var item in versions) _versions.WithLabels(item.Key).Set(item.Value);
                foreach (var item in forgeMods) _forgeMods.WithLabels(item.Key).Set(item.Value);
                await Task.Delay(600000);
            } catch (Exception e) {
                Log.Error("Statistics processor thread crashed: {0}", e);
            }
        }
    }
}