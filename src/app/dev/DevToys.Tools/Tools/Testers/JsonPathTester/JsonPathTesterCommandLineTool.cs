using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Testers.JsonPathTester;

[Export(typeof(ICommandLineTool))]
[Name("JSONPathTester")]
[CommandName(
    Name = "jsonpathtester",
    Alias = "jpt",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Testers.JsonPathTester.JsonPathTester",
    DescriptionResourceName = nameof(JsonPathTester.Description))]
internal sealed class JsonPathTesterCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "json",
        Alias = "j",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonPathTester.InputJsonOptionDescription))]
    internal OneOf<FileInfo, string>? InputJson { get; set; }

    [CommandLineOption(
        Name = "path",
        Alias = "p",
        IsRequired = true,
        DescriptionResourceName = nameof(JsonPathTester.InputJsonPathOptionDescription))]
    internal OneOf<FileInfo, string>? InputJsonPath { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(JsonPathTester.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!InputJson.HasValue || !InputJsonPath.HasValue)
        {
            Console.Error.WriteLine(JsonPathTester.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> inputJson = await InputJson.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!inputJson.HasSucceeded)
        {
            Console.Error.WriteLine(JsonPathTester.InputFileNotFound);
            return -1;
        }

        ResultInfo<string> inputJsonPath = await InputJsonPath.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!inputJsonPath.HasSucceeded)
        {
            Console.Error.WriteLine(JsonPathTester.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(inputJson.Data);
        Guard.IsNotNull(inputJsonPath.Data);
        ResultInfo<string> formatResult = await JsonHelper.TestJsonPathAsync(
            inputJson.Data,
            inputJsonPath.Data,
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
