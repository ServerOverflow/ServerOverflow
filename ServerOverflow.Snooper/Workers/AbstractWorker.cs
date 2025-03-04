using Serilog;

namespace ServerOverflow.Snooper.Workers;

/// <summary>
/// Abstract task worker
/// </summary>
public abstract class AbstractWorker {
    /// <summary>
    /// Task queue
    /// </summary>
    public Queue<Task> Tasks { get; } = [];

    /// <summary>
    /// Task pool size
    /// </summary>
    public int PoolSize { get; }

    /// <summary>
    /// Task fetcher batch size
    /// </summary>
    public int BatchSize { get; set; }

    /// <summary>
    /// How long to wait after failing to fetch any items
    /// </summary>
    public TimeSpan RestartDelay { get; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Current tasks array
    /// </summary>
    private readonly Task[] _tasks;

    /// <summary>
    /// Creates a new abstract worker
    /// </summary>
    /// <param name="poolSize">Pool size</param>
    protected AbstractWorker(int poolSize) {
        _tasks = new Task[poolSize];
        PoolSize = poolSize;
        BatchSize = poolSize * 3;
    }

    /// <summary>
    /// Starts the worker thread
    /// </summary>
    public virtual void Start() {
        new Thread(async () => await FetcherTask()).Start();
        new Thread(async () => await WorkerTask()).Start();
    }
    
    /// <summary>
    /// Fetches tasks ahead of time
    /// </summary>
    private async Task FetcherTask() {
        while (true) {
            while (Tasks.Count >= BatchSize) await Task.Delay(10);
            
            try {
                var batch = await FetchTasks();
                Log.Debug("[{0}] Fetched batch of size {1}", GetType().Name, batch.Count);
                if (batch.Count == 0) {
                    while (Tasks.Count > 0) await Task.Delay(10);
                    await Task.Delay(RestartDelay);
                    continue;
                }
                
                foreach (var task in batch)
                    Tasks.Enqueue(task);
            } catch (Exception e) {
                Log.Error("Failed to fetch next batch: {0}", e);
                await Task.Delay(60000);
            }
        }
    }

    /// <summary>
    /// Runs tasks on pool
    /// </summary>
    private async Task WorkerTask() {
        while (true) {
            while (Tasks.Count < PoolSize) {
                while (Tasks.Count == 0) await Task.Delay(10);
                await Task.Delay(1000);
                if (Tasks.Count >= PoolSize) break;
                Log.Debug("[{0}] Not enough items in queue, awaiting entire queue of size {1}", GetType().Name, Tasks.Count);
                var temp = new List<Task>();
                while (Tasks.TryDequeue(out var task))
                    temp.Add(task);
                await Task.WhenAll(temp);
                try {
                    await Cleanup();
                } catch (Exception e) {
                    Log.Error("Failed to clean up after finishing: {0}", e);
                }
            }

            for (var i = 0; i < PoolSize; i++)
                _tasks[i] = Tasks.Dequeue();

            Log.Debug("[{0}] Pool of size {1} filled, awaiting efficiently", GetType().Name, _tasks.Length);
            while (true) {
                var idx = Task.WaitAny(_tasks);
                if (Tasks.Count == 0) {
                    Log.Debug("[{0}] Queue was emptied, awaiting leftover tasks", GetType().Name);
                    await Task.WhenAll(_tasks);
                    try {
                        await Cleanup();
                    } catch (Exception e) {
                        Log.Error("Failed to clean up after finishing: {0}", e);
                    }
                    
                    await Task.Delay(RestartDelay);
                    continue;
                }
            
                _tasks[idx] = Tasks.Dequeue();
            }
        }
    }
    
    /// <summary>
    /// Fetches tasks of batch size and puts them into the queue
    /// </summary>
    /// <returns>List of tasks</returns>
    protected virtual Task<List<Task>> FetchTasks() => throw new NotImplementedException();

    /// <summary>
    /// Performs cleanup after all tasks finish
    /// </summary>
    protected virtual Task Cleanup() => Task.CompletedTask;
}