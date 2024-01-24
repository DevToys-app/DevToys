using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Text.EscapeUnescape;

[Export(typeof(ICommandLineTool))]
[Name("EscapeUnescape")]
[CommandName(
    Name = "escape",
    Alias = "esc",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.EscapeUnescape.EscapeUnescape",
    DescriptionResourceName = nameof(EscapeUnescape.Description))]
internal sealed class EscapeUnescapeCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(EscapeUnescape.InputOptionDescription))]
    private OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(EscapeUnescape.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        DescriptionResourceName = nameof(EscapeUnescape.ConversionOptionDescription))]
    private EncodingConversion EncodingConversionMode { get; set; } = EncodingConversion.Encode;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        ResultInfo<string> result;
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(EscapeUnescape.InvalidInputOrFileCommand);
            return -1;
        }

        result = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(EscapeUnescape.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(result.Data);
        ResultInfo<string> output = EncodingConversionMode switch
        {
            EncodingConversion.Encode => StringHelper.EscapeString(result.Data, logger, cancellationToken),
            EncodingConversion.Decode => StringHelper.UnescapeString(result.Data, logger, cancellationToken),
            _ => throw new NotSupportedException(),
        };

        cancellationToken.ThrowIfCancellationRequested();

        if (output.HasSucceeded)
        {
            await FileHelper.WriteOutputAsync(output.Data, OutputFile, cancellationToken);
            return 0;
        }
        else
        {
            Console.Error.WriteLine(output.Data);
            return -1;
        }
    }
}
