using System.Net;
using Prometheus;
using Serilog;
using Serilog.Events;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;
using ServerOverflow.Snooper;
using static ServerOverflow.Snooper.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting ServerOverflow Snooper");
Metrics.SuppressDefaultMetrics();
Database.Initialize(Config.MongoUri);
MinecraftBot.JoinProxy = new WebProxy(Config.ProxyUrl)
    { Credentials = new NetworkCredential(Config.ProxyUsername, Config.ProxyPassword) };

Log.Information("Starting background threads");
new Thread(async () => await BotWorker.MainThread()).Start();
var server = new MetricServer(8008);
try {
    server.Start();
} catch (Exception e) {
    Log.Error("Failed to start Prometheus server: {}", e);
}

Log.Information("Snooper is now running");
await Task.Delay(-1);