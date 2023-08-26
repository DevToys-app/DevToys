using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class UrlHelper
{
    internal static string EncodeOrDecode(string? input, EncodingConversion conversionMode, ILogger logger, CancellationToken cancellationToken)
    {
        string conversionResult
            = conversionMode switch
            {
                EncodingConversion.Encode
                    => EncodeUrlData(
                        input,
                        logger,
                        cancellationToken),

                EncodingConversion.Decode
                    => DecodeUrlData(
                        input,
                        logger,
                        cancellationToken),

                _ => throw new NotSupportedException(),
            };

        cancellationToken.ThrowIfCancellationRequested();

        return conversionResult;
    }

    internal static string EncodeUrlData(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        string encoded = string.Empty;
        try
        {
            encoded = Uri.EscapeDataString(data);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            // ignore;
        }
        catch (Exception ex)
        {
            LogFailEncodeUrl(logger, ex);
            return ex.Message;
        }

        return encoded;
    }

    internal static string DecodeUrlData(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return string.Empty;
        }

        string? decoded = string.Empty;

        try
        {
            decoded = Uri.UnescapeDataString(data);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is FormatException)
        {
            // ignore;
        }
        catch (Exception ex)
        {
            LogFailDecodeUrl(logger, ex);
            return ex.Message;
        }

        return decoded;
    }

    private static void LogFailEncodeUrl(ILogger logger, Exception exception)
    {
        logger.LogError(0, exception, $"Failed to encode text to Url.");
    }

    private static void LogFailDecodeUrl(ILogger logger, Exception exception)
    {
        logger.LogError(1, exception, $"Failed to decode Url.");
    }
}
