using DevToys.Api.Core;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Formatters.Xml;

[Export(typeof(ICommandLineTool))]
[Name("XmlFormatter")]
[CommandName(
    Name = "xmlFormatter",
    Alias = "xmlf",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Formatters.Xml.XmlFormatter",
    DescriptionResourceName = nameof(XmlFormatter.Description))]
internal sealed class XmlFormatterCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(XmlFormatter.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(XmlFormatter.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "indentation",
        DescriptionResourceName = nameof(XmlFormatter.IndentationOptionDescription))]
    internal Indentation IndentationMode { get; set; } = Indentation.TwoSpaces;

    [CommandLineOption(
        Name = "newLineOnAttributes",
        DescriptionResourceName = nameof(XmlFormatter.NewLineOnAttributesDescription))]
    internal bool NewLineOnAttributes { get; set; } = false;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(XmlFormatter.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> input = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!input.HasSucceeded)
        {
            Console.Error.WriteLine(XmlFormatter.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(input.Data);
        ResultInfo<string> formatResult = XmlHelper.Format(
            input.Data,
            IndentationMode,
            NewLineOnAttributes,
            logger);

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
