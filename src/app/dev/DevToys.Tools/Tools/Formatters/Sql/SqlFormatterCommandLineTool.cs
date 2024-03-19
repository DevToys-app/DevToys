using DevToys.Tools.Helpers.SqlFormatter;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Formatters.Sql;

[Export(typeof(ICommandLineTool))]
[Name("SqlFormatter")]
[CommandName(
    Name = "sqlFormatter",
    Alias = "sqlf",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Formatters.Sql.SqlFormatter",
    DescriptionResourceName = nameof(SqlFormatter.Description))]
internal sealed class SqlFormatterCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(SqlFormatter.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(SqlFormatter.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    [CommandLineOption(
        Name = "indentation",
        DescriptionResourceName = nameof(SqlFormatter.IndentationOptionDescription))]
    internal Indentation IndentationMode { get; set; } = Indentation.TwoSpaces;

    [CommandLineOption(
        Name = "language",
        DescriptionResourceName = nameof(SqlFormatter.SqlLanguageDescription))]
    internal SqlLanguage SqlLanguage { get; set; } = SqlLanguage.Sql;

    [CommandLineOption(
        Name = "leadingComma",
        DescriptionResourceName = nameof(SqlFormatter.LeadingCommaDescription))]
    internal bool UseLeadingComma { get; set; } = false;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(SqlFormatter.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> input = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!input.HasSucceeded)
        {
            Console.Error.WriteLine(SqlFormatter.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(input.Data);
        string formatResult = SqlFormatterHelper.Format(
                input.Data,
                SqlLanguage,
                new SqlFormatterOptions(
                            IndentationMode,
                            Uppercase: true,
                            LinesBetweenQueries: 2,
                            UseLeadingComma: UseLeadingComma));

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(formatResult))
        {
            return -1;
        }

        await FileHelper.WriteOutputAsync(formatResult, OutputFile, cancellationToken);
        return 0;
    }
}
