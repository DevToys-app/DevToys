using System.Xml;
using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.BuiltInDataTypeDetectors;

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
            if (IsValid(dataString, _logger))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    /// <summary>
    /// Detects whether the given string is a valid Xml or not.
    /// </summary>
    private static bool IsValid(string? input, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (!ValidateFirstAndLastCharOfXml(input))
        {
            return false;
        }

        try
        {
            var xmlDocument = new XmlDocument();

            // If loading failed, it's not valid Xml.
            xmlDocument.LoadXml(input);

            return true;
        }
        catch (XmlException)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid data detected.");
            return false;
        }
    }

    /// <summary>
    /// Validate that the XML starts with "<" and ends with ">", ignoring whitespace
    /// </summary>
    private static bool ValidateFirstAndLastCharOfXml(string input)
    {
        for (int i = 0; i < input.Length; ++i)
        {
            if (!char.IsWhiteSpace(input[i]))
            {
                if (input[i] == '<')
                {
                    break;
                }
                return false;
            }
        }

        for (int i = input.Length - 1; i >= 0; --i)
        {
            if (!char.IsWhiteSpace(input[i]))
            {
                if (input[i] == '>')
                {
                    return true;
                }
                return false;
            }
        }

        return false;
    }
}
