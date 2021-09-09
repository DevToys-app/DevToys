#nullable enable

using DevTools.Core.Threading;
using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace DevTools.Core.Impl.Threading
{
    [Export(typeof(IThread))]
    [Shared]
    internal sealed class Thread : IThread
    {
        private readonly static CoreDispatcher _uiDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        public void ThrowIfNotOnUIThread()
        {
            if (!_uiDispatcher.HasThreadAccess)
            {
                throw new Exception("The UI thread is expected, but the current call stack is running on another thread.");
            }
        }

        public void ThrowIfOnUIThread()
        {
            if (_uiDispatcher.HasThreadAccess)
            {
                throw new Exception("The UI thread is not expected, but the current call stack is running on UI thread.");
            }
        }

        public Task RunOnUIThreadAsync(Action action)
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
                return _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
            }
        }

        public async Task RunOnUIThreadAsync(Func<Task> action)
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
                    CoreDispatcherPriority.Normal,
                    async() =>
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

        public async Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> action)
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
                    CoreDispatcherPriority.Normal, async () =>
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
    }
}
