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

            int readAheadBytes = await stream.ReadAsync(readAheadBuffer, 0, bufferSize, cancellationToken);
            int bytesRead;
            long totalBytesRead = readAheadBytes;

            do
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

            } while (readAheadBytes != 0);

            return hashAlgorithm.Hash ?? Array.Empty<byte>();
        }
    }
}
