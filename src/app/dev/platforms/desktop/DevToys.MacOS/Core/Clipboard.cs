using DevToys.Api;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.MacOS.Core;

[Export(typeof(Api.Core.IClipboard))]
internal sealed partial class Clipboard : Api.Core.IClipboard
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

    public Task SetClipboardBitmapAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardFilesAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardTextAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    [LoggerMessage(1, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);
}
