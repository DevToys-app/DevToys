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
        public static bool IsJpegFormat(string format) { return string.Equals(format, "JPEG") || string.Equals(format, "JPEGXR"); }

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
                _ => throw new NotSupportedException()
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

            if (encodingOptions.Count > 0)
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
                _ => throw new NotSupportedException()
            };
        }
    }
}
