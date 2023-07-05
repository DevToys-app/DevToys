using System.Windows.Threading;
using DevToys.Api;
using Application = System.Windows.Application;

namespace DevToys.Windows.Helpers;

public static class ThreadHelper
{
    private static readonly Dispatcher dispatcher = Application.Current.Dispatcher;

    public static void ThrowIfNotOnUIThread()
    {
        if (!IsOnUIThread())
        {
            throw new Exception("The UI thread is expected, but the current call stack is running on another thread.");
        }
    }

    public static void ThrowIfOnUIThread()
    {
        if (IsOnUIThread())
        {
            throw new Exception("The UI thread is not expected, but the current call stack is running on UI thread.");
        }
    }

    public static Task RunOnUIThreadAsync(Action action)
    {
        return RunOnUIThreadAsync(DispatcherPriority.Normal, action);
    }

    public static Task RunOnUIThreadAsync(DispatcherPriority priority, Action action)
    {
        if (action is null)
        {
            return Task.CompletedTask;
        }

        if (IsOnUIThread() && priority == DispatcherPriority.Normal)
        {
            action();
            return Task.CompletedTask;
        }
        else
        {
            var tcs = new TaskCompletionSource<int>(0);
            dispatcher.BeginInvoke(
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
                },
                priority);
            return tcs.Task;
        }
    }

    public static Task<T> RunOnUIThreadAsync<T>(Func<T> func)
    {
        return RunOnUIThreadAsync(DispatcherPriority.Normal, func);
    }

    public static Task<T> RunOnUIThreadAsync<T>(DispatcherPriority priority, Func<T> func)
    {
        if (func is null)
        {
            return Task.FromResult<T>(default!);
        }

        if (IsOnUIThread() && priority == DispatcherPriority.Normal)
        {
            return Task.FromResult(func());
        }
        else
        {
            var tcs = new TaskCompletionSource<T>();
            dispatcher.BeginInvoke(
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
                },
                priority);
            return tcs.Task;
        }
    }

    public static Task RunOnUIThreadAsync(Func<Task> action)
    {
        return RunOnUIThreadAsync(DispatcherPriority.Normal, action);
    }

    public static async Task RunOnUIThreadAsync(DispatcherPriority priority, Func<Task> action)
    {
        if (action is null)
        {
            return;
        }

        if (IsOnUIThread() && priority == DispatcherPriority.Normal)
        {
            await action().ConfigureAwait(true);
        }
        else
        {
            var tcs = new TaskCompletionSource<int>(0);
            dispatcher.BeginInvoke(
                async () =>
                {
                    try
                    {
                        ThrowIfNotOnUIThread();
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
                },
                priority).Task.Forget();

            await tcs.Task.ConfigureAwait(false);
        }
    }

    public static Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> action)
    {
        return RunOnUIThreadAsync(DispatcherPriority.Normal, action);
    }

    public static async Task<T> RunOnUIThreadAsync<T>(DispatcherPriority priority, Func<Task<T>> action)
    {
        Guard.IsNotNull(action);

        if (IsOnUIThread() && priority == DispatcherPriority.Normal)
        {
            return await action().ConfigureAwait(true);
        }
        else
        {
            T result = default!;
            var tcs = new TaskCompletionSource<int>(0);
            dispatcher.BeginInvoke(
                async () =>
                {
                    try
                    {
                        ThrowIfNotOnUIThread();
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
                },
                priority).Task.Forget();

            await tcs.Task.ConfigureAwait(false);
            return result!;
        }
    }

    private static bool IsOnUIThread()
    {
        return dispatcher.Thread == Thread.CurrentThread;
    }
}
