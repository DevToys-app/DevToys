using System;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Shared.Core.OOP;

namespace DevToys.Api.Core.OOP
{
    /// <summary>
    /// Provides a service to interact with the Win32 process (DevToys.OutOfProcService.exe).
    /// </summary>
    public interface IAppService
    {
        Task SendMessageAsync(AppServiceMessageBase message);

        Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message) where T : AppServiceMessageBase;

        Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message, CancellationToken cancellationToken) where T : AppServiceMessageBase;

        Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message, IProgress<AppServiceProgressMessage> progress) where T : AppServiceMessageBase;

        Task<T> SendMessageAndGetResponseAsync<T>(AppServiceMessageBase message, IProgress<AppServiceProgressMessage> progress, CancellationToken cancellationToken) where T : AppServiceMessageBase;
    }
}
