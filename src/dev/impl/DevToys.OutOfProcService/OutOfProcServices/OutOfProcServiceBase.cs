using System;
using System.Threading;
using System.Threading.Tasks;
using DevToys.OutOfProcService.API.Core.OOP;
using DevToys.Shared.Core;
using DevToys.Shared.Core.OOP;
using DevToys.Shared.Core.Threading;

namespace DevToys.OutOfProcService.OutOfProcServices
{
    internal abstract class OutOfProcServiceBase<TInput, TOutput> : IOutOfProcService
        where TInput : AppServiceMessageBase
        where TOutput : AppServiceMessageBase
    {
        private readonly DisposableSempahore _sempahore = new();
        private Guid? _messageId;
        private bool _messageIsProcessing;

        public event EventHandler<AppServiceProgressMessageEventArgs>? ReportProgress;

        public async Task<AppServiceMessageBase> ProcessMessageAsync(AppServiceMessageBase inputMessage)
        {
            _messageIsProcessing = true;
            _messageId = inputMessage.MessageId;
            TOutput result = await ProcessMessageAsync((TInput)inputMessage);
            _messageIsProcessing = false;
            return result;
        }

        protected abstract Task<TOutput> ProcessMessageAsync(TInput inputMessage);

        protected async Task ReportProgressAsync(int progressPercentage, string? message = null)
        {
            await TaskScheduler.Default;

            using (await _sempahore.WaitAsync(CancellationToken.None))
            {
                Assumes.IsTrue(_messageIsProcessing, nameof(_messageIsProcessing));
                var eventArgs = new AppServiceProgressMessageEventArgs(_messageId!.Value, progressPercentage, message);
                ReportProgress?.Invoke(this, eventArgs);
                await eventArgs.MessageCompletedTask.Task;
            }
        }
    }
}
