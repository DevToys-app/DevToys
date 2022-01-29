﻿#nullable enable

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

            byte[] readAheadBuffer = new byte[bufferSize];
            byte[] buffer;

            int readAheadBytes = await stream.ReadAsync(readAheadBuffer, 0, bufferSize, cancellationToken);
            int bytesRead;
            long totalBytesRead = readAheadBytes;

            while (readAheadBytes != 0)
            {
                bytesRead = readAheadBytes;
                buffer = readAheadBuffer;

                readAheadBytes = await stream.ReadAsync(readAheadBuffer, 0, bufferSize, cancellationToken);
                totalBytesRead += readAheadBytes;

                if (readAheadBytes == 0)
                {
                    hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                }
                else
                {
                    hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                progress.Report(new HashingProgress(stream.Length, totalBytesRead));
                cancellationToken.ThrowIfCancellationRequested();
            }
            return hashAlgorithm.Hash ?? Array.Empty<byte>();
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
