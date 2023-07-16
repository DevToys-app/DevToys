using System.ComponentModel.Composition;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
            try
            {
                var options = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Allow,
                    AllowTrailingCommas = true,
                    MaxDepth = int.MaxValue
                };
                var jsonDocument = JsonDocument.Parse(dataString, options);
                if (jsonDocument is not null)
                {
                    return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: new Tuple<JsonDocument, string>(jsonDocument, dataString)));
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
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (data is string dataString && !string.IsNullOrEmpty(dataString))
        {
            return ValueTask.FromResult(new DataDetectionResult(true, dataString));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
