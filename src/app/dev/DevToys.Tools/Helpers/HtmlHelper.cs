using System.Web;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class HtmlHelper
{
    internal static string EncodeHtmlData(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        string encoded = string.Empty;
        try
        {
            encoded = HttpUtility.HtmlEncode(data);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            // ignore;
        }
        catch (Exception ex)
        {
            LogFailEncodeHtml(logger, ex);
            return ex.Message;
        }

        return encoded;
    }

    internal static string DecodeHtmlData(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        string? decoded = string.Empty;

        try
        {
            decoded = HttpUtility.HtmlDecode(data);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is FormatException)
        {
            // ignore;
        }
        catch (Exception ex)
        {
            LogFailDecodeHtml(logger, ex);
            return ex.Message;
        }

        return decoded;
    }

    private static void LogFailEncodeHtml(ILogger logger, Exception exception)
    {
        logger.LogError(0, exception, $"Failed to encode text to Html.");
    }

    private static void LogFailDecodeHtml(ILogger logger, Exception exception)
    {
        logger.LogError(1, exception, $"Failed to decode Html.");
    }
}
