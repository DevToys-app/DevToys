#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevToys.OutOfProcService.API.Core.OOP;
using DevToys.Shared.Api.Core;
using DevToys.Shared.Core;
using DevToys.Shared.Core.OOP;
using DevToys.Shared.Core.Threading;
using Newtonsoft.Json;
using Windows.ApplicationModel;

namespace DevToys.OutOfProcService.Core.OOP
{
    [Export(typeof(AppService))]
    [Shared]
    internal sealed class AppService
    {
        private const string PipeName = @"Sessions\{0}\AppContainerNamedObjects\{1}\{2}";

        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _inProgressMessages = new();
        private readonly DisposableSempahore _sempahore = new();
        private readonly TaskCompletionSource _appServiceConnectionClosedTask = new();
        private readonly IMefProvider _mefProvider;

        private NamedPipeClientStream? _pipeClientStream;

        [ImportingConstructor]
        public AppService(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
            ConnectAsync().Forget();
        }

        internal void IndicateAppServiceConnectionLost()
        {
            Disconnect();
            _appServiceConnectionClosedTask.TrySetResult();
        }

        internal Task WaitAppServiceConnectionCloseAsync()
        {
            return _appServiceConnectionClosedTask.Task;
        }

        private void ThrowIfNotConnected()
        {
            if (_pipeClientStream is null || !_pipeClientStream.IsConnected)
            {
                throw new Exception("The app isn't connected to the app service.");
            }
        }

        private void Disconnect()
        {
            _pipeClientStream?.Dispose();
            _pipeClientStream = null;
        }

        private async Task ConnectAsync()
        {
            try
            {
                if (_pipeClientStream is not null && _pipeClientStream.IsConnected)
                {
                    return;
                }

                await TaskScheduler.Default;

                Disconnect();

                string packageSid = GetPackageSid();

                _pipeClientStream
                    = new NamedPipeClientStream(
                        serverName: ".",
                        pipeName: string.Format(PipeName, Process.GetCurrentProcess().SessionId, packageSid, Shared.Constants.AppServiceName),
                        PipeDirection.InOut,
                        PipeOptions.Asynchronous);

                using var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(Shared.Constants.AppServiceTimeout);

                // Connect to the UWP process
                await _pipeClientStream.ConnectAsync(cancellationTokenSource.Token);
                _pipeClientStream.ReadMode = PipeTransmissionMode.Message;

                if (_pipeClientStream.IsConnected)
                {
                    // Let's start reading messages.
                    QueueReadMessage();
                }

                if (!_pipeClientStream.IsConnected)
                {
                    IndicateAppServiceConnectionLost();
                }
            }
            catch (Exception)
            {
                // TODO: Log this.
            }
        }

        private void QueueReadMessage(StringBuilder? partialMessage = null)
        {
            ThrowIfNotConnected();

            var lowLevelMessage = new LowLevelAppServiceMessage(_pipeClientStream!.InBufferSize, partialMessage);
            _pipeClientStream.BeginRead(lowLevelMessage.Buffer, 0, lowLevelMessage.Buffer.Length, ReadMessageCallback, lowLevelMessage);
        }

        private void ReadMessageCallback(IAsyncResult result)
        {
            ThrowIfNotConnected();

            int readLength = _pipeClientStream!.EndRead(result);
            if (readLength == 0)
            {
                return;
            }

            try
            {
                var lowLevelMessage = (LowLevelAppServiceMessage)result.AsyncState!;
                Assumes.NotNull(lowLevelMessage, nameof(lowLevelMessage));

                // Read the message
                lowLevelMessage.Message.Append(Encoding.UTF8.GetString(lowLevelMessage.Buffer, 0, readLength));

                if (!_pipeClientStream.IsMessageComplete)
                {
                    // We're not done reading the message. Let's keep reading it.
                    QueueReadMessage(lowLevelMessage.Message);
                    return;
                }

                // We're done reading the message.

                // Let's treat the current received message.
                string jsonMessage = lowLevelMessage.Message.ToString().TrimEnd('\0');

                AppServiceMessageBase inputMessage
                    = JsonConvert.DeserializeObject<AppServiceMessageBase>(
                        jsonMessage,
                        Shared.Constants.AppServiceJsonSerializerSettings)!;

                Assumes.NotNull(inputMessage, nameof(inputMessage));

                if (inputMessage.MessageId.HasValue)
                {
                    CancellationToken cancellationToken = CancellationToken.None;
                    var cancellationTokenSource = new CancellationTokenSource();
                    if (!_inProgressMessages.TryAdd(inputMessage.MessageId.Value, cancellationTokenSource))
                    {
                        cancellationTokenSource.Dispose();
                    }
                    else
                    {
                        cancellationToken = cancellationTokenSource.Token;
                    }

                    ProcessMessageAsync(inputMessage, cancellationToken).Forget();
                }
                else
                {
                    // TODO (if needed).
                    throw new InvalidOperationException();
                }

                // Let's queue reading the next message.
                QueueReadMessage();
            }
            catch (Exception)
            {
                // TODO: Log this
            }
        }

        private async Task ProcessMessageAsync(AppServiceMessageBase inputMessage, CancellationToken cancellationToken)
        {
            ThrowIfNotConnected();
            IOutOfProcService? service = null;

            try
            {
                if (inputMessage is AppServiceCancelMessage cancelMessage
                    && _inProgressMessages.TryGetValue(cancelMessage.MessageId!.Value, out CancellationTokenSource? cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                    return;
                }

                // Get the service that corresponds to the message.
                IEnumerable<Lazy<IOutOfProcService, OutOfProcServiceMetadata>> services
                    = _mefProvider.ImportMany<Lazy<IOutOfProcService, OutOfProcServiceMetadata>>();
                service = services.Single(service => service.Metadata.InputType == inputMessage.GetType()).Value;
                service.ReportProgress += Service_ReportProgress;

                // Invoke the service.
                AppServiceMessageBase? outputMessage = await service.ProcessMessageAsync(inputMessage, cancellationToken);
                if (outputMessage is not null)
                {
                    outputMessage.MessageId = inputMessage.MessageId;

                    // Send the service result as a response to the UWP app.
                    await SendMessageAsync(outputMessage);
                }
            }
            catch (OperationCanceledException)
            {
                // Let the UWP app that the message got canceled.
                await SendMessageAsync(new AppServiceCancelMessage
                {
                    MessageId = inputMessage.MessageId
                });
            }
            catch (Exception)
            {
                // TODO: Log this.
            }
            finally
            {
                if (_inProgressMessages.TryRemove(inputMessage.MessageId!.Value, out CancellationTokenSource? cancellationTokenSource))
                {
                    cancellationTokenSource.Dispose();
                }

                if (service is not null)
                {
                    service.ReportProgress -= Service_ReportProgress;
                    if (service is IDisposable disposableService)
                    {
                        disposableService.Dispose();
                    }
                }
            }
        }

        private void Service_ReportProgress(object? sender, AppServiceProgressMessageEventArgs e)
        {
            Assumes.NotNull(e.Message.MessageId, nameof(e.Message.MessageId));
            SendMessageAsync(e.Message)
                .ContinueWith(t =>
                {
                    e.MessageCompletedTask.TrySetResult();
                });
        }

        private async Task SendMessageAsync(AppServiceMessageBase inputMessage)
        {
            using (await _sempahore.WaitAsync(CancellationToken.None))
            {
                ThrowIfNotConnected();

                string jsonMessage = JsonConvert.SerializeObject(inputMessage, Shared.Constants.AppServiceJsonSerializerSettings);
                byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonMessage);

                await _pipeClientStream!.WriteAsync(messageBuffer, 0, messageBuffer.Length);
                await _pipeClientStream.FlushAsync();
                _pipeClientStream.WaitForPipeDrain();
            }
        }

        private static string GetPackageSid()
        {
            IntPtr sid = IntPtr.Zero;
            try
            {
                if (NativeMethods.DeriveAppContainerSidFromAppContainerName(Package.Current.Id.Name, out sid) == 0)
                {
                    string sidString = new SecurityIdentifier(sid).Value;
                    sidString = string.Join("-", sidString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).Take(11));
                    return sidString;
                }
            }
            finally
            {
                if (sid != IntPtr.Zero)
                {
                    NativeMethods.FreeSid(sid);
                }
            }

            throw new Exception("Unable to retrieve the package security identifier.");
        }
    }
}
