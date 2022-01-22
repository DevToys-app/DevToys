using System;

namespace DevToys.Models
{
    public class HashingProgress
    {
        public long TotalBytes { get; }
        public long CompletedBytes { get; }

        public int GetPercentage() => (int)Math.Floor(100f * CompletedBytes / TotalBytes);

        public HashingProgress(long totalBytes, long completedBytes = 0)
        {
            TotalBytes = totalBytes;
            CompletedBytes = completedBytes;
        }
    }
}
