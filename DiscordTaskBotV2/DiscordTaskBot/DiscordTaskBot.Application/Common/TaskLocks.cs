namespace DiscordTaskBot.Application;

using System.Collections.Concurrent;

/// <summary>
/// One lock per task, so read-modify-write operations on the same task (clicks, edits) run one after another
/// instead of overwriting each other. The bot is a single process, so an in-memory lock is enough.
/// </summary>
public class TaskLocks
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(Guid taskId)
    {
        var semaphore = _locks.GetOrAdd(taskId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
                semaphore.Release();
        }
    }
}
