using DevToys.Api;
using Microsoft.Extensions.Logging;
using ObjCRuntime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;

namespace DevToys.MacOS.Core;

[Export(typeof(IClipboard))]
internal sealed partial class Clipboard : IClipboard
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
            using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
            if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSUrl)) }, null))
            {
                return GetClipboardFilesInternal();
            }
            else if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSString)) }, null))
            {
                return await GetClipboardTextAsync();
            }
            else if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSImage)) }, null))
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

    public Task<string?> GetClipboardTextAsync()
    {
        try
        {
            using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
            if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSString)) }, null))
            {
                NSObject[] strings = pasteboard.ReadObjectsForClasses(new Class[] { new(typeof(NSString)) }, null);

                if (strings.Length > 0 && strings[0] is NSString nsString)
                {
                    return Task.FromResult<string?>(nsString.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return Task.FromResult<string?>(null);
    }

    public Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        try
        {
            return Task.FromResult(GetClipboardFilesInternal());
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
            using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
            if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSImage)) }, null))
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

    public async Task SetClipboardImageAsync(Image? image)
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

            using var pngMemoryStream = new MemoryStream();
            await image.SaveAsPngAsync(pngMemoryStream, encoder);
            pngMemoryStream.Seek(0, SeekOrigin.Begin);

            using var nsImage = NSImage.FromStream(pngMemoryStream);
            if (nsImage is not null)
            {
                using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
                pasteboard.ClearContents();
                pasteboard.WriteObjects(new INSPasteboardWriting[] { nsImage });
            }
        }
    }

    public Task SetClipboardFilesAsync(FileInfo[]? data)
    {
        try
        {
            if (data is not null)
            {
                var fileList = new INSPasteboardWriting[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    fileList[i] = new NSUrl("file://" + data[i].FullName);
                }

                using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
                pasteboard.ClearContents();
                pasteboard.WriteObjects(fileList);
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
            using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
            pasteboard.ClearContents();
            pasteboard.WriteObjects(new INSPasteboardWriting[] { new NSString(data) });
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }

        return Task.CompletedTask;
    }

    private static FileInfo[]? GetClipboardFilesInternal()
    {
        using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
        if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSUrl)) }, null))
        {
            NSObject[] urls = pasteboard.ReadObjectsForClasses(new Class[] { new(typeof(NSUrl)) }, null);

            if (urls.Length > 0)
            {
                var files = new List<FileInfo>();
                foreach (NSObject urlObj in urls)
                {
                    if (urlObj is NSUrl { AbsoluteString: not null, Path: not null } filePath)
                    {
                        if (filePath.AbsoluteString.StartsWith("file:///"))
                        {
                            files.Add(new FileInfo(filePath.Path));
                        }
                    }
                }

                return files.ToArray();
            }
        }

        return null;
    }

    private static async Task<Image<Rgba32>?> GetImageFromClipboardInternalAsync()
    {
        using NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
        if (pasteboard.CanReadObjectForClasses(new Class[] { new(typeof(NSImage)) }, null))
        {
            NSObject[] images = pasteboard.ReadObjectsForClasses(new Class[] { new(typeof(NSImage)) }, null);

            if (images.Length > 0 && images[0] is NSImage imageFromPasteboard)
            {
                NSData? imageData = imageFromPasteboard.AsTiff();
                if (imageData is not null)
                {
                    await using Stream tiffData = imageData.AsStream();
                    using var imageFromPasteboardStream = new MemoryStream();
                    await tiffData.CopyToAsync(imageFromPasteboardStream);

                    imageFromPasteboardStream.Seek(0, SeekOrigin.Begin);

                    using Image image = await Image.LoadAsync(imageFromPasteboardStream);
                    imageFromPasteboard.Dispose();
                    return image.CloneAs<Rgba32>(image.GetConfiguration());
                }
            }
        }

        return null;
    }

    [LoggerMessage(0, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Failed to set the clipboard text.")]
    partial void LogSetClipboardTextFailed(Exception ex);
}
