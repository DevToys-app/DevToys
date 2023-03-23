using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("JSON", baseName: "text")]
internal class MockJsonDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _))
        {
            try
            {
                var jtoken = JToken.Parse(dataString);
                if (jtoken is not null)
                {
                    return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: new Tuple<JToken, string>(jtoken, dataString)));
                }
            }
            catch
            {
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}

[Export(typeof(IDataTypeDetector))]
[DataTypeName("text")]
internal class MockTextDataTypeDetector : IDataTypeDetector
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
