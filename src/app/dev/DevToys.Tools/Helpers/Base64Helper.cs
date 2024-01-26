using System.Text;
using System.Text.RegularExpressions;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class Base64Helper
{
    internal static bool IsBase64DataStrict(string? data)
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

    internal static string FromTextToBase64(string? data, Base64Encoding encoding, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        string? encoded;
        try
        {
            Encoding encoder = GetEncoder(encoding);
            byte[]? dataBytes = encoder.GetBytes(data);

            cancellationToken.ThrowIfCancellationRequested();

            encoded = Convert.ToBase64String(dataBytes);

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch (Exception ex)
        {
            LogFailEncodeBase64(logger, ex, encoding);
            return ex.Message;
        }

        return encoded;
    }

    internal static string FromBase64ToText(string? data, Base64Encoding encoding, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        int remainder = data!.Length % 4;
        if (remainder > 0)
        {
            int padding = 4 - remainder;
            data = data.PadRight(data.Length + padding, '=');
        }

        string? decoded = string.Empty;

        try
        {
            byte[]? decodedData = Convert.FromBase64String(data);
            cancellationToken.ThrowIfCancellationRequested();

            Encoding encoder = GetEncoder(encoding);

            if (encoder is UTF8Encoding && decodedData != null)
            {
                byte[] preamble = encoder.GetPreamble();
                if (decodedData.Take(preamble.Length).SequenceEqual(preamble))
                {
                    // need to keep it this way to have the dom char
                    decoded += Encoding.Unicode.GetString(preamble, 0, 1);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (decodedData is not null)
            {
                decoded += encoder.GetString(decodedData);
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is FormatException)
        {
            // ignore;
        }
        catch (Exception ex)
        {
            LogFailDecodeBase64(logger, ex, encoding);
            return ex.Message;
        }

        return decoded;
    }

    private static Encoding GetEncoder(Base64Encoding encoding)
    {
        return encoding switch
        {
            Base64Encoding.Utf8 => new UTF8Encoding(true),
            Base64Encoding.Ascii => Encoding.ASCII,
            _ => throw new NotSupportedException(),
        };
    }

    private static void LogFailEncodeBase64(ILogger logger, Exception exception, Base64Encoding encoding)
    {
        logger.LogError(0, exception, "Failed to encode text to Base64. Encoding mode: '{encoding}'", encoding);
    }

    private static void LogFailDecodeBase64(ILogger logger, Exception exception, Base64Encoding encoding)
    {
        logger.LogError(1, exception, "Failed to decode Base64t to text. Encoding mode: '{encoding}'", encoding);
    }

    [GeneratedRegex("[^A-Z0-9+/=]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Base64Regex();
}
