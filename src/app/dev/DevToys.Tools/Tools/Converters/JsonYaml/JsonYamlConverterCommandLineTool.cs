using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Converters.JsonYaml;

[Export(typeof(ICommandLineTool))]
[Name("JsonYamlConverter")]
[CommandName(
    Name = "jsonToYaml",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.JsonYaml.JsonYamlConverter",
    DescriptionResourceName = nameof(JsonYamlConverter.Description))]
internal sealed class JsonYamlConverterCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonYamlConverter.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(JsonYamlConverter.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonYamlConverter.ConversionOptionDescription))]
    internal JsonToYamlConversion ConversionMode { get; set; } = JsonToYamlConversion.JsonToYaml;

    [CommandLineOption(
        Name = "indentation",
        DescriptionResourceName = nameof(JsonYamlConverter.IndentationOptionDescription))]
    internal Indentation IndentationMode { get; set; } = Indentation.TwoSpaces;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(JsonYamlConverter.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> input = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!input.HasSucceeded)
        {
            Console.Error.WriteLine(JsonYamlConverter.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(input.Data);
        ResultInfo<string> conversionResult = await JsonYamlHelper.ConvertAsync(
            input.Data,
            ConversionMode,
            IndentationMode,
            logger,
            cancellationToken
        );

        cancellationToken.ThrowIfCancellationRequested();

        if (!conversionResult.HasSucceeded)
        {
            Console.Error.WriteLine(conversionResult.Data);
            return -1;
        }

        await FileHelper.WriteOutputAsync(conversionResult.Data, OutputFile, cancellationToken);
        return 0;
    }
}
