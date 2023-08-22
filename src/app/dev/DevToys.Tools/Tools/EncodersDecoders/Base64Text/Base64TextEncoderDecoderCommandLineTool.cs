using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Base64Text;

[Export(typeof(ICommandLineTool))]
[Name("Base64TextEncoderDecoder")]
[CommandName(
    Name = "base64",
    Alias = "b64",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Base64Text.Base64TextEncoderDecoder",
    DescriptionResourceName = nameof(Base64TextEncoderDecoder.Description))]
internal sealed class Base64TextEncoderDecoderCommandLineTool : ICommandLineTool
{
    private enum Conversion
    {
        Encode,
        Decode
    }

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.InputOptionDescription))]
    private string? Input { get; set; } // NOTE: Could be a FileInfo instead of string too, if you we want to accept file as input instead.

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.ConversionOptionDescription))]
    private Conversion ConversionMode { get; set; } = Conversion.Encode;

    [CommandLineOption(
        Name = "encoding",
        Alias = "e",
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.EncodingOptionDescription))]
    private Base64Encoding EncodingMode { get; set; } = Base64Encoding.Utf8;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        string output;
        if (ConversionMode == Conversion.Encode)
        {
            output = Base64Helper.FromTextToBase64(Input, EncodingMode, logger, cancellationToken);
        }
        else
        {
            if (!Base64Helper.IsBase64DataStrict(Input))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.Error.WriteLine(Base64TextEncoderDecoder.InvalidBase64);
                return new ValueTask<int>(-1);
            }

            output = Base64Helper.FromBase64ToText(Input, EncodingMode, logger, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine(output);

        return new ValueTask<int>(0);
    }
}
