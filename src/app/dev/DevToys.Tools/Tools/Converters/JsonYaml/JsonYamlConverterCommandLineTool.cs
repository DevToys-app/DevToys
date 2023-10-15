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
        IsRequired = true,
        DescriptionResourceName = nameof(JsonYamlConverter.InputOptionDescription))]
    internal AnyType<FileInfo, string> Input { get; set; }

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
        if (Input.TryGetSecond(out string? input) && !string.IsNullOrWhiteSpace(input))
        {
            return await InvokeCliAsync(
                input,
                logger,
                cancellationToken
            );
        }
        else if (Input.TryGetFirst(out FileInfo? file) && file is not null)
        {
            return await InvokeFileAsync(
                file,
                logger,
                cancellationToken
            );
        }

        Console.Error.WriteLine(JsonYamlConverter.InvalidInputOrFileCommand);
        return -1;
    }

    private async ValueTask<int> InvokeCliAsync(string input, ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(Input);

        ToolResult<string> conversionResult = await JsonYamlHelper.ConvertAsync(
            input,
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

    private async ValueTask<int> InvokeFileAsync(FileInfo file, ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(OutputFile);

        if (!file.Exists)
        {
            Console.Error.WriteLine(JsonYamlConverter.InputFileNotFound);
            return -1;
        }

        string content = await File.ReadAllTextAsync(file.FullName, cancellationToken);

        ToolResult<string> conversionResult
            = await JsonYamlHelper.ConvertAsync(
                content!,
                ConversionMode,
                IndentationMode,
                logger,
                cancellationToken);

        if (!conversionResult.HasSucceeded)
        {
            Console.Error.WriteLine(conversionResult.Data);
            return -1;
        }

        await File.WriteAllTextAsync(OutputFile.FullName, conversionResult.Data, cancellationToken);
        return 0;
    }
}
