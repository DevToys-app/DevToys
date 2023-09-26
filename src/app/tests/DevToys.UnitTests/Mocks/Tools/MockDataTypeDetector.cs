using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("JSON", baseName: "text")]
internal class MockJsonDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _))
        {
            if (JsonHelper.IsValid(dataString, new MockILogger()))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}

[Export(typeof(IDataTypeDetector))]
[DataTypeName("text")]
internal class MockTextDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (data is string dataString && !string.IsNullOrEmpty(dataString))
        {
            return ValueTask.FromResult(new DataDetectionResult(true, dataString));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
