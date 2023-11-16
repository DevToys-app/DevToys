using System.IO.Compression;
using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.GZip;

[Export(typeof(ICommandLineTool))]
[Name("GZipEncoderDecoder")]
[CommandName(
    Name = "gzip",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.GZip.GZipEncoderDecoder",
    DescriptionResourceName = nameof(GZipEncoderDecoder.Description))]
internal sealed class GZipEncoderDecoderCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(GZipEncoderDecoder.InputOptionDescription))]
    private string? Input { get; set; } // TODO: Support file as input too.

    [CommandLineOption(
        Name = "mode",
        Alias = "m",
        DescriptionResourceName = nameof(GZipEncoderDecoder.CompressionModeOptionDescription))]
    private CompressionMode CompressionMode { get; set; } = CompressionMode.Compress;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        (string data, double differencePercentage) conversionResult
            = await GZipHelper.CompressOrDecompressAsync(
                Input,
                CompressionMode,
                logger,
                cancellationToken);

        Console.WriteLine(conversionResult.data);

        return 0;
    }
}
