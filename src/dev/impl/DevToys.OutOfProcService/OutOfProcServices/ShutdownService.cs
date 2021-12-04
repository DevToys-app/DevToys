using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using DevToys.OutOfProcService.API.Core.OOP;
using DevToys.OutOfProcService.Core.OOP;
using DevToys.Shared.Core.OOP;

namespace DevToys.OutOfProcService.OutOfProcServices
{
    [Export(typeof(IOutOfProcService))]
    [InputType(typeof(ShutdownMessage))]
    internal sealed class ShutdownService : OutOfProcServiceBase<ShutdownMessage, AppServiceMessageBase?>
    {
        private readonly AppService _appService;

        [ImportingConstructor]
        public ShutdownService(AppService appService)
        {
            _appService = appService;
        }

        protected override Task<AppServiceMessageBase?> ProcessMessageAsync(ShutdownMessage inputMessage, CancellationToken cancellationToken)
        {
            _appService.IndicateAppServiceConnectionLost();
            return Task.FromResult<AppServiceMessageBase?>(null);
        }
    }
}
