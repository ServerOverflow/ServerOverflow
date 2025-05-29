using System.Diagnostics;
using System.Net;
using Prometheus;
using Serilog;
using Serilog.Events;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;
using ServerOverflow.Snooper;
using ServerOverflow.Snooper.Workers;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting ServerOverflow Snooper");
Metrics.SuppressDefaultMetrics();
Database.Initialize(Configuration.MongoUri);
MinecraftBot.JoinProxy = new WebProxy(Configuration.ProxyUrl)
    { Credentials = new NetworkCredential(Configuration.ProxyUsername, Configuration.ProxyPassword) };

Log.Information("Starting background services");
new OfflineWorker().Start();
//new OnlineWorker().Start();
var server = new MetricServer(8008);
Trace.Listeners.Clear();
try {
    server.Start();
} catch (Exception e) {
    Log.Error("Failed to start Prometheus server: {}", e);
}

Log.Information("Snooper is now running");
await Task.Delay(-1);