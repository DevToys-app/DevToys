using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DevToys.Tools.SmartDetection;

/// <summary>
/// Detect non-empty JSON arrays.
/// </summary>
[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.JsonArray, baseName: PredefinedCommonDataTypeNames.Json)]
internal sealed partial class JsonArrayDataTypeDetector : IDataTypeDetector
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public JsonArrayDataTypeDetector()
    {
        _logger = this.Log();
    }

    public ValueTask<DataDetectionResult> TryDetectDataAsync(
        object data,
        DataDetectionResult? resultFromBaseDetector,
        CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && IsJsonArray(dataString))
        {
            return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    private static bool IsJsonArray(string data)
    {
        try
        {
            var jtoken = JToken.Parse(data);
            return jtoken is JArray ja && ja.Count > 0;
        }
        catch (Exception)
        {
            // Exception in parsing json. It likely mean the text isn't a JSON.
            return false;
        }
    }

    [LoggerMessage(1, LogLevel.Error, "An unexpected exception happened while trying to detect some JSON Array data.")]
    partial void LogError(Exception ex);
}
