using System.Runtime.CompilerServices;

namespace DevToys.Api;

/// <summary>
/// An awaiter returned from <see cref="GetAwaiter()"/>.
/// </summary>
public readonly struct TaskSchedulerAwaiter : ICriticalNotifyCompletion
{
    /// <summary>
    /// Switches the execution context off the main thread asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="TaskSchedulerAwaiter"/> configured to the appropriate <see cref="TaskScheduler"/>.</returns>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public static TaskSchedulerAwaiter SwitchOffMainThreadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new TaskSchedulerAwaiter(
            SynchronizationContext.Current != null ? TaskScheduler.Default : TaskScheduler.Current,
            cancellationToken);
    }

    /// <summary>
    /// The scheduler for continuations.
    /// </summary>
    private readonly TaskScheduler _scheduler;

    /// <summary>
    /// A value indicating whether <see cref="IsCompleted"/>
    /// should always return false.
    /// </summary>
    private readonly bool _alwaysYield;

    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskSchedulerAwaiter"/> struct.
    /// </summary>
    /// <param name="scheduler">The scheduler for continuations.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="alwaysYield">A value indicating whether the caller should yield even if
    /// already executing on the desired task scheduler.</param>
    public TaskSchedulerAwaiter(TaskScheduler scheduler, CancellationToken cancellationToken, bool alwaysYield = false)
    {
        _scheduler = scheduler;
        _cancellationToken = cancellationToken;
        _alwaysYield = alwaysYield;
    }

    /// <summary>
    /// Gets the <see cref="TaskSchedulerAwaiter"/>.
    /// </summary>
    /// <returns>
    /// Returns this instance.
    /// </returns>
    public TaskSchedulerAwaiter GetAwaiter()
    {
        return this;
    }

    /// <summary>
    /// Gets a value indicating whether no yield is necessary.
    /// </summary>
    /// <value><c>true</c> if the caller is already running on that TaskScheduler.</value>
    public bool IsCompleted
    {
        get
        {
            if (_alwaysYield)
            {
                return false;
            }

            // We special case the TaskScheduler.Default since that is semantically equivalent to being
            // on a ThreadPool thread, and there are various ways to get on those threads.
            // TaskScheduler.Current is never null.  Even if no scheduler is really active and the current
            // thread is not a thread pool thread, TaskScheduler.Current == TaskScheduler.Default, so we have
            // to protect against that case too.
            bool isThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
            return (_scheduler == TaskScheduler.Default && isThreadPoolThread)
                || (_scheduler == TaskScheduler.Current && TaskScheduler.Current != TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Schedules a continuation to execute using the specified task scheduler.
    /// </summary>
    /// <param name="continuation">The delegate to invoke.</param>
    public void OnCompleted(Action continuation)
    {
        if (_scheduler == TaskScheduler.Default)
        {
            ThreadPool.QueueUserWorkItem(state => ((Action)state!)(), continuation);
        }
        else
        {
            Task.Factory.StartNew(continuation, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }
    }

    /// <summary>
    /// Schedules a continuation to execute using the specified task scheduler
    /// without capturing the ExecutionContext.
    /// </summary>
    /// <param name="continuation">The action.</param>
    public void UnsafeOnCompleted(Action continuation)
    {
        if (_scheduler == TaskScheduler.Default)
        {
            ThreadPool.UnsafeQueueUserWorkItem(state => ((Action)state!)(), continuation);
        }
        else
        {
            Task.Factory.StartNew(continuation, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public void GetResult()
    {
        _cancellationToken.ThrowIfCancellationRequested();
    }
}
