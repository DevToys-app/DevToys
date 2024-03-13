using System.Collections.Specialized;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevToys.Api;
using DevToys.Windows.Core.Helpers;
using DevToys.Windows.Helpers;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using Image = System.Drawing.Image;

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
                        return GetClipboardFilesInternal();
                    }
                    else if (System.Windows.Clipboard.ContainsText())
                    {
                        return System.Windows.Clipboard.GetText();
                    }
                    else if (System.Windows.Clipboard.ContainsImage())
                    {
                        return GetClipboardImageInternal();
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
                        return GetClipboardFilesInternal();
                    }
                }
                catch (Exception ex)
                {
                    LogGetClipboardFailed(ex);
                }

                return null;
            });
    }

    public Task<Image<Rgba32>?> GetClipboardImageAsync()
    {
        return ThreadHelper.RunOnUIThreadAsync<Image<Rgba32>?>(
            DispatcherPriority.Background,
            () =>
            {
                try
                {
                    if (System.Windows.Clipboard.ContainsImage())
                    {
                        return GetClipboardImageInternal();
                    }
                }
                catch (Exception ex)
                {
                    LogGetClipboardFailed(ex);
                }

                return null;
            });
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

    public async Task SetClipboardImageAsync(SixLabors.ImageSharp.Image? image)
    {
        try
        {
            if (image is not null)
            {
                using MemoryStream pngMemoryStream = ImageHelper.GetPngMemoryStreamFromImage(image);
                using MemoryStream dibMemoryStream = ImageHelper.GetDeviceIndependentBitmapFromImage(pngMemoryStream, image.Width, image.Height);
                using Image bmpImage = ImageHelper.GetBitmapFromImage(pngMemoryStream);

                await ThreadHelper.RunOnUIThreadAsync(
                    DispatcherPriority.Background,
                    () =>
                    {
                        var data = new DataObject();

                        // As standard bitmap, without transparency support
                        data.SetData(DataFormats.Bitmap, bmpImage, autoConvert: true);

                        // As PNG.
                        data.SetData("PNG", pngMemoryStream, autoConvert: false);

                        // As DIB. This is (wrongly) accepted as ARGB by many applications.
                        data.SetData(DataFormats.Dib, dibMemoryStream, autoConvert: false);

                        // The 'copy = true' argument means the MemoryStreams can be safely disposed after the operation.
                        System.Windows.Clipboard.SetDataObject(data, copy: true);
                    });
            }
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

    private static FileInfo[] GetClipboardFilesInternal()
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

    private static Image<Rgba32>? GetClipboardImageInternal()
    {
        System.Windows.IDataObject dataObject = System.Windows.Clipboard.GetDataObject();
        BitmapSource? bitmapSource = GetBitmapSourceFromDataObject(dataObject);

        if (bitmapSource is not null)
        {
            using var pngMemoryStream = new MemoryStream();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(pngMemoryStream);

            pngMemoryStream.Seek(0, SeekOrigin.Begin);

            using var image = SixLabors.ImageSharp.Image.Load(pngMemoryStream);
            return image.CloneAs<Rgba32>(image.Configuration);
        }

        return null;
    }

    private static BitmapSource? GetBitmapSourceFromDataObject(System.Windows.IDataObject dataObject)
    {
        string[] formats = dataObject.GetFormats(true);
        string firstFormat = formats[0];

        // Guess at Chromium and Moz Web Browsers which can just use WPF's formatting
        if (firstFormat == DataFormats.Bitmap || formats.Contains("text/_moz_htmlinfo"))
        {
            return System.Windows.Clipboard.GetImage();
        }

        // retrieve image first then convert to image source
        // to avoid those image types that don't work with WPF GetImage()
        using Bitmap? bitmap = GetBitmapFromDataObject(dataObject);

        // couldn't convert image
        if (bitmap == null)
        {
            return null;
        }

        return ImageHelper.BitmapToBitmapSource(bitmap);
    }

    private static Bitmap? GetBitmapFromDataObject(System.Windows.IDataObject dataObject)
    {
        try
        {
            string[] formats = dataObject.GetFormats(true);
            if (formats == null || formats.Length == 0)
            {
                return null;
            }

            string firstFormat = formats[0];

            if (formats.Contains("PNG"))
            {
                using var pngMemoryStream = (MemoryStream)dataObject.GetData("PNG");
                pngMemoryStream.Position = 0;
                return new Bitmap(pngMemoryStream);
            }
            // Guess at Chromium and Moz Web Browsers which can just use WPF's formatting
            else if (firstFormat == DataFormats.Bitmap || formats.Contains("text/_moz_htmlinfo"))
            {
                BitmapSource bitmapSource = System.Windows.Clipboard.GetImage();
                return ImageHelper.BitmapSourceToBitmap(bitmapSource);
            }
            else if (formats.Contains("System.Drawing.Bitmap")) // (first == DataFormats.Dib)
            {
                var bitmap = (Bitmap)dataObject.GetData("System.Drawing.Bitmap");
                return bitmap;
            }

            return System.Windows.Forms.Clipboard.GetImage() as Bitmap;
        }
        catch
        {
            return null;
        }
    }
}
