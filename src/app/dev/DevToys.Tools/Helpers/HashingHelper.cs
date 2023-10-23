using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using DevToys.Tools.Models;

namespace DevToys.Tools.Helpers;

internal static class HashingHelper
{
    internal static async Task<string> ComputeHashAsync(
        HashAlgorithmType hashAlgorithm,
        Stream inputStream,
        string? hmacSecretKey,
        Action<HashingProgress>? progressCallback,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNull(inputStream);
        Guard.IsNotEqualTo((int)HashAlgorithmType.None, (int)hashAlgorithm);

        using HashAlgorithm? algorithm = CreateHashAlgorithm(hashAlgorithm, hmacSecretKey);

        byte[]? fileHash
            = await ComputeHashAsync(
                algorithm,
                inputStream,
                progressCallback == null ? null : new Progress<HashingProgress>(progressCallback),
                cancellationToken);

        string? fileHashString
            = BitConverter
                .ToString(fileHash)
                .Replace("-", string.Empty);

        return fileHashString;
    }

    internal static async Task<byte[]> ComputeHashAsync(
        HashAlgorithm hashAlgorithm,
        Stream stream,
        IProgress<HashingProgress>? progressCallback,
        CancellationToken cancellationToken,
        int bufferSize = 1024 * 1024)
    {
        Guard.IsNotNull(stream, nameof(stream));
        Guard.IsNotNull(hashAlgorithm, nameof(hashAlgorithm));

        byte[] buffer = new byte[bufferSize];

        if (stream.Length == 0)
        {
            return hashAlgorithm.ComputeHash(Array.Empty<byte>());
        }

        long totalBytesRead = 0;

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) != 0)
        {
            hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
            totalBytesRead += bytesRead;
            ProgressAndCancellation();
        }

        hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
        ProgressAndCancellation();

        return hashAlgorithm.Hash ?? hashAlgorithm.ComputeHash(Array.Empty<byte>());

        void ProgressAndCancellation()
        {
            progressCallback?.Report(new HashingProgress(stream.Length, totalBytesRead, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static HashAlgorithm CreateHashAlgorithm(HashAlgorithmType hashingAlgorithm, string? hmacSecretKey)
    {
        if (string.IsNullOrEmpty(hmacSecretKey))
        {
            return CreateHashAlgorithm(hashingAlgorithm);
        }
        else
        {
            return CreateHmacAlgorithm(hashingAlgorithm, hmacSecretKey);
        }
    }

    private static HashAlgorithm CreateHashAlgorithm(HashAlgorithmType hashingAlgorithm)
    {
        return hashingAlgorithm switch
        {
            HashAlgorithmType.Md5 => MD5.Create(),
            HashAlgorithmType.Sha1 => SHA1.Create(),
            HashAlgorithmType.Sha256 => SHA256.Create(),
            HashAlgorithmType.Sha384 => SHA384.Create(),
            HashAlgorithmType.Sha512 => SHA512.Create(),
            _ => throw new ArgumentException("Hash Algorithm not supported", nameof(hashingAlgorithm))
        };
    }

    private static HashAlgorithm CreateHmacAlgorithm(HashAlgorithmType hashingAlgorithm, string hmacSecretKey)
    {
        byte[] key = Encoding.UTF8.GetBytes(hmacSecretKey);
        return hashingAlgorithm switch
        {
            HashAlgorithmType.Md5 => new HMACMD5(key),
            HashAlgorithmType.Sha1 => new HMACSHA1(key),
            HashAlgorithmType.Sha256 => new HMACSHA256(key),
            HashAlgorithmType.Sha384 => new HMACSHA384(key),
            HashAlgorithmType.Sha512 => new HMACSHA512(key),
            _ => throw new ArgumentException("Hash Algorithm not supported", nameof(hashingAlgorithm))
        };
    }
}
