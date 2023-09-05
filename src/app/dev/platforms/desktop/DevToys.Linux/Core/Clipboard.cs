using System.Text;
using DevToys.Api;
using Gdk;
using Microsoft.Extensions.Logging;

namespace DevToys.Linux.Core;

[Export(typeof(IClipboard))]
internal sealed partial class Clipboard : IClipboard
{
    private readonly ILogger _logger;

    [ImportingConstructor]
    public Clipboard()
    {
        _logger = this.Log();
    }

    public Task<string?> GetClipboardBitmapAsync()
    {
        // TODO
        throw new NotImplementedException();
    }

    public async Task<object?> GetClipboardDataAsync()
    {
        try
        {
            FileInfo[]? files = await GetClipboardFilesAsync();
            if (files is not null)
            {
                return files;
            }

            string? text = await GetClipboardTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }

            // TODO: Get bitmap from clipboard.
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return null;
    }

    public async Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        try
        {
            string? clipboardContentText = await GetClipboardTextAsync();
            if (clipboardContentText is not null)
            {
                string[] filePaths = clipboardContentText.Split('\n');
                var files = new List<FileInfo>();
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        files.Add(new FileInfo(filePath));
                    }
                    else
                    {
                        return null;
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
    }

    public async Task<string?> GetClipboardTextAsync()
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();
            var tcs = new TaskCompletionSource<string?>();

            Gdk.Internal.Clipboard.ReadTextAsync(
                clipboard.Handle,
                IntPtr.Zero,
                new Gio.Internal.AsyncReadyCallbackAsyncHandler(
                    (_, args, _) =>
                    {
                        string? result
                            = Gdk.Internal.Clipboard.ReadTextFinish(
                                clipboard.Handle,
                                args.Handle,
                                out GLib.Internal.ErrorOwnedHandle error)
                            .ConvertToString();

                        tcs.SetResult(result);
                    }).NativeCallback, IntPtr.Zero);

            string? clipboardContent = await tcs.Task;
            return clipboardContent;
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

    public Task SetClipboardFilesAsync(FileInfo[]? filePaths)
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();

            if (filePaths is not null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("copy"); // Important.

                for (int i = 0; i < filePaths.Length; i++)
                {
                    stringBuilder.Append($"\nfile://{filePaths[i].FullName}");
                }

                string newClipboardContentString = stringBuilder.ToString();
                byte[] data = Encoding.UTF8.GetBytes(newClipboardContentString);
                using var dataBytes = GLib.Bytes.New(data);
                using var content = ContentProvider.NewForBytes("x-special/gnome-copied-files", dataBytes);
                clipboard.SetContent(content);
            }
            else
            {
                clipboard.SetText(string.Empty);
            }
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }

        return Task.CompletedTask;
    }

    public Task SetClipboardTextAsync(string? data)
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();
            clipboard.SetText(data ?? string.Empty);
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(0, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Failed to set the clipboard text.")]
    partial void LogSetClipboardTextFailed(Exception ex);
}
