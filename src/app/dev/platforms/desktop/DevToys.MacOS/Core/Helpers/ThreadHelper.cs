using DevToys.Api;

namespace DevToys.MacOS.Helpers;

public static class ThreadHelper
{
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
            MainThread.InvokeOnMainThreadAsync(
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

    public static Task<T> RunOnUIThreadAsync<T>(Func<T> func)
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
            MainThread.InvokeOnMainThreadAsync(
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

    public static async Task RunOnUIThreadAsync(Func<Task> action)
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
            MainThread.InvokeOnMainThreadAsync(
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
                }).Forget();

            await tcs.Task.ConfigureAwait(false);
        }
    }

    public static async Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> action)
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
            MainThread.InvokeOnMainThreadAsync(
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
                }).Forget();

            await tcs.Task.ConfigureAwait(false);
            return result!;
        }
    }

    private static bool IsOnUIThread()
    {
        return MainThread.IsMainThread;
    }
}
