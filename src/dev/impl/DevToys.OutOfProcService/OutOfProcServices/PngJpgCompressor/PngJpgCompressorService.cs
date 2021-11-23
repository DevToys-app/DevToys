using System.Composition;
using System.Threading.Tasks;
using DevToys.OutOfProcService.API.Core.OOP;
using DevToys.Shared.AppServiceMessages.PngJpgCompressor;

namespace DevToys.OutOfProcService.OutOfProcServices.PngJpgCompressor
{
    [Export(typeof(IOutOfProcService))]
    [InputType(typeof(PngJpgCompressorWorkMessage))]
    internal class PngJpgCompressorService : OutOfProcServiceBase<PngJpgCompressorWorkMessage, PngJpgCompressorWorkResultMessage>
    {
        protected override async Task<PngJpgCompressorWorkResultMessage> ProcessMessageAsync(PngJpgCompressorWorkMessage inputMessage)
        {
            await Task.Delay(500);
            await ReportProgressAsync(10);
            await Task.Delay(500);
            await ReportProgressAsync(20);
            await Task.Delay(500);
            await ReportProgressAsync(30);
            await Task.Delay(500);
            await ReportProgressAsync(40);
            await Task.Delay(500);
            await ReportProgressAsync(50);
            await Task.Delay(500);
            await ReportProgressAsync(60);
            await Task.Delay(500);
            await ReportProgressAsync(70);
            await Task.Delay(500);
            await ReportProgressAsync(80);
            await Task.Delay(500);
            await ReportProgressAsync(90);
            await Task.Delay(500);
            await ReportProgressAsync(100);
            return new PngJpgCompressorWorkResultMessage();
        }
    }
}
