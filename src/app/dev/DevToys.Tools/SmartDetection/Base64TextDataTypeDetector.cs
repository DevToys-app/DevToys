using DevToys.Tools.Helpers;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Base64Text, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class Base64TextDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _)
            && !string.IsNullOrWhiteSpace(dataString))
        {
            string? trimmedData = dataString.Trim();
            bool isBase64 = Base64Helper.IsBase64DataStrict(trimmedData);

            if (isBase64)
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
