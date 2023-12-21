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
        ResultInfo<string> result;
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(Base64TextEncoderDecoder.InvalidInputOrFileCommand);
            return -1;
        }

        result = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(Base64TextEncoderDecoder.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(result.Data);

        string output;
        switch (EncodingConversionMode)
        {
            case EncodingConversion.Encode:
                output = Base64Helper.FromTextToBase64(result.Data, EncodingMode, logger, cancellationToken);
                break;

            case EncodingConversion.Decode:
                if (!Base64Helper.IsBase64DataStrict(result.Data))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Console.Error.WriteLine(Base64TextEncoderDecoder.InvalidBase64);
                    return -1;
                }

                output = Base64Helper.FromBase64ToText(result.Data, EncodingMode, logger, cancellationToken);
                break;

            default:
                throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();

        await FileHelper.WriteOutputAsync(output, OutputFile, cancellationToken);
        return 0;
    }
}
