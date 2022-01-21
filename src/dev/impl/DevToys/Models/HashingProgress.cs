using System;

namespace DevToys.Models
{
    public class HashingProgress
    {
        public long Total { get; }
        public long Completed { get; }

        public int GetPercentage() => (int)Math.Floor(100f * Completed / Total);

        public HashingProgress(long total, long completed = 0)
        {
            Total = total;
            Completed = completed;
        }
    }
}
