using System.Collections.Specialized;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;
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
            if (UIPasteboard.General.HasUrls && UIPasteboard.General.Urls is not null)
            {
                var files = new List<FileInfo>();
                foreach (NSUrl filePath in UIPasteboard.General.Urls)
                {
                    if (filePath.AbsoluteString is not null && filePath.Path is not null)
                    {
                        if (filePath.AbsoluteString.StartsWith("file:///"))
                        {
                            files.Add(new FileInfo(filePath.Path));
                        }
                    }
                }
                return files.ToArray();
            }
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

    public async Task<string?> GetClipboardTextAsync()
    {
        try
        {
            if (Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.HasText)
            {
                return await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.GetTextAsync();
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }
        return null;
    }

    public Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        try
        {
            if (UIPasteboard.General.HasUrls && UIPasteboard.General.Urls is not null)
            {
                var files = new List<FileInfo>();
                foreach (NSUrl filePath in UIPasteboard.General.Urls)
                {
                    if (filePath.AbsoluteString is not null && filePath.Path is not null)
                    {
                        if (filePath.AbsoluteString.StartsWith("file:///"))
                        {
                            files.Add(new FileInfo(filePath.Path));
                        }
                    }
                }
                return Task.FromResult<FileInfo[]?>(files.ToArray());
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }
        return Task.FromResult<FileInfo[]?>(null);
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
        try
        {
            NSUrl[]? fileList = null;
            if (data is not null)
            {
                fileList = new NSUrl[data.Length];
                if (data is not null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        fileList[i] = new NSUrl("file://" + data[i].FullName);
                    }
                }
            }

            UIPasteboard.General.Urls = fileList;
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }

        return Task.CompletedTask;
    }

    public async Task SetClipboardTextAsync(string? data)
    {
        try
        {
            await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(data);
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }
    }

    [LoggerMessage(0, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Failed to set the clipboard text.")]
    partial void LogSetClipboardTextFailed(Exception ex);
}
