using System.Collections.Concurrent;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Snooper.Workers;

/// <summary>
/// Offline mode bot join worker
/// </summary>
public class OfflineWorker : AbstractWorker, IDisposable {
    /// <summary>
    /// MongoDB cursor
    /// </summary>
    private IAsyncCursor<Server>? _cursor;

    /// <summary>
    /// Database write requests bag
    /// </summary>
    private readonly ConcurrentBag<WriteModel<Server>> _requests = [];

    /// <summary>
    /// Creates a new offline mode bot join worker
    /// </summary>
    public OfflineWorker() : base(1000) {
        BatchSize = 5000;
        CreateCursor();
    }

    /// <summary>
    /// Starts the worker thread
    /// </summary> 
    public override void Start() {
        new Thread(async () => await WriteTask()).Start();
        base.Start();
    }

    /// <summary>
    /// Creates a MongoDB cursor
    /// </summary>
    private void CreateCursor() {
        Cleanup().GetAwaiter().GetResult();
        var builder = Builders<Server>.Filter;
        var filter = builder.Eq(x => x.JoinResult, null) 
            | builder.Lt(x => x.JoinResult!.Timestamp, 
                DateTime.UtcNow - TimeSpan.FromDays(1));
        
        var projection = Builders<Server>.Projection
            .Include(x => x.JoinResult)
            .Include(x => x.Ping.IsForge)
            .Include(x => x.Ping.Version!.Protocol)
            .Include(x => x.Ping.ModernForgeMods!.ProtocolVersion)
            .Include(x => x.Port)
            .Include(x => x.IP);
        
        _cursor = Database.Servers.FindSync(filter,
            new FindOptions<Server> {
                Projection = projection, 
                BatchSize = BatchSize
            });
    }
    
    
    /// <summary>
    /// Connects to a Minecraft server
    /// </summary>
    /// <param name="server">Server</param>
    /// <returns>Result</returns>
    private async Task Connect(Server server) {
        var result = await MinecraftBot.Join(server);
        Statistics.ServersTotal.WithLabels("offline", result.Success ? "true" : "false").Inc();
        result.LastSeen ??= server.JoinResult?.LastSeen;
        result.Whitelist ??= server.JoinResult?.Whitelist;
        result.OnlineMode ??= server.JoinResult?.OnlineMode;
        result.OnlineTimestamp ??= server.JoinResult?.OnlineTimestamp;
        _requests.Add(new UpdateOneModel<Server>(
            Builders<Server>.Filter.Eq(y => y.Id, server.Id),
            Builders<Server>.Update.Set(x => x.JoinResult, result)));
    }

    /// <summary>
    /// Bulk writer task
    /// </summary>
    private async Task WriteTask() {
        while (true) {
            while (_requests.IsEmpty) await Task.Delay(10);
            var requests = new List<WriteModel<Server>>();
            while (_requests.TryTake(out var request))
                requests.Add(request);
            if (requests.Count != 0)
                await Database.Servers.BulkWriteAsync(requests);
            await Task.Delay(60000);
        }
    }

    /// <summary>
    /// Fetches tasks of batch size and puts them into the queue
    /// </summary>
    /// <returns>List of tasks</returns>
    protected override async Task<List<Func<Task>>> FetchTasks() {
        if (_cursor == null) CreateCursor();
        if (!await _cursor!.MoveNextAsync()) {
            _cursor.Dispose();
            _cursor = null;
            return [];
        }
        
        var exclusions = await Exclusion.GetAll();
        return _cursor.Current
            .Where(x => exclusions.All(y => !y.IsExcluded(x.IP)))
            .Select(x => (Func<Task>)(() => Connect(x)))
            .ToList();
    }

    /// <summary>
    /// Writes all changes to the database
    /// </summary>
    protected override async Task Cleanup() {
        if (_requests.IsEmpty) return;
        await Database.Servers.BulkWriteAsync(_requests);
        _requests.Clear();
    }

    /// <summary>
    /// Disposes the MongoDB cursor
    /// </summary>
    public void Dispose() => _cursor?.Dispose();
}