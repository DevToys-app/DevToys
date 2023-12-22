using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Xml, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class XmlDataTypeDetector : IDataTypeDetector
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public XmlDataTypeDetector()
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
            if (XmlHelper.IsValid(dataString, _logger))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
