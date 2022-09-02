#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using DevToys.Shared.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.Core
{
    public abstract class QueueWorkerViewModelBase<T> : ObservableRecipient where T : class
    {
        private readonly object _lock = new();
        private readonly Queue<T> _computationQueue = new();

        private bool _computationInProgress;

        internal Task ComputationTask { get; private set; } = Task.CompletedTask;

        protected void EnqueueComputation(T value)
        {
            lock (_lock)
            {
                _computationQueue.Enqueue(value);

                if (!_computationInProgress)
                {
                    _computationInProgress = true;
                    ComputationTask = TreatComputationQueueAsync();
                }
            }
        }

        protected abstract Task TreatComputationQueueAsync(T value);

        private async Task TreatComputationQueueAsync()
        {
            await TaskScheduler.Default;

            while (_computationQueue.TryDequeue(out T value))
            {
                await TreatComputationQueueAsync(value).ConfigureAwait(false);

                lock (_lock)
                {
                    if (_computationQueue.Count == 0)
                    {
                        _computationInProgress = false;
                    }
                }
            }
        }
    }
}
