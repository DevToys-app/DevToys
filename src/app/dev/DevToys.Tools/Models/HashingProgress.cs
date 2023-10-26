namespace DevToys.Tools.Models;

internal sealed record HashingProgress
{
    internal HashingProgress(long totalBytes, long completedBytes, CancellationToken cancellationToken)
    {
        TotalBytes = totalBytes;
        CompletedBytes = completedBytes;
        CancellationToken = cancellationToken;
    }

    internal long TotalBytes { get; init; }

    internal long CompletedBytes { get; init; }

    internal CancellationToken CancellationToken { get; init; }

    internal double GetPercentage() => 100f * CompletedBytes / TotalBytes;
}
