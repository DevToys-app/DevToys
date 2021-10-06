#nullable enable

using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace DevToys.MonacoEditor.Extensions
{
    internal static class DispatcherExtensions
    {
        internal static Task RunOnUIThreadAsync(this CoreDispatcher dispatcher, Action action)
        {
            return dispatcher.RunOnUIThreadAsync(ThreadPriority.Normal, action);
        }

        internal static Task RunOnUIThreadAsync(this CoreDispatcher dispatcher, ThreadPriority priority, Action action)
        {
            if (action is null)
            {
                return Task.CompletedTask;
            }

            if (dispatcher.HasThreadAccess)
            {
                action();
                return Task.CompletedTask;
            }
            else
            {
                return dispatcher.RunAsync(GetDispatcherPriority(priority), () => action()).AsTask();
            }
        }

        internal static Task RunOnUIThreadAsync(this CoreDispatcher dispatcher, Func<Task> action)
        {
            return dispatcher.RunOnUIThreadAsync(ThreadPriority.Normal, action);
        }

        internal static async Task RunOnUIThreadAsync(this CoreDispatcher dispatcher, ThreadPriority priority, Func<Task> action)
        {
            if (action is null)
            {
                return;
            }

            if (dispatcher.HasThreadAccess)
            {
                await action().ConfigureAwait(true);
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();
                await dispatcher.RunAsync(
                    GetDispatcherPriority(priority),
                    async () =>
                    {
                        try
                        {
                            await action().ConfigureAwait(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                        finally
                        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            tcs.TrySetResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        }
                    });

                await tcs.Task.ConfigureAwait(false);
            }
        }

        internal static Task<T> RunOnUIThreadAsync<T>(this CoreDispatcher dispatcher, Func<Task<T>> action)
        {
            return dispatcher.RunOnUIThreadAsync(ThreadPriority.Normal, action);
        }

        internal static async Task<T> RunOnUIThreadAsync<T>(this CoreDispatcher dispatcher, ThreadPriority priority, Func<Task<T>> action)
        {
            if (dispatcher.HasThreadAccess)
            {
                return await action().ConfigureAwait(true);
            }
            else
            {
                T result = default!;
                var tcs = new TaskCompletionSource<object>();
                _ = dispatcher.RunAsync(
                    GetDispatcherPriority(priority), async () =>
                    {
                        try
                        {
                            result = await action().ConfigureAwait(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                        finally
                        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            tcs.TrySetResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        }
                    }).AsTask();

                await tcs.Task.ConfigureAwait(false);
                return result!;
            }
        }

        private static CoreDispatcherPriority GetDispatcherPriority(ThreadPriority priority)
        {
            switch (priority)
            {
                case ThreadPriority.Low:
                    return CoreDispatcherPriority.Low;
                case ThreadPriority.Normal:
                    return CoreDispatcherPriority.Normal;
                case ThreadPriority.High:
                    return CoreDispatcherPriority.High;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
