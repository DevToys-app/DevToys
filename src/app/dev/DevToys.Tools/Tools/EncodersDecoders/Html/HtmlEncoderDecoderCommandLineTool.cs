using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Html;

[Export(typeof(ICommandLineTool))]
[Name("HtmlEncoderDecoder")]
[CommandName(
    Name = "html",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Html.HtmlEncoderDecoder",
    DescriptionResourceName = nameof(HtmlEncoderDecoder.Description))]
internal sealed class HtmlEncoderDecoderCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(HtmlEncoderDecoder.InputOptionDescription))]
    private string? Input { get; set; }

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        DescriptionResourceName = nameof(HtmlEncoderDecoder.ConversionOptionDescription))]
    private EncodingConversion EncodingConversionMode { get; set; } = EncodingConversion.Encode;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        string output;
        switch (EncodingConversionMode)
        {
            case EncodingConversion.Encode:
                output = HtmlHelper.EncodeHtmlData(Input, logger, cancellationToken);
                break;

            case EncodingConversion.Decode:
                output = HtmlHelper.EncodeHtmlData(Input, logger, cancellationToken);
                break;

            default:
                throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine(output);

        return new ValueTask<int>(0);
    }
}
