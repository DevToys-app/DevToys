using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Converters.JsonYaml;

[Export(typeof(ICommandLineTool))]
[Name("JsonYamlConverter")]
[CommandName(
    Name = "jsonToYaml",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.JsonYaml.JsonYamlConverter",
    DescriptionResourceName = nameof(JsonYamlConverter.Description))]
internal sealed class JsonYamlConverterCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        DescriptionResourceName = nameof(JsonYamlConverter.InputOptionDescription))]
    internal string? Input { get; set; }

    [CommandLineOption(
        Name = "file",
        Alias = "f",
        DescriptionResourceName = nameof(JsonYamlConverter.FileOptionDescription))]
    internal FileInfo? InputFile { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "of",
        DescriptionResourceName = nameof(JsonYamlConverter.FileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonYamlConverter.ConversionOptionDescription))]
    internal Conversion ConversionMode { get; set; } = Conversion.JsonToYaml;

    [CommandLineOption(
        Name = "indentation",
        DescriptionResourceName = nameof(JsonYamlConverter.IndentationOptionDescription))]
    internal Indentation IndentationMode { get; set; } = Indentation.TwoSpaces;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(Input))
        {
            return await InvokeCliAsync(
                logger,
                cancellationToken
            );
        }
        else if (InputFile is not null)
        {
            return await InvokeFileAsync(
                logger,
                cancellationToken
            );
        }
        return -1;
    }

    private async ValueTask<int> InvokeCliAsync(ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(Input);

        ToolResult<string> conversionResult = await JsonYamlHelper.ConvertAsync(
            Input,
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

        Console.WriteLine(conversionResult.Data);
        return 0;
    }

    private async ValueTask<int> InvokeFileAsync(ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(InputFile);
        Guard.IsNotNull(OutputFile);

        if (!InputFile.Exists)
        {
            Console.Error.WriteLine(JsonYamlConverter.InputFileNotFound);
            return -1;
        }

        string content = File.ReadAllText(InputFile.FullName);

        ToolResult<string> conversionResult = await JsonYamlHelper.ConvertAsync(
            content!,
            ConversionMode,
            IndentationMode,
            logger,
            cancellationToken
        );

        if (!conversionResult.HasSucceeded)
        {
            Console.Error.WriteLine(conversionResult.Data);
            return -1;
        }

        File.WriteAllText(OutputFile.FullName, conversionResult.Data);
        return 0;
    }
}
