namespace DevToys.Api;

/// <summary>
/// Provides a set of helper method to play around with cancellation tokens.
/// </summary>
public static class CancellationTokenExtension
{
    /// <summary>
    /// Converts the <see cref="CancellationToken"/> to a <see cref="Task"/>
    /// that cancels when the <see cref="CancellationToken"/> is being canceled.
    /// </summary>
    public static Task AsTask(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>();
        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
        return tcs.Task;
    }
}
