using System.Text.Json;
using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Yaml, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class YamlDataTypeDetector : IDataTypeDetector
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public YamlDataTypeDetector()
    {
        _logger = this.Log();
    }

    public ValueTask<DataDetectionResult> TryDetectDataAsync(
        object data,
        DataDetectionResult? resultFromBaseDetector,
        CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString)
        {
            if (YamlHelper.IsValid(dataString))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    [LoggerMessage(1, LogLevel.Error, "An unexpected exception happened while trying to detect some JSON data.")]
    partial void LogError(Exception ex);
}
