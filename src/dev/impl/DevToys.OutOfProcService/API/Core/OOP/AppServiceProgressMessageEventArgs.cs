using System;
using System.Threading.Tasks;
using DevToys.Shared.Core.OOP;

namespace DevToys.OutOfProcService.API.Core.OOP
{
    internal sealed class AppServiceProgressMessageEventArgs : EventArgs
    {
        internal AppServiceProgressMessage Message { get; }

        internal TaskCompletionSource MessageCompletedTask { get; } = new();

        internal AppServiceProgressMessageEventArgs(Guid messageId, int progressPercentage, string? message)
        {
            if (progressPercentage is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(progressPercentage));
            }

            Message = new AppServiceProgressMessage
            {
                MessageId = messageId,
                ProgressPercentage = progressPercentage,
                Message = message
            };
        }
    }
}
