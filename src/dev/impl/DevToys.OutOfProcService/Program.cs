using System.Threading.Tasks;
using DevToys.OutOfProcService.Core.OOP;
using DevToys.Shared.Api.Core;
using DevToys.Shared.Core;

namespace DevToys.OutOfProcService
{
    internal sealed class Program
    {
        private static MefComposer? _mefComposer;
        private static IMefProvider? _mefProvider;

        public static async Task Main(string[] args)
        {
            _mefComposer
                = new MefComposer(
                    typeof(Program).Assembly,
                    typeof(Shared.Constants).Assembly);

            _mefProvider = _mefComposer.ExportProvider.GetExport<IMefProvider>();

            await _mefProvider.Import<AppService>().WaitAppServiceConnectionCloseAsync();
        }
    }
}
