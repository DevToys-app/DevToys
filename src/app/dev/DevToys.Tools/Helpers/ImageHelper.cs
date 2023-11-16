using DevToys.Tools.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.Tools.Helpers;

internal static class ImageHelper
{
    internal static Task SaveAsync(Stream destinationStream, Image<Rgba32> image, ImageConverterSupportedFormat format, CancellationToken cancellationToken)
    {
        IImageFormat? imageFormat
            = format switch
            {
                ImageConverterSupportedFormat.BMP => BmpFormat.Instance,
                ImageConverterSupportedFormat.JPEG => JpegFormat.Instance,
                ImageConverterSupportedFormat.PBM => PbmFormat.Instance,
                ImageConverterSupportedFormat.PNG => PngFormat.Instance,
                ImageConverterSupportedFormat.TGA => TgaFormat.Instance,
                ImageConverterSupportedFormat.TIFF => TiffFormat.Instance,
                ImageConverterSupportedFormat.WEBP => WebpFormat.Instance,
                _ => null,
            };

        if (imageFormat is null)
        {
            ThrowHelper.ThrowNotSupportedException();
        }

        return image.SaveAsync(destinationStream, imageFormat, cancellationToken);
    }

    internal static async Task<Image<Rgba32>> LoadImageFromFileAsync(FileInfo file, CancellationToken cancellationToken)
    {
        using Stream inputFileStream = file.OpenRead();
        return await Image.LoadAsync<Rgba32>(inputFileStream, cancellationToken);
    }

    internal static async Task<Image<Rgba32>> LoadImageFromFileAsync(SandboxedFileReader file, CancellationToken cancellationToken)
    {
        using Stream inputFileStream = await file.GetNewAccessToFileContentAsync(cancellationToken);
        return await Image.LoadAsync<Rgba32>(inputFileStream, cancellationToken);
    }
}
