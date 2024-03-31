namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.GZip, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class GZipTextDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _)
            && !string.IsNullOrWhiteSpace(dataString))
        {
            if (IsGZip(dataString))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    private static bool IsGZip(string? input)
    {
        ReadOnlySpan<char> trimmedInput = input.AsSpan().TrimStart();

        return trimmedInput is ['H', '4', 's', 'I', ..];
    }
}
