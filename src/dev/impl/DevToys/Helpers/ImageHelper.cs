#nullable enable

using System;
using Windows.Graphics.Imaging;

namespace DevToys.Helpers
{
    public static class ImageHelper
    {
        public static Guid GetEncoderGuid(string format)
        {
            return format switch
            {
                "PNG" => BitmapEncoder.PngEncoderId,
                "JPEG" => BitmapEncoder.JpegEncoderId,
                "JPEGXR" => BitmapEncoder.JpegXREncoderId,
                "BMP" => BitmapEncoder.BmpEncoderId,
                "TIFF" => BitmapEncoder.TiffEncoderId,
                "HEIF" => BitmapEncoder.HeifEncoderId,
                "GIF" => BitmapEncoder.GifEncoderId,
                _ => throw new Exception()
            };
        }
    }
}
