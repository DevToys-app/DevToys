#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevToys.Core.Threading
{
    /// <summary>
    /// Represents a semaphore that free other threads when disposing the result of the <see cref="WaitAsync(CancellationToken)"/> method..
    /// </summary>
    internal sealed class DisposableSempahore : IDisposable
    {
        private readonly object _lockObject = new();
        private readonly SemaphoreSlim _semaphore;

        internal bool Disposed { get; private set; }

        internal bool IsBusy => _semaphore.CurrentCount == 0;

        internal DisposableSempahore(int maxTasksCount = 1)
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

        internal async Task<IDisposable> WaitAsync(CancellationToken cancellationToken)
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
}
