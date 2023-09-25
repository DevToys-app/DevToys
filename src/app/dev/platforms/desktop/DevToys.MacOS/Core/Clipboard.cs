using DevToys.Api;
using System.Collections.Specialized;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;

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
                return GetClipboardFilesInternal();
            }
            else if (Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.HasText)
            {
                return await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.GetTextAsync();
            }
            else if (UIPasteboard.General.HasImages)
            {
                return await GetImageFromClipboardInternalAsync();
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
                return Task.FromResult<FileInfo[]?>(GetClipboardFilesInternal());
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }
        return Task.FromResult<FileInfo[]?>(null);
    }

    public async Task<Image<Rgba32>?> GetClipboardImageAsync()
    {
        try
        {
            if (UIPasteboard.General.HasImages)
            {
                return await GetImageFromClipboardInternalAsync();
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return null;
    }

    public async Task SetClipboardImageAsync(SixLabors.ImageSharp.Image? image)
    {
        if (image is not null)
        {
            var encoder = new PngEncoder
            {
                ColorType = PngColorType.RgbWithAlpha,
                TransparentColorMode = PngTransparentColorMode.Preserve,
                BitDepth = PngBitDepth.Bit8,
                CompressionLevel = PngCompressionLevel.BestSpeed
            };

            var pngMemoryStream = new MemoryStream();
            await image.SaveAsPngAsync(pngMemoryStream, encoder);
            pngMemoryStream.Seek(0, SeekOrigin.Begin);

            var data = NSData.FromStream(pngMemoryStream);
            if (data is not null)
            {
                var macImage = UIImage.LoadFromData(data);
                if (macImage is not null)
                {
                    UIPasteboard.General.SetData(macImage.AsPNG(), "public.png");
                }
            }
        }
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

    private static FileInfo[]? GetClipboardFilesInternal()
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

        return null;
    }

    private static async Task<Image<Rgba32>?> GetImageFromClipboardInternalAsync()
    {
        UIImage? imageFromPasteboard = UIPasteboard.General.Image;

        if (imageFromPasteboard is not null)
        {
            using Stream imageFromPasteboardStream = imageFromPasteboard.AsPNG().AsStream();
            using var pngMemoryStream = new MemoryStream();

            await imageFromPasteboardStream.CopyToAsync(pngMemoryStream);
            pngMemoryStream.Seek(0, SeekOrigin.Begin);

            using var image = SixLabors.ImageSharp.Image.Load(pngMemoryStream);
            imageFromPasteboard.Dispose();
            return image.CloneAs<Rgba32>(image.GetConfiguration());
        }

        return null;
    }
}
