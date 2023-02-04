﻿using Microsoft.UI.Dispatching;

namespace DevToys.UI.Framework.Threading;

public static class DispatcherQueueExtensions
{
    public static void ThrowIfNotOnUIThread(this DispatcherQueue dispatcherQueue)
    {
        if (!dispatcherQueue.HasThreadAccess)
        {
            throw new Exception("The UI thread is expected, but the current call stack is running on another thread.");
        }
    }

    public static void ThrowIfOnUIThread(this DispatcherQueue dispatcherQueue)
    {
        if (dispatcherQueue.HasThreadAccess)
        {
            throw new Exception("The UI thread is not expected, but the current call stack is running on UI thread.");
        }
    }

    public static Task RunOnUIThreadAsync(this DispatcherQueue dispatcherQueue, Action action)
    {
        return RunOnUIThreadAsync(dispatcherQueue, DispatcherQueuePriority.Normal, action);
    }

    public static Task RunOnUIThreadAsync(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority, Action action)
    {
        if (action is null)
        {
            return Task.CompletedTask;
        }

        if (dispatcherQueue.HasThreadAccess && priority == DispatcherQueuePriority.Normal)
        {
            action();
            return Task.CompletedTask;
        }
        else
        {
            var tcs = new TaskCompletionSource<int>(0);
            dispatcherQueue.TryEnqueue(
                priority,
                () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                    finally
                    {
                        tcs.TrySetResult(0);
                    }
                });
            return tcs.Task;
        }
    }

    public static Task<T> RunOnUIThreadAsync<T>(this DispatcherQueue dispatcherQueue, Func<T> func)
    {
        return RunOnUIThreadAsync(dispatcherQueue, DispatcherQueuePriority.Normal, func);
    }

    public static Task<T> RunOnUIThreadAsync<T>(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority, Func<T> func)
    {
        if (func is null)
        {
            return Task.FromResult<T>(default!);
        }

        if (dispatcherQueue.HasThreadAccess && priority == DispatcherQueuePriority.Normal)
        {
            return Task.FromResult(func());
        }
        else
        {
            var tcs = new TaskCompletionSource<T>();
            dispatcherQueue.TryEnqueue(
                priority,
                () =>
                {
                    T result = default!;
                    try
                    {
                        result = func();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                    finally
                    {
                        tcs.TrySetResult(result);
                    }
                });
            return tcs.Task;
        }
    }

    public static Task RunOnUIThreadAsync(this DispatcherQueue dispatcherQueue, Func<Task> action)
    {
        return RunOnUIThreadAsync(dispatcherQueue, DispatcherQueuePriority.Normal, action);
    }

    public static async Task RunOnUIThreadAsync(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority, Func<Task> action)
    {
        if (action is null)
        {
            return;
        }

        if (dispatcherQueue.HasThreadAccess && priority == DispatcherQueuePriority.Normal)
        {
            await action().ConfigureAwait(true);
        }
        else
        {
            var tcs = new TaskCompletionSource<int>(0);
            dispatcherQueue.TryEnqueue(
                priority,
                async () =>
                {
                    try
                    {
                        ThrowIfNotOnUIThread(dispatcherQueue);
                        await action().ConfigureAwait(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                    finally
                    {
                        tcs.TrySetResult(0);
                    }
                });

            await tcs.Task.ConfigureAwait(false);
        }
    }

    public static Task<T> RunOnUIThreadAsync<T>(this DispatcherQueue dispatcherQueue, Func<Task<T>> action)
    {
        return RunOnUIThreadAsync(dispatcherQueue, DispatcherQueuePriority.Normal, action);
    }

    public static async Task<T> RunOnUIThreadAsync<T>(this DispatcherQueue dispatcherQueue, DispatcherQueuePriority priority, Func<Task<T>> action)
    {
        Guard.IsNotNull(action);

        if (dispatcherQueue.HasThreadAccess && priority == DispatcherQueuePriority.Normal)
        {
            return await action().ConfigureAwait(true);
        }
        else
        {
            T result = default!;
            var tcs = new TaskCompletionSource<int>(0);
            dispatcherQueue.TryEnqueue(
                priority,
                async () =>
                {
                    try
                    {
                        ThrowIfNotOnUIThread(dispatcherQueue);
                        result = await action().ConfigureAwait(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                    finally
                    {
                        tcs.TrySetResult(0);
                    }
                });

            await tcs.Task.ConfigureAwait(false);
            return result!;
        }
    }
}
