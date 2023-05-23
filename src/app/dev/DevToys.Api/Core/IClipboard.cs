namespace DevToys.Api;

/// <summary>
/// Provides a platform agnostic way to interact with the clipboard of the operating system the app is running on.
/// </summary>
public interface IClipboard
{
    /// <summary>
    /// Retrieves data from the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <returns>The data currently stored in the system clipboard.</returns>
    Task<object?> GetClipboardDataAsync();

    /// <summary>
    /// Sets text to the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <param name="data">The data to be stored in the system clipboard.</param>
    Task SetClipboardTextAsync(string? data);

    /// <summary>
    /// Sets files to the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <param name="data">The data to be stored in the system clipboard.</param>
    Task SetClipboardFilesAsync(string? data);

    /// <summary>
    /// Sets bitmap to the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <param name="data">The data to be stored in the system clipboard.</param>
    Task SetClipboardBitmapAsync(string? data);
}
