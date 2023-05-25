using System.IO;
using System.Windows.Threading;
using DevToys.Windows.Helpers;
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

    public Task<object?> GetClipboardDataAsync()
    {
        return ThreadHelper.RunOnUIThreadAsync<object?>(
            DispatcherPriority.Background,
            () =>
            {
                try
                {
                    if (System.Windows.Clipboard.ContainsText())
                    {
                        return System.Windows.Clipboard.GetText();
                    }
                    else if (System.Windows.Clipboard.ContainsFileDropList())
                    {
                        var files = new List<FileInfo>();
                        foreach (string? filePath in System.Windows.Clipboard.GetFileDropList())
                        {
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                files.Add(new FileInfo(filePath));
                            }
                        }
                        return files;
                    }
                    else if (System.Windows.Clipboard.ContainsImage())
                    {
                        // TODO: convert to a cross-platform compatible format?
                        return System.Windows.Clipboard.GetImage();
                    }
                }
                catch (Exception ex)
                {
                    LogGetClipboardFailed(ex);
                }

                return null;
            });
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
