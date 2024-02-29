using DevToys.Tools.Helpers;
using DevToys.Tools.Models.NumberBase;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("NumberBase", baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed class NumberBaseDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object rawData, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString)
        {
            if (NumberBaseHelper.TryDetectNumberBase(dataString, out INumberBaseDefinition<long>? numberBaseDefinition))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: (dataString, numberBaseDefinition)));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
