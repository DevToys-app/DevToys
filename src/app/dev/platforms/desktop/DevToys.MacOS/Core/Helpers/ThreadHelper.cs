namespace DevToys.MacOS.Core.Helpers;

internal static class ThreadHelper
{
    internal static void ThrowIfNotOnUIThread()
    {
        if (!IsOnUIThread())
        {
            throw new Exception("The UI thread is expected, but the current call stack is running on another thread.");
        }
    }

    internal static void ThrowIfOnUIThread()
    {
        if (IsOnUIThread())
        {
            throw new Exception("The UI thread is not expected, but the current call stack is running on UI thread.");
        }
    }

    internal static Task RunOnUIThreadAsync(Action? action)
    {
        if (action is null)
        {
            return Task.CompletedTask;
        }

        if (IsOnUIThread())
        {
            action();
            return Task.CompletedTask;
        }
        else
        {
            var tcs = new TaskCompletionSource<int>(0);
            NSApplication.SharedApplication.BeginInvokeOnMainThread(
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

    internal static Task<T> RunOnUIThreadAsync<T>(Func<T>? func)
    {
        if (func is null)
        {
            return Task.FromResult<T>(default!);
        }

        if (IsOnUIThread())
        {
            return Task.FromResult(func());
        }
        else
        {
            var tcs = new TaskCompletionSource<T>();
            NSApplication.SharedApplication.BeginInvokeOnMainThread(
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

    internal static async Task RunOnUIThreadAsync(Func<Task>? action)
    {
        if (action is null)
        {
            return;
        }

        if (IsOnUIThread())
        {
            await action().ConfigureAwait(true);
        }
        else
        {
            var tcs = new TaskCompletionSource<int>(0);
            NSApplication.SharedApplication.BeginInvokeOnMainThread(
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
                });

            await tcs.Task.ConfigureAwait(false);
        }
    }

    internal static async Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> action)
    {
        Guard.IsNotNull(action);

        if (IsOnUIThread())
        {
            return await action().ConfigureAwait(true);
        }
        else
        {
            T result = default!;
            var tcs = new TaskCompletionSource<int>(0);
            NSApplication.SharedApplication.BeginInvokeOnMainThread(
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
                });

            await tcs.Task.ConfigureAwait(false);
            return result!;
        }
    }

    private static bool IsOnUIThread()
    {
        return NSThread.IsMain;
    }
}
