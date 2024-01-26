namespace DevToys.Api;

/// <summary>
/// Represents a semaphore that free other threads when disposing the result of the <see cref="WaitAsync(CancellationToken)"/> method.
/// </summary>
[DebuggerDisplay($"IsBusy = {{{nameof(IsBusy)}}}, Disposed = {{{nameof(Disposed)}}}")]
public sealed class DisposableSemaphore : IDisposable
{
    private readonly object _lockObject = new();
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Gets a value indicating whether the semaphore has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the semaphore is currently busy.
    /// </summary>
    public bool IsBusy => _semaphore.CurrentCount == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableSemaphore"/> class.
    /// </summary>
    /// <param name="maxTasksCount">The maximum number of concurrent tasks that can be executed.</param>
    public DisposableSemaphore(int maxTasksCount = 1)
    {
        _semaphore = new SemaphoreSlim(maxTasksCount, maxTasksCount);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="DisposableSemaphore"/>.
    /// </summary>
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

    /// <summary>
    /// Asynchronously waits for the semaphore to be available.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="IDisposable"/> object that should be disposed to release the semaphore.</returns>
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

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}
