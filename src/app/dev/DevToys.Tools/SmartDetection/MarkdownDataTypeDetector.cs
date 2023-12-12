using System.Text.RegularExpressions;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("Markdown", baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class MarkdownDataTypeDetector : IDataTypeDetector
{
    [GeneratedRegex("^#{1,6}\\s", RegexOptions.Compiled)]
    private static partial Regex HeaderRegex();

    [GeneratedRegex("\\*\\*.*\\*\\*", RegexOptions.Compiled)]
    private static partial Regex BoldRegex();

    [GeneratedRegex("\\*.*\\*", RegexOptions.Compiled)]
    private static partial Regex ItalicRegex();

    [GeneratedRegex("\\[.*\\]\\(.*\\)", RegexOptions.Compiled)]
    private static partial Regex LinkRegex();

    [GeneratedRegex("!\\[.*\\]\\(.*\\)", RegexOptions.Compiled)]
    private static partial Regex ImageRegex();

    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && dataString.Length <= 10_000) // Perf: Don't try to detect Markdown in large strings as RegEx are expensive.
        {
            // Check if the input string contains any of the Markdown elements
            if (HeaderRegex().IsMatch(dataString)
                || BoldRegex().IsMatch(dataString)
                || ItalicRegex().IsMatch(dataString)
                || LinkRegex().IsMatch(dataString)
                || ImageRegex().IsMatch(dataString))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
