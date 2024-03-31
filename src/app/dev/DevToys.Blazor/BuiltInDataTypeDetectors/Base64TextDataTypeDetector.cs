using System.Text;
using System.Text.RegularExpressions;

namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Base64Text, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed partial class Base64TextDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !long.TryParse(dataString, out _)
            && !string.IsNullOrWhiteSpace(dataString))
        {
            string? trimmedData = dataString.Trim();
            bool isBase64 = IsBase64DataStrict(trimmedData);

            if (isBase64)
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }

    private static bool IsBase64DataStrict(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        data = data!.Trim();

        if (data.Length % 4 != 0)
        {
            return false;
        }

        if (Base64Regex().IsMatch(data))
        {
            return false;
        }

        int equalIndex = data.IndexOf('=');
        int length = data.Length;

        if (!(equalIndex == -1 || equalIndex == length - 1 || equalIndex == length - 2 && data[length - 1] == '='))
        {
            return false;
        }

        string? decoded;

        try
        {
            byte[]? decodedData = Convert.FromBase64String(data);
            decoded = Encoding.UTF8.GetString(decodedData);
        }
        catch (Exception)
        {
            return false;
        }

        //check for special chars that you know should not be there
        char current;
        for (int i = 0; i < decoded.Length; i++)
        {
            current = decoded[i];
            if (current == 65533)
            {
                return false;
            }

            if (!(current == 0x9
                || current == 0xA
                || current == 0xD
                || current >= 0x20 && current <= 0xD7FF
                || current >= 0xE000 && current <= 0xFFFD
                || current >= 0x10000 && current <= 0x10FFFF))
            {
                return false;
            }
        }

        return true;
    }

    [GeneratedRegex("[^A-Z0-9+/=]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Base64Regex();
}
