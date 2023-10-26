using DevToys.Api.Core;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

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
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.InputOptionDescription))]
    private OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.ConversionOptionDescription))]
    private EncodingConversion EncodingConversionMode { get; set; } = EncodingConversion.Encode;

    [CommandLineOption(
        Name = "encoding",
        Alias = "e",
        DescriptionResourceName = nameof(Base64TextEncoderDecoder.EncodingOptionDescription))]
    private Base64Encoding EncodingMode { get; set; } = Base64Encoding.Utf8;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        string? input = null;

        if (Input.HasValue)
        {
            input = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        }

        Guard.IsNotNull(input);

        string output;
        switch (EncodingConversionMode)
        {
            case EncodingConversion.Encode:
                output = Base64Helper.FromTextToBase64(input, EncodingMode, logger, cancellationToken);
                break;

            case EncodingConversion.Decode:
                if (!Base64Helper.IsBase64DataStrict(input))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Console.Error.WriteLine(Base64TextEncoderDecoder.InvalidBase64);
                    return -1;
                }

                output = Base64Helper.FromBase64ToText(input, EncodingMode, logger, cancellationToken);
                break;

            default:
                throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (OutputFile is null)
        {
            Console.WriteLine(output);
        }
        else
        {
            await File.WriteAllTextAsync(OutputFile.FullName, output, cancellationToken);
        }

        return 0;
    }
}
