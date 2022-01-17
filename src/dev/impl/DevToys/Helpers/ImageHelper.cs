#nullable enable

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace DevToys.Helpers
{
    public static class ImageHelper
    {
        public static bool IsPngFormat(string format) { return format is "PNG"; }
        public static bool IsJpegFormat(string format) { return format is "JPEG" or "JPEGXR"; }
        public static bool IsBmpFormat(string format) { return format is "BMP"; }
        public static bool IsTiffFormat(string format) { return format is "TIFF"; }
        public static bool IsHeifFormat(string format) { return format is "HEIF"; }
        public static bool IsGifFormat(string format) { return format is "GIF"; }


        public static string GetExtension(string format)
        {
            return format switch
            {
                "PNG" => ".png",
                "JPEG" => ".jpg",
                "JPEGXR" => ".jxr",
                "BMP" => ".bmp",
                "TIFF" => ".tif",
                "HEIF" => ".heic",
                "GIF" => ".gif",
                _ => throw new Exception()
            };
        }

        public static async Task<BitmapEncoder> GetEncoderAsync(string format, IRandomAccessStream encoderAccessStream)
        {
            var encodingOptions = new BitmapPropertySet();

            if (IsJpegFormat(format))
            {
                var booleanBitmapTypedValue = new BitmapTypedValue(1, PropertyType.Single);
                encodingOptions.Add("ImageQuality", booleanBitmapTypedValue);
            }

            if (encodingOptions is not null)
            {
                return await BitmapEncoder.CreateAsync(GetEncoderGuid(format), encoderAccessStream, encodingOptions);
            }
            else
            {
                return await BitmapEncoder.CreateAsync(GetEncoderGuid(format), encoderAccessStream);
            }
        }

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
