using System.Composition;
using System.Threading.Tasks;
using DevToys.OutOfProcService.API.Core.OOP;
using DevToys.OutOfProcService.Core.OOP;

namespace DevToys.OutOfProcService.OutOfProcServices
{
    [Export(typeof(IOutOfProcService))]
    [InputType(typeof(ShutdownMessage))]
    internal sealed class ShutdownService : OutOfProcServiceBase<ShutdownMessage, ShutdownMessage>
    {
        private readonly AppService _appService;

        [ImportingConstructor]
        public ShutdownService(AppService appService)
        {
            _appService = appService;
        }

        protected override Task<ShutdownMessage> ProcessMessageAsync(ShutdownMessage inputMessage)
        {
            _appService.IndicateAppServiceConnectionLost();
            return Task.FromResult(inputMessage);
        }
    }
}
