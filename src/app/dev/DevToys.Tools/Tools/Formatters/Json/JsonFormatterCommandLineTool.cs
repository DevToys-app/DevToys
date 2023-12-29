using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Formatters.Json;

[Export(typeof(ICommandLineTool))]
[Name("JsonFormatter")]
[CommandName(
    Name = "JsonFormatter",
    Alias = "Jsonf",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Formatters.Json.JsonFormatter",
    DescriptionResourceName = nameof(JsonFormatter.Description))]
internal sealed class JsonFormatterCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonFormatter.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(JsonFormatter.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "indentation",
        DescriptionResourceName = nameof(JsonFormatter.IndentationOptionDescription))]
    internal Indentation IndentationMode { get; set; } = Indentation.TwoSpaces;

    [CommandLineOption(
        Name = "sortProperties",
        DescriptionResourceName = nameof(JsonFormatter.SortPropertiesOptionDescription))]
    internal bool SortProperties { get; set; } = false;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(JsonFormatter.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> input = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!input.HasSucceeded)
        {
            Console.Error.WriteLine(JsonFormatter.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(input.Data);
        ResultInfo<string> formatResult = await JsonHelper.FormatAsync(
            input.Data,
            IndentationMode,
            SortProperties,
            logger,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (!formatResult.HasSucceeded)
        {
            Console.Error.WriteLine(formatResult.Data);
            return -1;
        }

        await FileHelper.WriteOutputAsync(formatResult.Data, OutputFile, cancellationToken);
        return 0;
    }
}
