using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            catch (JsonReaderException)
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
