using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Rectangle = System.Drawing.Rectangle;

namespace DevToys.Windows.Core.Helpers;

/// <summary>
/// An helper class to help with images.
/// </summary>
internal static partial class ImageHelper
{
    internal static Bitmap? BitmapSourceToBitmap(BitmapSource source)
    {
        if (source == null)
        {
            return null;
        }

        PixelFormat pixelFormat = PixelFormat.Format32bppArgb;  //Bgr32 equiv default
        if (source.Format == System.Windows.Media.PixelFormats.Bgr24)
        {
            pixelFormat = PixelFormat.Format24bppRgb;
        }
        else if (source.Format == System.Windows.Media.PixelFormats.Pbgra32)
        {
            pixelFormat = PixelFormat.Format32bppPArgb;
        }
        else if (source.Format == System.Windows.Media.PixelFormats.Prgba64)
        {
            pixelFormat = PixelFormat.Format64bppPArgb;
        }

        var bmp
            = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                pixelFormat);

        BitmapData data
            = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                pixelFormat);

        source.CopyPixels(
            Int32Rect.Empty,
            data.Scan0,
            data.Height * data.Stride,
            data.Stride);

        bmp.UnlockBits(data);

        return bmp;
    }

    internal static BitmapSource BitmapToBitmapSource(Bitmap bmp)
    {
        nint hBitmap = bmp.GetHbitmap();
        BitmapSource imageSource
            = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        DeleteObject(hBitmap);
        return imageSource;
    }

    internal static MemoryStream GetPngMemoryStreamFromImage(SixLabors.ImageSharp.Image image)
    {
        Guard.IsNotNull(image);

        var encoder = new PngEncoder
        {
            ColorType = PngColorType.RgbWithAlpha,
            TransparentColorMode = PngTransparentColorMode.Preserve,
            BitDepth = PngBitDepth.Bit8,
            CompressionLevel = PngCompressionLevel.BestSpeed
        };

        var pngMemoryStream = new MemoryStream();
        image.SaveAsPng(pngMemoryStream, encoder);
        pngMemoryStream.Seek(0, SeekOrigin.Begin);

        return pngMemoryStream;
    }

    internal static System.Drawing.Image GetBitmapFromImage(MemoryStream pngMemoryStream)
    {
        Guard.IsNotNull(pngMemoryStream);
        return System.Drawing.Image.FromStream(pngMemoryStream);
    }

    internal static MemoryStream GetDeviceIndependentBitmapFromImage(MemoryStream pngMemoryStream, int width, int height)
    {
        // Ensure image is 32bppARGB by painting it on a new 32bppARGB image.
        byte[] bm32bData = GetArgb32Bitmap(pngMemoryStream, width, height);

        // BITMAPINFOHEADER struct for DIB.
        int hdrSize = 0x28;
        byte[] fullImage = new byte[hdrSize + 12 + bm32bData.Length];

        //Int32 biSize;
        WriteIntToByteArray(fullImage, 0x00, 4, true, (uint)hdrSize);

        //Int32 biWidth;
        WriteIntToByteArray(fullImage, 0x04, 4, true, (uint)width);

        //Int32 biHeight;
        WriteIntToByteArray(fullImage, 0x08, 4, true, (uint)height);

        //Int16 biPlanes;
        WriteIntToByteArray(fullImage, 0x0C, 2, true, 1);

        //Int16 biBitCount;
        WriteIntToByteArray(fullImage, 0x0E, 2, true, 32);

        //BITMAPCOMPRESSION biCompression = BITMAPCOMPRESSION.BITFIELDS;
        WriteIntToByteArray(fullImage, 0x10, 4, true, 3);

        //Int32 biSizeImage;
        WriteIntToByteArray(fullImage, 0x14, 4, true, (uint)bm32bData.Length);

        // The aforementioned "BITFIELDS": color masks applied to the Int32 pixel value to get the R, G and B values.
        WriteIntToByteArray(fullImage, hdrSize + 0, 4, true, 0x00FF0000);
        WriteIntToByteArray(fullImage, hdrSize + 4, 4, true, 0x0000FF00);
        WriteIntToByteArray(fullImage, hdrSize + 8, 4, true, 0x000000FF);
        Array.Copy(bm32bData, 0, fullImage, hdrSize + 12, bm32bData.Length);

        var dibMemoryStream = new MemoryStream();
        dibMemoryStream.Write(fullImage, 0, fullImage.Length);
        return dibMemoryStream;
    }

    private static byte[] GetArgb32Bitmap(MemoryStream pngMemoryStream, int width, int height)
    {
        Guard.IsNotNull(pngMemoryStream);

        using var argb32Bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

        using (var graphic = Graphics.FromImage(argb32Bitmap))
        {
            graphic.DrawImage(
                System.Drawing.Image.FromStream(pngMemoryStream),
                new Rectangle(0, 0, argb32Bitmap.Width, argb32Bitmap.Height));
        }

        // Bitmap format has its lines reversed.
        argb32Bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
        return GetBitmapData(argb32Bitmap, out int stride);
    }

    private static byte[] GetBitmapData(Bitmap sourceImage, out int stride)
    {
        BitmapData sourceData
            = sourceImage.LockBits(
                new Rectangle(
                    0,
                    0,
                    sourceImage.Width,
                    sourceImage.Height),
                ImageLockMode.ReadOnly,
                sourceImage.PixelFormat);

        stride = sourceData.Stride;
        byte[] data = new byte[stride * sourceImage.Height];
        Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
        sourceImage.UnlockBits(sourceData);
        return data;
    }

    private static void WriteIntToByteArray(byte[] data, int startIndex, int bytes, bool littleEndian, uint value)
    {
        int lastByte = bytes - 1;
        if (data.Length < startIndex + bytes)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException("startIndex", $"Data array is too small to write a {bytes}-byte value at offset {startIndex}.");
        }

        for (int index = 0; index < bytes; index++)
        {
            int offs = startIndex + (littleEndian ? index : lastByte - index);
            data[offs] = (byte)(value >> (8 * index) & 0xFF);
        }
    }

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(IntPtr hObject);
}
