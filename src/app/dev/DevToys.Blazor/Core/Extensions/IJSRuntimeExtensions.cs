using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.Core.Extensions;

internal static class IJSRuntimeExtensions
{
    private static readonly ILogger logger = typeof(IJSRuntimeExtensions).Log();

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
    public static async ValueTask<bool> InvokeVoidWithErrorHandlingAsync(this IJSRuntime jsRuntime, string identifier, params object?[]? args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
            return true;
        }
        catch (JSException ex)
        {
            logger.LogError(ex, GetExceptionString(ex));
            return false;
        }
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
        catch (Exception ex)
        {
            // Do not log args as they can contain user personal information such as the text to set to a Monaco Editor instance.
            logger.LogError(ex, $"Unexpected exception while invoking a JavaScript function: '{identifier}'");
            return false;
        }
    }

    /// <summary>
    /// Invokes the specified JavaScript function asynchronously and catches <see cref="JSException"/>,
    /// <see cref="JSDisconnectedException"/> and <see cref="TaskCanceledException"/>.
    /// </summary>
    /// <param name="jsObjectReference">The <see cref="IJSObjectReference"/>.</param>
    /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will
    /// invoke the function <c>window.someScope.someFunction</c>.</param>
    /// <param name="args">JSON-serializable arguments.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous invocation operation and resolves to true in case
    /// no exception has occurred otherwise false.</returns>
    public static async ValueTask<bool> InvokeVoidWithErrorHandlingAsync(this IJSObjectReference jsObjectReference, string identifier, params object?[]? args)
    {
        try
        {
            await jsObjectReference.InvokeVoidAsync(identifier, args);
            return true;
        }
        catch (JSException ex)
        {
            logger.LogError(ex, GetExceptionString(ex));
            return false;
        }
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
        catch (Exception ex)
        {
            // Do not log args as they can contain user personal information such as the text to set to a Monaco Editor instance.
            logger.LogError(ex, $"Unexpected exception while invoking a JavaScript function: '{identifier}'");
            return false;
        }
    }

    private static string GetExceptionString(JSException ex)
    {
        if (ex.InnerException is null)
        {
            return
@$"===========================
JAVASCRIPT Exception caught:
MESSAGE: {ex.Message}
STACK TRACE:
{ex.StackTrace}
===========================";
        }

        return
@$"===========================
JAVASCRIPT Exception caught:
MESSAGE: {ex.Message}
STACK TRACE:
{ex.StackTrace}

== INNER EXCEPTION ==
MESSAGE: {ex.InnerException!.Message}
STACK TRACE:
{ex.InnerException!.StackTrace}
===========================";
    }
}
