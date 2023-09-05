using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;

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

    public ValueTask<DataDetectionResult> TryDetectDataAsync(
        object data,
        DataDetectionResult? resultFromBaseDetector,
        CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _))
        {
            if (JsonHelper.IsValid(dataString, _logger))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    [LoggerMessage(1, LogLevel.Error, "An unexpected exception happened while trying to detect some JSON data.")]
    partial void LogError(Exception ex);
}
