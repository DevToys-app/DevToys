using System.Xml.Schema;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Xsd, baseName: PredefinedCommonDataTypeNames.Xml)]
internal sealed partial class XsdDataTypeDetector : IDataTypeDetector
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public XsdDataTypeDetector()
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
            if (IsValid(dataString, _logger))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    /// <summary>
    /// Detects whether the given string is a valid XSD or not.
    /// </summary>
    private static bool IsValid(string? input, ILogger logger)
    {
        try
        {
            using StringReader reader = new(input!);
            var xmlSchema = XmlSchema.Read(reader, null);
            return xmlSchema is not null;
        }
        catch (Exception ex) when (ex is XmlException || ex is XmlSchemaException)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid data detected.");
            return false;
        }
    }
}
