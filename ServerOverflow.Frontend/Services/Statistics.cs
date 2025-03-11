using System.Diagnostics;
using MaxMind.GeoIP2;
using MineProtocol;
using MongoDB.Driver;
using Prometheus;
using Serilog;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Frontend.Services;

/// <summary>
/// Statistics processor
/// </summary>
public class Statistics : BackgroundService {
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
    /// Server distribution by ASN gauge
    /// </summary>
    private static readonly Gauge _asns = Metrics.CreateGauge("so_geo_asns", "Server distribution per country", "as", "company");
    
    /// <summary>
    /// Server distribution by cities gauge
    /// </summary>
    private static readonly Gauge _cities = Metrics.CreateGauge("so_geo_cities", "Server distribution by cities", "country", "city");
    
    /// <summary>
    /// Server join errors (dead, network, bug, remote)
    /// </summary>
    public static readonly Gauge _errors = Metrics.CreateGauge("so_bot_errors", "Servers per join error", "message", "type");
    
    /// <summary>
    /// Runs the main service loop
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken token) {
        if (!File.Exists("GeoLite2-ASN.mmdb") || !File.Exists("GeoLite2-City.mmdb")) {
            Log.Fatal("Failed to find GeoLite2 database files within the working directory");
            Log.Fatal("Statistics processors WILL NOT RUN without them");
            return;
        }
        
        while (true) {
            var watch = new Stopwatch();
            watch.Start();
            
            try {
                var builder = Builders<Server>.Filter;
                _servers.WithLabels("total").Set(
                    await Database.Servers.CountDocumentsAsync(builder.Empty, cancellationToken: token));
                _servers.WithLabels("chat_reporting").Set(
                    await Database.Servers.CountDocumentsAsync(builder.Eq(x => x.Ping.ChatReporting, true), cancellationToken: token));
                _servers.WithLabels("online_mode").Set(
                    await Database.Servers.CountDocumentsAsync(builder.Eq(x => x.JoinResult!.OnlineMode, true), cancellationToken: token));
                _servers.WithLabels("whitelist").Set(
                    await Database.Servers.CountDocumentsAsync(builder.Eq(x => x.JoinResult!.Whitelist, true), cancellationToken: token));
                _servers.WithLabels("forge").Set(
                    await Database.Servers.CountDocumentsAsync(builder.Eq(x => x.Ping.IsForge, true), cancellationToken: token));
                
                using var city = new DatabaseReader("GeoLite2-City.mmdb");
                using var asn = new DatabaseReader("GeoLite2-ASN.mmdb");
                var cities = new Dictionary<(string, string), int>();
                var asns = new Dictionary<(string, string), int>();
                var forgeMods = new Dictionary<string, int>();
                var software = new Dictionary<string, int>();
                var versions = new Dictionary<string, int>();
                var customSoftware = 0;
                var antiDDoS = 0;
                
                var filter = Builders<Server>.Filter.Empty;
                var projection = Builders<Server>.Projection
                    .Include(x => x.Ping.Version!.Name)
                    .Include(x => x.Ping.Version!.Protocol)
                    .Include(x => x.Ping.ModernForgeMods!.ModList)
                    .Include(x => x.Ping.LegacyForgeMods!.ModList)
                    .Include(x => x.Ping.CleanDescription)
                    .Include(x => x.Port)
                    .Include(x => x.IP);
                
                using var cursor = await Database.Servers.FindAsync(filter,
                    new FindOptions<Server> {
                        Projection = projection, 
                        BatchSize = 500
                    }, token);
                while (await cursor.MoveNextAsync(token))
                    foreach (var server in cursor.Current) {
                        if (server.IsAntiDDoS()) {
                            antiDDoS++;
                            continue;
                        }
                        
                        if (server.Ping.Version?.Name != null) {
                            var split = server.Ping.Version.Name.Split(" ");
                            var version = split.Length > 1 ? split[0] : "Vanilla";
                            if (version != "Vanilla") customSoftware++;
                            if (!software.TryGetValue(version, out _))
                                software.Add(version, 1);
                            else software[version] += 1;
                        }

                        if (server.Ping.Version?.Protocol != null && 
                            Resources.Protocol.TryGetValue(server.Ping.Version.Protocol.Value, out var mapping)) {
                            if (!versions.TryGetValue(mapping, out _)) versions.Add(mapping, 1);
                            else versions[mapping] += 1;
                        }
                        
                        if (server.Ping.ModernForgeMods?.ModList != null)
                            foreach (var mod in server.Ping.ModernForgeMods.ModList) {
                                if (mod.ModId == null) continue;
                                if (mod.ModId is not "minecraft" and not "mcp" and not "forge" and not "FML")
                                    if (!forgeMods.TryGetValue(mod.ModId, out _)) forgeMods.Add(mod.ModId, 1);
                                    else forgeMods[mod.ModId] += 1;
                            }
                        
                        if (server.Ping.LegacyForgeMods?.ModList != null)
                            foreach (var mod in server.Ping.LegacyForgeMods.ModList) {
                                if (mod.ModId == null) continue;
                                if (mod.ModId is not "minecraft" and not "mcp" and not "forge" and not "FML")
                                    if (!forgeMods.TryGetValue(mod.ModId, out _)) forgeMods.Add(mod.ModId, 1);
                                    else forgeMods[mod.ModId] += 1;
                            }
                        
                        if (city.TryCity(server.IP, out var result2) && result2?.Country.IsoCode != null) {
                            var value = (result2.Country.IsoCode, result2.City.Name ?? "Unknown");
                            if (!cities.TryGetValue(value, out _)) cities.Add(value, 1);
                            else cities[value] += 1;
                        }

                        if (asn.TryAsn(server.IP, out var result3) && result3?.AutonomousSystemOrganization != null) {
                            var value = ($"AS{result3.AutonomousSystemNumber}", result3.AutonomousSystemOrganization);
                            if (!asns.TryGetValue(value, out _)) asns.Add(value, 1);
                            else asns[value] += 1;
                        }
                    }
                
                _servers.WithLabels("custom").Set(customSoftware);
                _servers.WithLabels("anti_ddos").Set(antiDDoS);
                foreach (var item in software) _software.WithLabels(item.Key).Set(item.Value);
                foreach (var item in versions) _versions.WithLabels(item.Key).Set(item.Value);
                foreach (var item in forgeMods) _forgeMods.WithLabels(item.Key).Set(item.Value);
                foreach (var item in cities) _cities.WithLabels(item.Key.Item1, item.Key.Item2).Set(item.Value);
                foreach (var item in asns) _asns.WithLabels(item.Key.Item1, item.Key.Item2).Set(item.Value);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception e) {
                Log.Error("Statistics processor thread crashed: {0}", e);
            }

            watch.Stop();
            var period = TimeSpan.FromMinutes(1);
            if (watch.Elapsed > period) {
                Log.Warning("Statistics collection took too much time: {0}", watch.Elapsed);
                continue;
            }
            
            await Task.Delay(period - watch.Elapsed, token);
        }
    }
}