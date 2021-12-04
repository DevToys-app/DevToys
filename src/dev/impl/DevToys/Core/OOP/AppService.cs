#nullable enable

using System;
using System.Collections.Concurrent;
using System.Composition;
using System.IO.Pipes;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Core.OOP;
using DevToys.OutOfProcService.OutOfProcServices;
using DevToys.Shared.Core;
using DevToys.Shared.Core.OOP;
using DevToys.Shared.Core.Threading;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;

namespace DevToys.Core.OOP
{
    [Export(typeof(IAppService))]
    [Shared]
    internal sealed class AppService : IAppService
    {
        private const string FullTrustAppContractName = "Windows.ApplicationModel.FullTrustAppContract";
        private const string PipeName = "LOCAL\\{0}";

        private readonly DisposableSempahore _sempahore = new();
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<AppServiceMessageBase>> _inProgressMessages = new();
        private readonly ConcurrentDictionary<Guid, IProgress<AppServiceProgressMessage>> _progressReporters = new();
        private NamedPipeServerStream? _pipeServerStream;

        [ImportingConstructor]
        public AppService()
        {
            App.Current.Suspending += OnAppSuspending;
        }

        public async Task SendMessageAsync(AppServiceMessageBase message)
        {
            await InternalSendMessageAndGetResponseAsync(
                waitForResult: false,
                message,
                DummyProgress.DefaultInstance,
                CancellationToken.None);
        }

        public Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message) where T : AppServiceMessageBase
        {
            return SendMessageAndGetResponseAsync<T>(message, CancellationToken.None);
        }

        public Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message, CancellationToken cancellationToken) where T : AppServiceMessageBase
        {
            return SendMessageAndGetResponseAsync<T>(message, DummyProgress.DefaultInstance, cancellationToken);
        }

        public Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message, IProgress<AppServiceProgressMessage> progress) where T : AppServiceMessageBase
        {
            return SendMessageAndGetResponseAsync<T>(message, progress, CancellationToken.None);
        }

        public async Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message, IProgress<AppServiceProgressMessage> progress, CancellationToken cancellationToken) where T : AppServiceMessageBase
        {
            Arguments.NotNull(progress, nameof(progress));

            AppServiceMessageBase? result
                = await InternalSendMessageAndGetResponseAsync(
                    waitForResult: true,
                    message,
                    progress,
                    cancellationToken);

            if (result is null)
            {
                throw new Exception("Unable to send a message to the app service.");
            }
            else if (result is AppServiceCancelMessage)
            {
                throw new OperationCanceledException("The message sent through the app service has been canceled.");
            }

            return (T)result;
        }

        private async void OnAppSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral? deferral = e.SuspendingOperation.GetDeferral();

            // Shut down the Win32 app.
            await SendMessageAsync(new ShutdownMessage());

            Disconnect();

            deferral.Complete();
        }

        private async Task<AppServiceMessageBase?> InternalSendMessageAndGetResponseAsync(bool waitForResult, AppServiceMessageBase message, IProgress<AppServiceProgressMessage> progress, CancellationToken cancellationToken)
        {
            Arguments.NotNull(message, nameof(message));
            Arguments.NotNull(progress, nameof(progress));

            try
            {
                var messageId = Guid.NewGuid();
                var messageCompletionSource = new TaskCompletionSource<AppServiceMessageBase>();

                using (await _sempahore.WaitAsync(cancellationToken))
                {
                    await TaskScheduler.Default;

                    // Make sure we're connected to the app service.
                    await ConnectAsync();
                    ThrowIfNotConnected();
                    cancellationToken.ThrowIfCancellationRequested();

                    // Give a unique ID to the message.
                    message.MessageId = messageId;

                    // start tracking the message.
                    if (waitForResult)
                    {
                        _inProgressMessages.TryAdd(message.MessageId.Value, messageCompletionSource);
                        _progressReporters.TryAdd(message.MessageId.Value, progress);
                    }

                    // Send the message.
                    SendMessage(message);
                }

                if (waitForResult)
                {
                    // Wait for the answer of the app service.
                    using CancellationTokenRegistration cancellationTokenRegistration
                        = cancellationToken.Register(() =>
                        {
                            OnSendMessageCanceledAsync(messageId).Forget();
                        });

                    AppServiceMessageBase result = await messageCompletionSource.Task;
                    _progressReporters.TryRemove(message.MessageId.Value, out _);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(AppService), ex, "Unable to send a message to the app service.");
            }

            return null;
        }

        private void SendMessage(AppServiceMessageBase message)
        {
            string jsonMessage = JsonConvert.SerializeObject(message, Shared.Constants.AppServiceJsonSerializerSettings);
            byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonMessage);

            _pipeServerStream!.Write(messageBuffer, 0, messageBuffer.Length);
            _pipeServerStream.Flush();
            _pipeServerStream.WaitForPipeDrain();
        }

        private async Task OnSendMessageCanceledAsync(Guid messageId)
        {
            ThrowIfNotConnected();
            using (await _sempahore.WaitAsync(CancellationToken.None))
            {
                await TaskScheduler.Default;

                // Send the cancellation message.
                var message = new AppServiceCancelMessage
                {
                    MessageId = messageId
                };
                SendMessage(message);
            }
        }

        private void Disconnect()
        {
            foreach (TaskCompletionSource<AppServiceMessageBase>? messageCompletionSource in _inProgressMessages.Values)
            {
                messageCompletionSource.TrySetCanceled();
            }
            _pipeServerStream?.Dispose();
            _pipeServerStream = null;
            _inProgressMessages.Clear();
            _progressReporters.Clear();
        }

        private async Task<bool> ConnectAsync()
        {
            try
            {
                if (_pipeServerStream is not null && _pipeServerStream.IsConnected)
                {
                    return true;
                }

                await TaskScheduler.Default;

                Disconnect();

                if (!ApiInformation.IsApiContractPresent(FullTrustAppContractName, 1, 0))
                {
                    throw new Exception("API Contract for full trust app isn't found.");
                }

                // Launch DevToys.OutOfProcService.exe
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();

                _pipeServerStream
                    = new NamedPipeServerStream(
                        string.Format(PipeName, Shared.Constants.AppServiceName),
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous,
                        inBufferSize: Shared.Constants.AppServiceBufferSize,
                        outBufferSize: Shared.Constants.AppServiceBufferSize);

                using var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(Shared.Constants.AppServiceTimeout);

                // Connect to the process
                await _pipeServerStream.WaitForConnectionAsync(cancellationTokenSource.Token);

                if (_pipeServerStream.IsConnected)
                {
                    // Let's start reading messages.
                    QueueReadMessage();
                }

                return _pipeServerStream.IsConnected;
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(AppService), ex, "Unable to connect to the app service.");
            }

            return false;
        }

        private void ThrowIfNotConnected()
        {
            if (_pipeServerStream is null || !_pipeServerStream.IsConnected)
            {
                throw new Exception("The app isn't connected to the app service.");
            }
        }

        private void QueueReadMessage(StringBuilder? partialMessage = null)
        {
            ThrowIfNotConnected();

            var lowLevelMessage = new LowLevelAppServiceMessage(_pipeServerStream!.InBufferSize, partialMessage);
            _pipeServerStream.BeginRead(lowLevelMessage.Buffer, 0, lowLevelMessage.Buffer.Length, ReadMessageCallback, lowLevelMessage);
        }

        private void ReadMessageCallback(IAsyncResult result)
        {
            if (_pipeServerStream is null || !_pipeServerStream.IsConnected)
            {
                // We're disconnected.
                return;
            }

            int readLength = _pipeServerStream!.EndRead(result);
            if (readLength == 0)
            {
                // There's nothing to read.
                return;
            }

            try
            {
                var lowLevelMessage = (LowLevelAppServiceMessage)result.AsyncState!;
                Assumes.NotNull(lowLevelMessage, nameof(lowLevelMessage));

                // Read the message
                lowLevelMessage.Message.Append(Encoding.UTF8.GetString(lowLevelMessage.Buffer, 0, readLength));

                if (!_pipeServerStream.IsMessageComplete)
                {
                    // We're not done reading the message. Let's keep reading it.
                    QueueReadMessage(lowLevelMessage.Message);
                    return;
                }

                // We're done reading the message.

                // Let's treat the current received message.
                string jsonMessage = lowLevelMessage.Message.ToString().TrimEnd('\0');

                AppServiceMessageBase messageBase
                    = JsonConvert.DeserializeObject<AppServiceMessageBase>(
                        jsonMessage,
                        Shared.Constants.AppServiceJsonSerializerSettings)!;

                Assumes.NotNull(messageBase, nameof(messageBase));

                if (messageBase.MessageId.HasValue)
                {
                    if (messageBase is AppServiceProgressMessage progressMessage)
                    {
                        // The message is here to indicate report a progression of a message previously sent through SendMessageAsync.
                        if (_progressReporters.TryGetValue(messageBase.MessageId.Value, out IProgress<AppServiceProgressMessage> progress))
                        {
                            progress.Report(progressMessage);
                        }
                    }
                    else
                    {
                        // This message is the reponse to a message we previously sent through SendMessageAsync.
                        // Let's unblock the TaskCompletionSource of the SendMessageAsync that sent the initial message.
                        if (_inProgressMessages.TryRemove(messageBase.MessageId.Value, out TaskCompletionSource<AppServiceMessageBase> messageCompletionSource))
                        {
                            messageCompletionSource.TrySetResult(messageBase);
                        }
                    }
                }
                else
                {
                    // TODO (if needed).
                    throw new InvalidOperationException();
                }

                // Let's queue reading the next message.
                QueueReadMessage();
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(AppService), ex, "Unable to read a message from the app service.");
            }
        }

        private class DummyProgress : IProgress<AppServiceProgressMessage>
        {
            internal static readonly DummyProgress DefaultInstance = new();

            public void Report(AppServiceProgressMessage value)
            {
            }
        }
    }
}
