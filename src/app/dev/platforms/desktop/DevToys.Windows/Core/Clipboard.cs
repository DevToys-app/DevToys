using System.Collections.Specialized;
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
                    if (System.Windows.Clipboard.ContainsFileDropList())
                    {
                        var files = new List<FileInfo>();
                        foreach (string? filePath in System.Windows.Clipboard.GetFileDropList())
                        {
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                files.Add(new FileInfo(filePath));
                            }
                        }
                        return files.ToArray();
                    }
                    else if (System.Windows.Clipboard.ContainsText())
                    {
                        return System.Windows.Clipboard.GetText();
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

    public Task<string?> GetClipboardTextAsync()
    {
        return ThreadHelper.RunOnUIThreadAsync<string?>(
            DispatcherPriority.Background,
            () =>
            {
                try
                {
                    if (System.Windows.Clipboard.ContainsText())
                    {
                        return System.Windows.Clipboard.GetText();
                    }
                }
                catch (Exception ex)
                {
                    LogGetClipboardFailed(ex);
                }

                return null;
            });
    }

    public Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        return ThreadHelper.RunOnUIThreadAsync<FileInfo[]?>(
            DispatcherPriority.Background,
            () =>
            {
                try
                {
                    if (System.Windows.Clipboard.ContainsFileDropList())
                    {
                        var files = new List<FileInfo>();
                        foreach (string? filePath in System.Windows.Clipboard.GetFileDropList())
                        {
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                files.Add(new FileInfo(filePath));
                            }
                        }
                        return files.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    LogGetClipboardFailed(ex);
                }

                return null;
            });
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
        return ThreadHelper.RunOnUIThreadAsync(
            DispatcherPriority.Background,
            () =>
            {
                try
                {
                    var fileList = new StringCollection();
                    if (data is not null)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            fileList.Add(data[i].FullName);
                        }
                    }
                    System.Windows.Clipboard.SetFileDropList(fileList);
                }
                catch (Exception ex)
                {
                    LogSetClipboardTextFailed(ex);
                }
            });
    }

    public Task SetClipboardTextAsync(string? data)
    {
        return ThreadHelper.RunOnUIThreadAsync(
            DispatcherPriority.Background,
            () =>
            {
                try
                {
                    System.Windows.Clipboard.SetText(data);
                }
                catch (Exception ex)
                {
                    LogSetClipboardTextFailed(ex);
                }
            });
    }

    [LoggerMessage(0, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Failed to set the clipboard text.")]
    partial void LogSetClipboardTextFailed(Exception ex);
}
