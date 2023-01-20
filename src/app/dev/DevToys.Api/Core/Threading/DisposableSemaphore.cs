namespace DevToys.Api;

/// <summary>
/// Represents a semaphore that free other threads when disposing the result of the <see cref="WaitAsync(CancellationToken)"/> method..
/// </summary>
[DebuggerDisplay($"IsBusy = {{{nameof(IsBusy)}}}, Disposed = {{{nameof(Disposed)}}}")]
public sealed class DisposableSemaphore : IDisposable
{
    private readonly object _lockObject = new();
    private readonly SemaphoreSlim _semaphore;

    public bool Disposed { get; private set; }

    public bool IsBusy => _semaphore.CurrentCount == 0;

    public DisposableSemaphore(int maxTasksCount = 1)
    {
        _semaphore = new SemaphoreSlim(maxTasksCount, maxTasksCount);
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            if (!Disposed)
            {
                Disposed = true;
                _semaphore.Dispose();
            }
        }
    }

    public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        return new DummyDisposable(_semaphore);
    }

    private sealed class DummyDisposable : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        internal DummyDisposable(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}
