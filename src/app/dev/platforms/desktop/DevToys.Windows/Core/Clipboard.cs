using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.Windows.Core;

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
            await Task.CompletedTask;
            return string.Empty; // TODO: retrieve clipboard's text, image and files
        }
        catch (Exception ex)
        {
            //  LogGetClipboardFailed(ex);
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

    //[LoggerMessage(1, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    //partial void LogGetClipboardFailed(Exception ex);
}
