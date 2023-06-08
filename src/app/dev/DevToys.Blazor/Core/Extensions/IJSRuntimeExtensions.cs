namespace DevToys.Blazor.Core.Extensions;

internal static class IJSRuntimeExtensions
{
    /// <summary>
    /// Invokes the specified JavaScript function asynchronously and catches <see cref="JSException"/>,
    /// <see cref="JSDisconnectedException"/> and <see cref="TaskCanceledException"/>.
    /// </summary>
    /// <param name="jsRuntime">The <see cref="IJSRuntime"/>.</param>
    /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will
    /// invoke the function <c>window.someScope.someFunction</c>.</param>
    /// <param name="args">JSON-serializable arguments.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous invocation operation and resolves to true in case
    /// no exception has occurred otherwise false.</returns>
    public static async ValueTask<bool> InvokeVoidWithErrorHandlingAsync(this IJSRuntime jsRuntime, string identifier, params object[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
            return true;
        }
#if DEBUG
#else
        catch (JSException)
        {
            return false;
        }
#endif
        // catch pre-rending errors since there is no browser at this point.
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerender", StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
