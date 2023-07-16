using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Json, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class JsonDataTypeDetector : IDataTypeDetector
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public JsonDataTypeDetector()
    {
        _logger = this.Log();
    }

    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _))
        {
            try
            {
                var options = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    MaxDepth = int.MaxValue
                };
                var jsonDocument = JsonDocument.Parse(dataString, options);
                if (jsonDocument is not null)
                {
                    return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: new Tuple<JsonDocument, string>(jsonDocument, dataString)));
                }
            }
            catch (JsonException)
            {
                // Exception in parsing json. It likely mean the text isn't a JSON.
            }
            catch (Exception ex) //some other exception
            {
                LogError(ex);
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    [LoggerMessage(1, LogLevel.Error, "An unexpected exception happened while trying to detect some JSON data.")]
    partial void LogError(Exception ex);
}
