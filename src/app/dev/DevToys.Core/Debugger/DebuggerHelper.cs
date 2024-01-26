namespace DevToys.Core.Debugger;

public static class DebuggerHelper
{
    public static async Task<bool> WaitForDebuggerAsync(TimeSpan? limit = null)
    {
#if DEBUG
        limit ??= TimeSpan.FromSeconds(60);
        using var source = new CancellationTokenSource(limit.Value);

        Console.WriteLine($"◉ Waiting {limit.Value.TotalSeconds} secs for debugger (PID: {Environment.ProcessId})...");

        try
        {
            await Task.Run(async () =>
            {
                while (!System.Diagnostics.Debugger.IsAttached)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), source.Token);
                }
            }, source.Token);
        }
        catch (OperationCanceledException)
        {
            // it's ok
        }

        Console.WriteLine(System.Diagnostics.Debugger.IsAttached
            ? "✔ Debugger attached"
            : "✕ Continuing without debugger");

        return System.Diagnostics.Debugger.IsAttached;
#else
        return await Task.FromResult(false);
#endif
    }
}
