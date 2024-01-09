
using DevToys.Tools.Helpers;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(InternalName, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed class EscapedCharactersDetector : IDataTypeDetector
{
    internal const string InternalName = "TextWithEscapedCharacters";

    public ValueTask<DataDetectionResult> TryDetectDataAsync(object rawData, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _)
            && !string.IsNullOrWhiteSpace(dataString))
        {
            bool hasEscapedCharacters = StringHelper.HasEscapeCharacters(dataString);

            if (hasEscapedCharacters)
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
