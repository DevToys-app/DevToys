﻿#nullable enable

using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.Core
{
    public abstract class QueueWorkerViewModelBase<T> : ObservableRecipient
    {
        private readonly object _lock = new();
        private readonly Queue<T> _computationQueue = new();

        private bool _computationInProgress;

        internal bool ComputationTask { get; private set; }

        protected void EnqueueComputation(T value)
        {
            lock (_lock)
            {
                _computationQueue.Enqueue(value);

                if (!_computationInProgress)
                {
                    _computationInProgress = true;
                    ComputationTask = TreatComputationQueue();
                }
            }
        }

        protected abstract void TreatComputationQueue(T value);

        private bool TreatComputationQueue()
        {
            while (_computationQueue.TryDequeue(out T value))
            {
                TreatComputationQueue(value);

                lock (_lock)
                {
                    if (_computationQueue.Count == 0)
                    {
                        _computationInProgress = false;
                        return true;
                    }
                }
            }
            return true;
        }
    }
}
