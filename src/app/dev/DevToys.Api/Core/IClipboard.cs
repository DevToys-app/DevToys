using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
    /// Retrieves text from the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <returns>The text currently stored in the system clipboard, or null if nothing is present.</returns>
    Task<string?> GetClipboardTextAsync();

    /// <summary>
    /// Retrieves files from the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <returns>The files currently stored in the system clipboard, or null if nothing is present.</returns>
    Task<FileInfo[]?> GetClipboardFilesAsync();

    /// <summary>
    /// Retrieves the image from the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <returns>The image currently stored in the system clipboard, or null if nothing is present.</returns>
    Task<Image<Rgba32>?> GetClipboardImageAsync();

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
    /// <param name="filePaths">The list of files to be stored in the system clipboard.</param>
    Task SetClipboardFilesAsync(FileInfo[]? filePaths);

    /// <summary>
    /// Sets image to the system clipboard.
    /// </summary>
    /// <remarks>
    /// This method may try to access to the UI thread.
    /// </remarks>
    /// <param name="image">The image to be stored in the system clipboard.</param>
    Task SetClipboardImageAsync(Image? image);
}
