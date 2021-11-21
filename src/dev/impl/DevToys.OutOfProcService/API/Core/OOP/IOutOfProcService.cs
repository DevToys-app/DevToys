#nullable enable

using System;
using System.Threading.Tasks;
using DevToys.Shared.Core.OOP;

namespace DevToys.OutOfProcService.API.Core.OOP
{
    internal interface IOutOfProcService
    {
        event EventHandler<AppServiceProgressMessageEventArgs>? ReportProgress;

        Task<AppServiceMessageBase> ProcessMessageAsync(AppServiceMessageBase inputMessage);
    }
}
