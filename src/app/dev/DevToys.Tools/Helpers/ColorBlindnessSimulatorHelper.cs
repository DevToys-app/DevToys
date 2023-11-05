using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DevToys.Tools.Helpers;

internal static class ColorBlindnessSimulatorHelper
{
    // Original source: https://github.com/DaltonLens/libDaltonLens/blob/master/libDaltonLens.c
    // http://daltonlens.org

    private struct Brettel1997Parameters
    {
        /// <summary>
        /// Transformation using plane 1 == rgbFromLms . projection1 . lmsFromRgb
        /// </summary>
        internal float[] RgbCvdFromRgb1 { get; set; }

        /// <summary>
        /// Full transformation using plane 2 == rgbFromLms . projection2 . lmsFromRgb
        /// </summary>
        internal float[] RgbCvdFromRgb2 { get; set; }

        /// <summary>
        /// Normal of the separation plane to pick the right transform, already in the RGB space == normalInLms . lmsFromRgb
        /// </summary>
        internal float[] SeparationPlaneNormalInRgb { get; set; }
    }

    /// <summary>
    /// Represents the default severity of the color blindness.
    /// </summary>
    private const float Severity = 1f;

    private static readonly Brettel1997Parameters ProtanopiaParameters
        = new()
        {
            RgbCvdFromRgb1 = new[] { 0.14980f, 1.19548f, -0.34528f, 0.10764f, 0.84864f, 0.04372f, 0.00384f, -0.00540f, 1.00156f },
            RgbCvdFromRgb2 = new[] { 0.14570f, 1.16172f, -0.30742f, 0.10816f, 0.85291f, 0.03892f, 0.00386f, -0.00524f, 1.00139f },
            SeparationPlaneNormalInRgb = new[] { 0.00048f, 0.00393f, -0.00441f }
        };

    private static readonly Brettel1997Parameters TritanopiaParameters
        = new()
        {
            RgbCvdFromRgb1 = new[] { 1.01277f, 0.13548f, -0.14826f, -0.01243f, 0.86812f, 0.14431f, 0.07589f, 0.80500f, 0.11911f },
            RgbCvdFromRgb2 = new[] { 0.93678f, 0.18979f, -0.12657f, 0.06154f, 0.81526f, 0.12320f, -0.37562f, 1.12767f, 0.24796f },
            SeparationPlaneNormalInRgb = new[] { 0.03901f, -0.02788f, -0.01113f }
        };

    private static readonly Brettel1997Parameters DeuteranopiaParameters
        = new()
        {
            RgbCvdFromRgb1 = new[] { 0.36477f, 0.86381f, -0.22858f, 0.26294f, 0.64245f, 0.09462f, -0.02006f, 0.02728f, 0.99278f },
            RgbCvdFromRgb2 = new[] { 0.37298f, 0.88166f, -0.25464f, 0.25954f, 0.63506f, 0.10540f, -0.01980f, 0.02784f, 0.99196f },
            SeparationPlaneNormalInRgb = new[] { -0.00281f, -0.00611f, 0.00892f }
        };

    internal static Image<Rgba32>? SimulateColorBlindness(
        Image<Rgba32> inputImage,
        ColorBlindnessMode colorBlindnessMode,
        Action<int> progressReport,
        CancellationToken cancellationToken)
    {
        return colorBlindnessMode switch
        {
            ColorBlindnessMode.Protanopia
                => SimulateColorBlindness(
                    inputImage,
                    ProtanopiaParameters,
                    progressReport,
                    cancellationToken),

            ColorBlindnessMode.Tritanopia
                => SimulateColorBlindness(
                    inputImage,
                    TritanopiaParameters,
                    progressReport,
                    cancellationToken),

            ColorBlindnessMode.Deuteranopia
                => SimulateColorBlindness(
                    inputImage,
                    DeuteranopiaParameters,
                    progressReport,
                    cancellationToken),

            _ => throw new NotSupportedException(),
        };
    }

    private static Image<Rgba32>? SimulateColorBlindness(Image<Rgba32> inputImage, Brettel1997Parameters brettel1997Parameters, Action<int> progressReport, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(inputImage);

        Image<Rgba32> outputImage = inputImage.Clone();
        int imagePixelCount = outputImage.Width * outputImage.Height;
        long pixelTreated = 0;
        float[]? separationPlaneNormalInRgb = brettel1997Parameters.SeparationPlaneNormalInRgb;

        try
        {
            outputImage
                .ProcessPixelRows(
                    (PixelAccessor<Rgba32> accessor) =>
                    {
                        for (int i = 0; i < outputImage.Height; i++)
                        {
                            Span<Rgba32> pixelRow = accessor.GetRowSpan(i);

                            // pixelRow.Length has the same value as accessor.Width,
                            // but using pixelRow.Length allows the JIT to optimize away bounds checks.
                            for (int j = 0; j < pixelRow.Length; j++)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                // Get a reference to the pixel at position j.
                                ref Rgba32 pixel = ref pixelRow[j];

                                // Convert the pixel to linear RGB.
                                float linearR = SRgbToLinearRgb(pixel.R);
                                float linearG = SRgbToLinearRgb(pixel.G);
                                float linearB = SRgbToLinearRgb(pixel.B);

                                float dotWithSepPlane
                                    = linearR * separationPlaneNormalInRgb[0]
                                    + linearG * separationPlaneNormalInRgb[1]
                                    + linearB * separationPlaneNormalInRgb[2];

                                float[] rgbCvdFromRgb;
                                if (dotWithSepPlane >= 0)
                                {
                                    rgbCvdFromRgb = brettel1997Parameters.RgbCvdFromRgb1;
                                }
                                else
                                {
                                    rgbCvdFromRgb = brettel1997Parameters.RgbCvdFromRgb2;
                                }

                                // Apply the color blindness transform.
                                float rCvd = rgbCvdFromRgb[0] * linearR + rgbCvdFromRgb[1] * linearG + rgbCvdFromRgb[2] * linearB; // R
                                float gCvd = rgbCvdFromRgb[3] * linearR + rgbCvdFromRgb[4] * linearG + rgbCvdFromRgb[5] * linearB; // G
                                float bCvd = rgbCvdFromRgb[6] * linearR + rgbCvdFromRgb[7] * linearG + rgbCvdFromRgb[8] * linearB; // B

                                // Apply the severity factor as a linear interpolation.
                                // It's the same to do it in the RGB space or in the LMS
                                // space since it's a linear transform.
                                rCvd = rCvd * Severity + linearR * (1f - Severity);
                                gCvd = gCvd * Severity + linearG * (1f - Severity);
                                bCvd = bCvd * Severity + linearB * (1f - Severity);

                                // Encode as sRGB and write the result.
                                var newPixel
                                    = new Rgba32(
                                        LinearRgbToSRgb(rCvd),
                                        LinearRgbToSRgb(gCvd),
                                        LinearRgbToSRgb(bCvd),
                                        pixel.A);

                                // Overwrite the pixel referenced by 'ref Rgba32 pixel':
                                pixel = newPixel;

                                pixelTreated++;
                                progressReport((int)(100 * pixelTreated / imagePixelCount));
                            }
                        }
                    });
        }
        catch (OperationCanceledException)
        {
            outputImage.Dispose();
            return null;
        }

        return outputImage;
    }

    private static float SRgbToLinearRgb(byte input)
    {
        float floatInput = input / 255f;
        if (floatInput < 0.04045f)
        {
            return floatInput / 12.92f;
        }

        return MathF.Pow((floatInput + 0.055f) / 1.055f, 2.4f);
    }

    private static byte LinearRgbToSRgb(float input)
    {
        if (input <= 0f)
        {
            return 0;
        }

        if (input >= 1f)
        {
            return 255;
        }

        if (input < 0.0031308f)
        {
            return (byte)(0.5f + (input * 12.92 * 255f));
        }

        return (byte)(255f * (MathF.Pow(input, 1f / 2.4f) * 1.055f - 0.055f));
    }
}
