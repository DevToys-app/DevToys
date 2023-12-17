using DevToys.Tools.Helpers;
using DevToys.Tools.Tools.Converters.JsonTable;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Converters.JsonYaml;

[Export(typeof(ICommandLineTool))]
[Name("JsonTableConverter")]
[CommandName(
    Name = "jsonToTable",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.JsonTable.JsonTableConverter",
    DescriptionResourceName = nameof(JsonTableConverter.Description))]
internal sealed class JsonTableConverterCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonTableConverter.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(JsonTableConverter.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    // Default to comma-separated-values because output-to-file is the most likely use-case.
    [CommandLineOption(
        Name = "format",
        DescriptionResourceName = nameof(JsonTableConverter.ClipboardFormatDescription))]
    internal JsonTableHelper.CopyFormat Format { get; set; } = JsonTableHelper.CopyFormat.CSV;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(JsonYamlConverter.InvalidInputOrFileCommand);
            return ValueTask.FromResult(-1);
        }

        return Input.Value.Match(
            file =>
                InvokeFileAsync(
                    file,
                    cancellationToken),
            text =>
                InvokeCliAsync(
                    text,
                    cancellationToken));
    }

    private async ValueTask<int> InvokeCliAsync(string input, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(Input);

        JsonTableHelper.ConvertResult conversionResult = JsonTableHelper.ConvertFromJson(input, Format, cancellationToken);
        string resultText = conversionResult.Text;

        cancellationToken.ThrowIfCancellationRequested();

        if (conversionResult.Error != JsonTableHelper.ConvertResultError.None)
        {
            Console.Error.WriteLine(conversionResult.Error);
            return -1;
        }

        if (OutputFile == null)
        {
            Console.WriteLine(resultText);
        }
        else
        {
            await File.WriteAllTextAsync(OutputFile.FullName, resultText, cancellationToken);
        }

        return 0;
    }

    private async ValueTask<int> InvokeFileAsync(FileInfo file, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(OutputFile);

        if (!file.Exists)
        {
            Console.Error.WriteLine(JsonYamlConverter.InputFileNotFound);
            return -1;
        }

        string content = await File.ReadAllTextAsync(file.FullName, cancellationToken);

        return await InvokeCliAsync(content, cancellationToken);
    }
}
