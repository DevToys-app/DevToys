#nullable enable

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.Shared.Core;

namespace DevToys.Helpers
{
    internal static class HashingHelper
    {
        internal static async Task<byte[]> ComputeHashAsync(
            HashAlgorithm hashAlgorithm,
            Stream stream,
            IProgress<HashingProgress> progress,
            CancellationToken cancellationToken,
            int bufferSize = 1024 * 1024)
        {
            Arguments.NotNull(stream, nameof(stream));
            Arguments.NotNull(hashAlgorithm, nameof(hashAlgorithm));

            byte[] buffer = new byte[bufferSize];

            if (stream.Length == 0)
            {
                return hashAlgorithm.ComputeHash(Array.Empty<byte>());
            }

            long totalBytesRead = 0;


            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
            {
                hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                totalBytesRead += bytesRead;
                ProgessAndCancellation();
            }
            hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
            ProgessAndCancellation();

            void ProgessAndCancellation()
            {
                progress.Report(new HashingProgress(stream.Length, totalBytesRead));
                cancellationToken.ThrowIfCancellationRequested();
            }

            return hashAlgorithm.Hash ?? hashAlgorithm.ComputeHash(Array.Empty<byte>());
        }

        internal static int ComputeHashIterations(Stream stream, int bufferSize = 1024 * 1024)
        {
            Arguments.NotNull(stream, nameof(stream));
            Arguments.NotZeroOrBelow(bufferSize, nameof(bufferSize));

            if(stream.Length == 0)
            {
                return 0;
            }
            else if(bufferSize >= stream.Length)
            {
                return 1;
            }

            return (int)(stream.Length / bufferSize);
        }
    }
}
