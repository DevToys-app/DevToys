using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Text.AnalyzerAndUtilities;

[Export(typeof(ICommandLineTool))]
[Name("AnalyzerAndUtilities")]
[CommandName(
    Name = "textutilities",
    Alias = "txt",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.AnalyzerAndUtilities.AnalyzerAndUtilities",
    DescriptionResourceName = nameof(AnalyzerAndUtilities.Description))]
internal sealed class AnalyzerAndUtilitiesCommandLineTool : ICommandLineTool
{
    private enum OperationType
    {
        ConvertLineBreakToLF,
        ConvertLineBreakToCRLF,
        LowerCase,
        UpperCase,
        SentenceCase,
        TitleCase,
        CamelCase,
        PascalCase,
        SnakeCase,
        ConstantCase,
        KebabCase,
        CobolCase,
        TrainCase,
        AlternatingCase,
        InverseCase,
        RandomCase,
        SortLinesAlphabetically,
        SortLinesAlphabeticallyDescending,
        SortLinesByLastWord,
        SortLinesByLastWordDescending,
        ReverseLines,
        ShuffleLines,
    }

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(AnalyzerAndUtilities.InputOptionDescription))]
    private OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "action",
        Alias = "a",
        IsRequired = true,
        DescriptionResourceName = nameof(AnalyzerAndUtilities.ActionsOptionDescription))]
    private OperationType[] Actions { get; set; } = Array.Empty<OperationType>();

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(AnalyzerAndUtilities.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(AnalyzerAndUtilities.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> input = await Input.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!input.HasSucceeded)
        {
            Console.Error.WriteLine(AnalyzerAndUtilities.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(input.Data);

        string newText = input.Data;
        for (int i = 0; i < Actions.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            OperationType action = Actions[i];
            newText = action switch
            {
                OperationType.ConvertLineBreakToLF
                    => StringHelper.ConvertLineBreakToLF(newText),
                OperationType.ConvertLineBreakToCRLF
                    => StringHelper.ConvertLineBreakToCRLF(newText),
                OperationType.LowerCase
                    => newText.ToLowerInvariant(),
                OperationType.UpperCase
                    => newText.ToUpperInvariant(),
                OperationType.SentenceCase
                    => StringHelper.ConvertToSentenceCase(newText, cancellationToken),
                OperationType.TitleCase
                    => StringHelper.ConvertToTitleCase(newText, cancellationToken),
                OperationType.CamelCase
                    => StringHelper.ConvertToCamelCase(newText, cancellationToken),
                OperationType.PascalCase
                    => StringHelper.ConvertToPascalCase(newText, cancellationToken),
                OperationType.SnakeCase
                    => StringHelper.ConvertToSnakeCase(newText, cancellationToken),
                OperationType.ConstantCase
                    => StringHelper.ConvertToConstantCase(newText, cancellationToken),
                OperationType.KebabCase
                    => StringHelper.ConvertToKebabCase(newText, cancellationToken),
                OperationType.CobolCase
                    => StringHelper.ConvertToCobolCase(newText, cancellationToken),
                OperationType.TrainCase
                    => StringHelper.ConvertToTrainCase(newText, cancellationToken),
                OperationType.AlternatingCase
                    => StringHelper.ConvertToAlternatingCase(newText, cancellationToken),
                OperationType.InverseCase
                    => StringHelper.ConvertToInverseCase(newText, cancellationToken),
                OperationType.RandomCase
                    => StringHelper.ConvertToRandomCase(newText, cancellationToken),
                OperationType.SortLinesAlphabetically
                    => StringHelper.SortLinesAlphabetically(newText, StringHelper.DetectLineBreakKind(newText, cancellationToken)),
                OperationType.SortLinesAlphabeticallyDescending
                    => StringHelper.SortLinesAlphabeticallyDescending(newText, StringHelper.DetectLineBreakKind(newText, cancellationToken)),
                OperationType.SortLinesByLastWord
                    => StringHelper.SortLinesByLastWord(newText, StringHelper.DetectLineBreakKind(newText, cancellationToken)),
                OperationType.SortLinesByLastWordDescending
                    => StringHelper.SortLinesByLastWordDescending(newText, StringHelper.DetectLineBreakKind(newText, cancellationToken)),
                OperationType.ReverseLines
                    => StringHelper.ReverseLines(newText, StringHelper.DetectLineBreakKind(newText, cancellationToken)),
                OperationType.ShuffleLines
                    => StringHelper.ShuffleLines(newText, StringHelper.DetectLineBreakKind(newText, cancellationToken)),
                _ => throw new NotSupportedException(),
            };
        }

        await FileHelper.WriteOutputAsync(newText, OutputFile, cancellationToken);
        return 0;
    }
}
