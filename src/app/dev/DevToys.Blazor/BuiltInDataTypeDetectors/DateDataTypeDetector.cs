namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Date, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class DateDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _)
            && !string.IsNullOrWhiteSpace(dataString))
        {
            if (long.TryParse(dataString, out long potentialTimestamp))
            {
                try
                {
                    DateTime dateTimeOffset = DateTime.UnixEpoch.AddSeconds(potentialTimestamp);
                    return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dateTimeOffset));
                }
                catch
                {
                    return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
                }
            }
            else if (DateTimeOffset.TryParse(dataString, out DateTimeOffset dateTimeOffset))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dateTimeOffset));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
