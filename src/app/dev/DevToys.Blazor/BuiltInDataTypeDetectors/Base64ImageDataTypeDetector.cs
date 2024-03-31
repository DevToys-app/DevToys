namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Base64Image, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class Base64ImageDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _)
            && !string.IsNullOrWhiteSpace(dataString))
        {
            string? trimmedData = dataString.Trim();

            if (trimmedData is not null
                && (trimmedData.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/bmp;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/gif;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/x-icon;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/svg+xml;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase)))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
