namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed partial class TextDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector)
    {
        if (data is string dataString && !string.IsNullOrEmpty(dataString))
        {
            return ValueTask.FromResult(new DataDetectionResult(true, dataString));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
