using DevToys.Tools.Helpers;

namespace DevToys.Tools.SmartDetection;

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
            if (GZipHelper.IsGZip(dataString))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
