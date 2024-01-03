using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using static ZXing.Rendering.SvgRenderer;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace DevToys.Tools.Helpers;

internal static class QrCodeHelper
{
    internal static Image<Rgba32> GenerateQrCode(string text)
    {
        var barcodeWriter = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Height = 1024,
                Width = 1024,
                Margin = 2
            }
        };

        Image<Rgba32> image = barcodeWriter.Write(text);
        return image;
    }

    internal static string GenerateSvgQrCode(string text)
    {
        BarcodeWriterSvg barcodeWriter = new()
        {
            Format = BarcodeFormat.QR_CODE,
            Renderer = new SvgRenderer()
        };

        EncodingOptions encodingOptions = new()
        {
            Width = 1024,
            Height = 1024,
            Margin = 2,
        };
        encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.M);
        barcodeWriter.Options = encodingOptions;

        SvgImage svg = barcodeWriter.Write(text);
        return svg.Content;
    }

    internal static async Task<string> ReadQrCodeAsync(Stream stream, CancellationToken cancellationToken)
    {
        using Image<Rgba32> bitmap = await Image.LoadAsync<Rgba32>(stream, cancellationToken);

        var barcodeReader = new ZXing.ImageSharp.BarcodeReader<Rgba32>()
        {
            AutoRotate = true,
            Options =
                {
                    TryHarder = true,
                    TryInverted = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
        };

        Result decodedQrCode = barcodeReader.Decode(bitmap);

        if (decodedQrCode is not null && !string.IsNullOrWhiteSpace(decodedQrCode.Text))
        {
            return decodedQrCode.Text;
        }

        return string.Empty;
    }
}
