// A fixed pool of background threads consuming a work queue. Threads are created once at
// startup (avoiding per-request thread spin-up) and run until the pool is disposed. Each
// thread owns a single, reused PokerMonteCarloServer instance instead of a new one being
// allocated per job.
//
// Admission control is done with an atomic reservation counter (_reserved), not with the
// queue's own capacity: EquityService reserves all of a request's sub-tasks in one atomic
// step before enqueuing any of them. Once a request is admitted this way, every one of its
// children is guaranteed a slot and Enqueue cannot fail - the underlying queue is unbounded
// so a batch is never left partially enqueued because the queue "filled up" mid-loop.
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using PokerCalculator.Api.Options;

namespace PokerCalculator.Api.Simulation;

// A unit of work applied to the calling worker's own PokerMonteCarloServer instance:
// it configures the server for the job at hand and runs the matching Simulate* method.
public delegate void SimulationJob(PokerMonteCarloServer server);

public class SimulationWorkerPool : IDisposable
{
    private readonly BlockingCollection<SimulationJob> _queue = new();
    private readonly List<Thread> _workers = new();
    private int _reserved;

    public int MaxQueueSize { get; }

    public SimulationWorkerPool(IOptions<SimulationOptions> options)
    {
        var opt = options.Value;
        MaxQueueSize = opt.MaxQueueSize;

        for (int i = 0; i < opt.WorkerCount; i++)
        {
            var thread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = $"SimWorker-{i}"
            };
            thread.Start();
            _workers.Add(thread);
        }
    }

    // Number of jobs currently reserved: queued plus in-flight on a worker. Used both for
    // the admission decision and as the load signal that drives simulation-count reduction.
    public int PendingCount => Volatile.Read(ref _reserved);

    // Atomically reserves `count` slots if doing so would not exceed MaxQueueSize. All-or-
    // nothing: either the whole batch is reserved, or none of it is.
    public bool TryReserve(int count)
    {
        while (true)
        {
            int current = Volatile.Read(ref _reserved);
            int next = current + count;
            if (next > MaxQueueSize) return false;
            if (Interlocked.CompareExchange(ref _reserved, next, current) == current) return true;
        }
    }

    // Enqueues a job that was already reserved via TryReserve. The queue itself is
    // unbounded, so this always succeeds - the reservation is what enforces the limit.
    public void Enqueue(SimulationJob job) => _queue.Add(job);

    private void WorkerLoop()
    {
        var server = new PokerMonteCarloServer();

        foreach (var job in _queue.GetConsumingEnumerable())
        {
            try
            {
                job(server);
            }
            catch
            {
                // A failed sub-task must not take the worker thread down; the caller's
                // TaskCompletionSource is left unset and will surface as a timeout/fault.
            }
            finally
            {
                Interlocked.Decrement(ref _reserved);
            }
        }
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
    }
}
