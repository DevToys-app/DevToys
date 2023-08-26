using System.IO.Compression;
using System.Text;
using DevToys.Tools.Tools.EncodersDecoders.GZip;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class GZipHelper
{
    internal static bool IsGZip(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        int index = -1;
        for (int i = 0; i < input.Length; i++)
        {
            if (!char.IsWhiteSpace(input[i]))
            {
                index = i;
                break;
            }
        }

        if (index > -1 && input.Length > index + 3)
        {
            bool isGZip
                = input[index] == 'H'
                && input[index + 1] == '4'
                && input[index + 2] == 's'
                && input[index + 3] == 'I';

            return isGZip;
        }

        return false;
    }

    internal static async Task<(string compressedData, double compressionPercentage)> CompressOrDecompressAsync(
        string? input,
        CompressionMode compressionMode,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        (string data, double differencePercentage) conversionResult;

        switch (compressionMode)
        {
            case CompressionMode.Compress:
                conversionResult
                    = await CompressGZipDataAsync(
                        input,
                        logger,
                        cancellationToken);
                break;

            case CompressionMode.Decompress:
                conversionResult
                    = await DecompressGZipDataAsync(
                        input,
                        logger,
                        cancellationToken);
                break;

            default:
                throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();

        return conversionResult;
    }

    internal static async Task<(string compressedData, double compressionPercentage)> CompressGZipDataAsync(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return (string.Empty, 0);
        }

        string compressed = string.Empty;
        try
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(data);
            using var outputStream = new MemoryStream();
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                await gZipStream.WriteAsync(inputBytes, cancellationToken);
            }

            compressed = Convert.ToBase64String(outputStream.ToArray());
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            // ignore;
        }
        catch (Exception ex)
        {
            LogFailCompressGZip(logger, ex);
            return (ex.Message, 0);
        }

        int difference = data.Length - compressed.Length;
        double percentageDifference = (double)difference / data.Length * 100;

        return (compressed, percentageDifference);
    }

    internal static async Task<(string decompressedData, double compressionPercentage)> DecompressGZipDataAsync(string? compressedData, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(compressedData))
        {
            return (string.Empty, 0);
        }

        string? decompressed = string.Empty;

        try
        {
            byte[] inputBytes = Convert.FromBase64String(compressedData);
            using var inputStream = new MemoryStream(inputBytes);
            using var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gZipStream);
            decompressed = await streamReader.ReadToEndAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            // ignore;
        }
        catch (FormatException)
        {
            return (GZipEncoderDecoder.InvalidGZipData, 0);
        }
        catch (Exception ex)
        {
            LogFailDecompressGZip(logger, ex);
            return (GZipEncoderDecoder.InvalidGZipData, 0);
        }

        int difference = decompressed.Length - compressedData.Length;
        double percentageDifference = (double)difference / decompressed.Length * 100;

        return (decompressed, percentageDifference);
    }

    private static void LogFailCompressGZip(ILogger logger, Exception exception)
    {
        logger.LogError(0, exception, $"Failed to compress text to GZip.");
    }

    private static void LogFailDecompressGZip(ILogger logger, Exception exception)
    {
        logger.LogError(1, exception, $"Failed to decompress GZip.");
    }
}
