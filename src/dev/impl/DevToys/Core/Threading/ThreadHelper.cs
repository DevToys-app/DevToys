#nullable enable

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace DevToys.Core.Threading
{
    internal static class ThreadHelper
    {
        private static readonly CoreDispatcher _uiDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        internal static void ThrowIfNotOnUIThread()
        {
            if (!_uiDispatcher.HasThreadAccess)
            {
                throw new Exception("The UI thread is expected, but the current call stack is running on another thread.");
            }
        }

        internal static void ThrowIfOnUIThread()
        {
            if (_uiDispatcher.HasThreadAccess)
            {
                throw new Exception("The UI thread is not expected, but the current call stack is running on UI thread.");
            }
        }

        internal static Task RunOnUIThreadAsync(Action action)
        {
            return RunOnUIThreadAsync(ThreadPriority.Normal, action);
        }

        internal static Task RunOnUIThreadAsync(ThreadPriority priority, Action action)
        {
            if (action is null)
            {
                return Task.CompletedTask;
            }

            if (_uiDispatcher.HasThreadAccess)
            {
                action();
                return Task.CompletedTask;
            }
            else
            {
                return _uiDispatcher.RunAsync(GetDispatcherPriority(priority), () => action()).AsTask();
            }
        }

        internal static Task RunOnUIThreadAsync(Func<Task> action)
        {
            return RunOnUIThreadAsync(ThreadPriority.Normal, action);
        }

        internal static async Task RunOnUIThreadAsync(ThreadPriority priority, Func<Task> action)
        {
            if (action is null)
            {
                return;
            }

            if (_uiDispatcher.HasThreadAccess)
            {
                await action().ConfigureAwait(true);
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();
                await _uiDispatcher.RunAsync(
                    GetDispatcherPriority(priority),
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            tcs.TrySetResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        }
                    });

                await tcs.Task.ConfigureAwait(false);
            }
        }

        internal static Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> action)
        {
            return RunOnUIThreadAsync(ThreadPriority.Normal, action);
        }

        internal static async Task<T> RunOnUIThreadAsync<T>(ThreadPriority priority, Func<Task<T>> action)
        {
            Arguments.NotNull(action, nameof(action));

            if (_uiDispatcher.HasThreadAccess)
            {
                return await action().ConfigureAwait(true);
            }
            else
            {
                T result = default!;
                var tcs = new TaskCompletionSource<object>();
                _uiDispatcher.RunAsync(
                    GetDispatcherPriority(priority), async () =>
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            tcs.TrySetResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        }
                    }).AsTask().ForgetSafely();

                await tcs.Task.ConfigureAwait(false);
                return result!;
            }
        }

        private static CoreDispatcherPriority GetDispatcherPriority(ThreadPriority priority)
        {
            return priority switch
            {
                ThreadPriority.Low => CoreDispatcherPriority.Low,
                ThreadPriority.Normal => CoreDispatcherPriority.Normal,
                ThreadPriority.High => CoreDispatcherPriority.High,
                _ => throw new NotSupportedException(),
            };
        }
    }
}
