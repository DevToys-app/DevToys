using System.Buffers;
using System.Buffers.Text;
using System.IO.Compression;
using System.Text;
using DevToys.Tools.Tools.EncodersDecoders.GZip;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace DevToys.Tools.Helpers;

internal static partial class GZipHelper
{
    private static readonly RecyclableMemoryStreamManager streamManager = new();

    internal static bool IsGZip(string? input)
    {
        ReadOnlySpan<char> trimmedInput = input.AsSpan().TrimStart();

        return trimmedInput is ['H', '4', 's', 'I', ..];
    }

    internal static async Task<(string compressedData, double compressionPercentage)> CompressOrDecompressAsync(
        string? input,
        CompressionMode compressionMode,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        (string, double) conversionResult = compressionMode switch
        {
            CompressionMode.Compress => await CompressGZipDataAsync(input, logger, cancellationToken),
            CompressionMode.Decompress => await DecompressGZipDataAsync(input, logger, cancellationToken),
            _ => throw new NotSupportedException(),
        };
        cancellationToken.ThrowIfCancellationRequested();

        return conversionResult;
    }

    internal static async Task<(string compressedData, double compressionPercentage)> CompressGZipDataAsync(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (data is null)
        {
            return (string.Empty, 0);
        }

        byte[]? inputBytes = null;
        string compressed = string.Empty;

        try
        {
            inputBytes = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(data));
            int bytesWritten = Encoding.UTF8.GetBytes(data, inputBytes);

            using RecyclableMemoryStream outputStream = streamManager.GetStream();
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true))
            {
                await gZipStream.WriteAsync(inputBytes.AsMemory(0, bytesWritten), cancellationToken);
            }

            compressed = Convert.ToBase64String(outputStream.GetBuffer().AsSpan(0, (int)outputStream.Length));
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
        finally
        {
            if (inputBytes is not null)
            {
                ArrayPool<byte>.Shared.Return(inputBytes);
            }
        }

        int difference = data.Length - compressed.Length;
        double percentageDifference = (double)difference / data.Length * 100;

        return (compressed, percentageDifference);
    }

    internal static async Task<(string decompressedData, double compressionPercentage)> DecompressGZipDataAsync(string? compressedData, ILogger logger, CancellationToken cancellationToken)
    {
        if (compressedData is null)
        {
            return (string.Empty, 0);
        }

        byte[]? inputBytes = null;
        string decompressed = string.Empty;

        try
        {
            inputBytes = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(compressedData));
            int bytesWritten = Encoding.UTF8.GetBytes(compressedData, inputBytes);

            OperationStatus operationStatus = Base64.DecodeFromUtf8InPlace(inputBytes.AsSpan(0, bytesWritten), out bytesWritten);

            if (operationStatus is not OperationStatus.Done)
                return (GZipEncoderDecoder.InvalidGZipData, 0);

            using var inputStream = new MemoryStream(inputBytes);
            using var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gZipStream);
            decompressed = await streamReader.ReadToEndAsync(cancellationToken); // TODO: Perf risk. Read in chunks.
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
        finally
        {
            if (inputBytes is not null)
            {
                ArrayPool<byte>.Shared.Return(inputBytes);
            }
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
