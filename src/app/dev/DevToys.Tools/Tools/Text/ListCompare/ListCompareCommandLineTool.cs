using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using OneOf;

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
    private OneOf<FileInfo, string>? FileA { get; set; }

    [CommandLineOption(
        Name = "fileb",
        Alias = "b",
        IsRequired = true,
        DescriptionResourceName = nameof(ListCompare.PathListB))]
    private OneOf<FileInfo, string>? FileB { get; set; }

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
        ResultInfo<string> fileAContent;
        ResultInfo<string> fileBContent;
        if (!FileA.HasValue)
        {
            Console.Error.WriteLine(string.Concat("listA :", ListCompare.InvalidInputOrFileCommand));
            return -1;
        }

        if (!FileB.HasValue)
        {
            Console.Error.WriteLine(string.Concat("listB :", ListCompare.InvalidInputOrFileCommand));
            return -1;
        }

        fileAContent = await FileA.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        fileBContent = await FileB.Value.ReadAllTextAsync(_fileStorage, cancellationToken);

        if (!fileAContent.HasSucceeded)
        {
            Console.Error.WriteLine(String.Concat("File A: ", ListCompare.InputFileNotFound));
            return -1;
        }

        if (!fileBContent.HasSucceeded)
        {
            Console.Error.WriteLine(String.Concat("File B: ", ListCompare.InputFileNotFound));
            return -1;
        }

        Guard.IsNotNull(fileAContent.Data);
        Guard.IsNotNull(fileBContent.Data);

        ResultInfo<string> output = ComparisonMode switch
        {
            ListComparisonMode.AInterB => ListCompareHelper.Compare(fileAContent.Data, fileBContent.Data, IsCaseSensitive, ListComparisonMode.AInterB, logger),
            ListComparisonMode.AOnly => ListCompareHelper.Compare(fileAContent.Data, fileBContent.Data, IsCaseSensitive, ListComparisonMode.AOnly, logger),
            ListComparisonMode.BOnly => ListCompareHelper.Compare(fileAContent.Data, fileBContent.Data, IsCaseSensitive, ListComparisonMode.BOnly, logger),
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
