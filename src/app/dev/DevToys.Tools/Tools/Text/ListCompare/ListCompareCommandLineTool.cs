using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Text.ListCompare;

[Export(typeof(ICommandLineTool))]
[Name("ListCompare")]
[CommandName(
    Name = "listcompare",
    Alias = "lc",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.ListCompare.ListCompare",
    DescriptionResourceName = nameof(ListCompare.Description))]
internal sealed class ListCompareCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "filea",
        Alias = "a",
        IsRequired = true,
        DescriptionResourceName = nameof(ListCompare.PathListA))]
    private FileInfo? FileA { get; set; }

    [CommandLineOption(
        Name = "fileb",
        Alias = "b",
        IsRequired = true,
        DescriptionResourceName = nameof(ListCompare.PathListB))]
    private FileInfo? FileB { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(ListCompare.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "casesensitive",
        Alias = "cs",
        DescriptionResourceName = nameof(ListCompare.TextCaseSensitiveComparison))]
    private bool IsCaseSensitive { get; set; }

    [CommandLineOption(
        Name = "comparisonmode",
        Alias = "cm",
        DescriptionResourceName = nameof(ListCompare.ComparisonOptionDescription))]
    private ListComparisonMode ComparisonMode { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (FileA == null || !FileA.Exists)
        {
            Console.Error.WriteLine(string.Concat("fileA :", ListCompare.InputFileNotFound));
            return -1;
        }

        if (FileB == null || !FileB.Exists)
        {
            Console.Error.WriteLine(string.Concat("fileB :", ListCompare.InputFileNotFound));
            return -1;
        }

        using Stream fileAStream = _fileStorage.OpenReadFile(FileA.FullName);
        using var readerFileA = new StreamReader(fileAStream);
        string fileAContent = await readerFileA.ReadToEndAsync(cancellationToken);

        using Stream fileBStream = _fileStorage.OpenReadFile(FileA.FullName);
        using var readerFileB = new StreamReader(fileBStream);
        string fileBContent = await readerFileB.ReadToEndAsync(cancellationToken);

        ResultInfo<string> output = ListCompareHelper.Compare(fileAContent, fileBContent, IsCaseSensitive, ComparisonMode, logger);
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
