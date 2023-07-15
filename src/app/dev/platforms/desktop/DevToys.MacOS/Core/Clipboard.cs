using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.MacOS.Core;

[Export(typeof(Api.IClipboard))]
internal sealed partial class Clipboard : Api.IClipboard
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public Clipboard()
    {
        _logger = this.Log();
    }

    public async Task<object?> GetClipboardDataAsync()
    {
        try
        {
            if (Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.HasText)
            {
                return await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.GetTextAsync();
            }
            else
            {
                // TODO: On Mac, use AppKit API to get file and bitmap from clipboard.
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }
        return null;
    }

    public Task<string?> GetClipboardTextAsync()
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task<string?> GetClipboardBitmapAsync()
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardBitmapAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardFilesAsync(FileInfo[]? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardTextAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    [LoggerMessage(0, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Failed to set the clipboard text.")]
    partial void LogSetClipboardTextFailed(Exception ex);
}
