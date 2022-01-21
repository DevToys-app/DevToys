using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Models;

namespace DevToys.Helpers
{
    internal static class HashingHelper
    {
        internal static async Task<byte[]> ComputeHashAsync(
        HashAlgorithm hashAlgorithm,
        Stream stream,
        IProgress<HashingProgress> progress,
        CancellationToken cancellationToken = default,
        int bufferSize = 1024 * 1024)
        {
            byte[] readAheadBuffer = new byte[bufferSize];
            byte[] buffer;

            int readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0, readAheadBuffer.Length, cancellationToken);
            int bytesRead;

            long size = stream.Length;
            long totalBytesRead = readAheadBytesRead;

            do
            {
                bytesRead = readAheadBytesRead;
                buffer = readAheadBuffer;

                readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0, readAheadBuffer.Length, cancellationToken);
                totalBytesRead += readAheadBytesRead;

                if (readAheadBytesRead == 0)
                {
                    hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                }
                else
                {
                    hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                progress.Report(new HashingProgress(size, totalBytesRead));

                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

            } while (readAheadBytesRead != 0);

            return hashAlgorithm.Hash ?? Array.Empty<byte>();
        }
    }
}
