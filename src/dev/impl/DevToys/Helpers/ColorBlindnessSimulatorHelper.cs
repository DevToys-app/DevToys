#nullable enable

using System;
using System.Threading;
using DevToys.Shared.Core;

namespace DevToys.Helpers
{
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
            = new Brettel1997Parameters
            {
                RgbCvdFromRgb1 = new[] { 0.14980f, 1.19548f, -0.34528f, 0.10764f, 0.84864f, 0.04372f, 0.00384f, -0.00540f, 1.00156f },
                RgbCvdFromRgb2 = new[] { 0.14570f, 1.16172f, -0.30742f, 0.10816f, 0.85291f, 0.03892f, 0.00386f, -0.00524f, 1.00139f },
                SeparationPlaneNormalInRgb = new[] { 0.00048f, 0.00393f, -0.00441f }
            };

        private static readonly Brettel1997Parameters TritanopiaParameters
            = new Brettel1997Parameters
            {
                RgbCvdFromRgb1 = new[] { 1.01277f, 0.13548f, -0.14826f, -0.01243f, 0.86812f, 0.14431f, 0.07589f, 0.80500f, 0.11911f },
                RgbCvdFromRgb2 = new[] { 0.93678f, 0.18979f, -0.12657f, 0.06154f, 0.81526f, 0.12320f, -0.37562f, 1.12767f, 0.24796f },
                SeparationPlaneNormalInRgb = new[] { 0.03901f, -0.02788f, -0.01113f }
            };

        private static readonly Brettel1997Parameters DeuteranopiaParameters
            = new Brettel1997Parameters
            {
                RgbCvdFromRgb1 = new[] { 0.36477f, 0.86381f, -0.22858f, 0.26294f, 0.64245f, 0.09462f, -0.02006f, 0.02728f, 0.99278f },
                RgbCvdFromRgb2 = new[] { 0.37298f, 0.88166f, -0.25464f, 0.25954f, 0.63506f, 0.10540f, -0.01980f, 0.02784f, 0.99196f },
                SeparationPlaneNormalInRgb = new[] { -0.00281f, -0.00611f, 0.00892f }
            };

        internal static byte[] SimulateProtanopia(byte[] bgra8SourcePixels, Action<int> progressReport, CancellationToken cancellationToken)
        {
            return SimulateColorBlindness(bgra8SourcePixels, ProtanopiaParameters, progressReport, cancellationToken);
        }

        internal static byte[] SimulateTritanopia(byte[] bgra8SourcePixels, Action<int> progressReport, CancellationToken cancellationToken)
        {
            return SimulateColorBlindness(bgra8SourcePixels, TritanopiaParameters, progressReport, cancellationToken);
        }

        internal static byte[] SimulateDeuteranopia(byte[] bgra8SourcePixels, Action<int> progressReport, CancellationToken cancellationToken)
        {
            return SimulateColorBlindness(bgra8SourcePixels, DeuteranopiaParameters, progressReport, cancellationToken);
        }

        private static byte[] SimulateColorBlindness(byte[] bgra8SourcePixels, Brettel1997Parameters brettel1997Parameters, Action<int> progressReport, CancellationToken cancellationToken)
        {
            Arguments.NotNull(bgra8SourcePixels, nameof(bgra8SourcePixels));

            byte[] result = new byte[bgra8SourcePixels.Length];

            for (int i = 0; i <= bgra8SourcePixels.Length - 4; i += 4)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Array.Empty<byte>();
                }

                float[] rgbPixel
                    = new float[3]
                    {
                        SRgbToLinearRgb(bgra8SourcePixels[i + 2]), // R
                        SRgbToLinearRgb(bgra8SourcePixels[i + 1]), // G
                        SRgbToLinearRgb(bgra8SourcePixels[i]),     // B
                    };

                float[]? separationPlaneNormalInRgb = brettel1997Parameters.SeparationPlaneNormalInRgb;
                float dotWithSepPlane = rgbPixel[0] * separationPlaneNormalInRgb[0] + rgbPixel[1] * separationPlaneNormalInRgb[1] + rgbPixel[2] * separationPlaneNormalInRgb[2];
                float[] rgbCvdFromRgb;
                if (dotWithSepPlane >= 0)
                {
                    rgbCvdFromRgb = brettel1997Parameters.RgbCvdFromRgb1;
                }
                else
                {
                    rgbCvdFromRgb = brettel1997Parameters.RgbCvdFromRgb2;
                }

                float[] rgbCvd
                    = new float[3]
                    {
                        rgbCvdFromRgb[0] * rgbPixel[0] + rgbCvdFromRgb[1] * rgbPixel[1] + rgbCvdFromRgb[2] * rgbPixel[2], // R
                        rgbCvdFromRgb[3] * rgbPixel[0] + rgbCvdFromRgb[4] * rgbPixel[1] + rgbCvdFromRgb[5] * rgbPixel[2], // G
                        rgbCvdFromRgb[6] * rgbPixel[0] + rgbCvdFromRgb[7] * rgbPixel[1] + rgbCvdFromRgb[8] * rgbPixel[2], // B
                    };

                // Apply the severity factor as a linear interpolation.
                // It's the same to do it in the RGB space or in the LMS
                // space since it's a linear transform.
                rgbCvd[0] = rgbCvd[0] * Severity + rgbPixel[0] * (1f - Severity);
                rgbCvd[1] = rgbCvd[1] * Severity + rgbPixel[1] * (1f - Severity);
                rgbCvd[2] = rgbCvd[2] * Severity + rgbPixel[2] * (1f - Severity);

                // Encode as sRGB and write the result.
                result[i + 2] = LinearRgbToSRgb(rgbCvd[0]); // R
                result[i + 1] = LinearRgbToSRgb(rgbCvd[1]); // G
                result[i] = LinearRgbToSRgb(rgbCvd[2]);     // B
                result[i + 3] = bgra8SourcePixels[i + 3];   // A

                progressReport((int)(100 *  (long)i / bgra8SourcePixels.Length));
            }

            return result;
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
}
