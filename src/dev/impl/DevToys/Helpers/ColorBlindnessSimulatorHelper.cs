#nullable enable

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

        private static Brettel1997Parameters ProtanopiaParameters
            = new Brettel1997Parameters
            {
                RgbCvdFromRgb1 = new[] { 0.14980f, 1.19548f, -0.34528f, 0.10764f, 0.84864f, 0.04372f, 0.00384f, -0.00540f, 1.00156f },
                RgbCvdFromRgb2 = new[] { 0.14570f, 1.16172f, -0.30742f, 0.10816f, 0.85291f, 0.03892f, 0.00386f, -0.00524f, 1.00139f },
                SeparationPlaneNormalInRgb = new[] { 0.00048f, 0.00393f, -0.00441f }
            };

        private static Brettel1997Parameters TritanopiaParameters
            = new Brettel1997Parameters
            {
                RgbCvdFromRgb1 = new[] { 1.01277f, 0.13548f, -0.14826f, -0.01243f, 0.86812f, 0.14431f, 0.07589f, 0.80500f, 0.11911f },
                RgbCvdFromRgb2 = new[] { 0.93678f, 0.18979f, -0.12657f, 0.06154f, 0.81526f, 0.12320f, -0.37562f, 1.12767f, 0.24796f },
                SeparationPlaneNormalInRgb = new[] { 0.03901f, -0.02788f, -0.01113f }
            };

        private static Brettel1997Parameters DeuteranopiaParameters
            = new Brettel1997Parameters
            {
                RgbCvdFromRgb1 = new[] { 0.36477f, 0.86381f, -0.22858f, 0.26294f, 0.64245f, 0.09462f, -0.02006f, 0.02728f, 0.99278f },
                RgbCvdFromRgb2 = new[] { 0.37298f, 0.88166f, -0.25464f, 0.25954f, 0.63506f, 0.10540f, -0.01980f, 0.02784f, 0.99196f },
                SeparationPlaneNormalInRgb = new[] { -0.00281f, -0.00611f, 0.00892f }
            };

        private static byte[] SimulateProtanopia(byte[] bgra8SourcePixels)
        {
            return SimulateColorBlindness(bgra8SourcePixels, ProtanopiaParameters);
        }

        private static byte[] SimulateTritanopia(byte[] bgra8SourcePixels)
        {
            return SimulateColorBlindness(bgra8SourcePixels, TritanopiaParameters);
        }

        private static byte[] SimulateDeuteranopia(byte[] bgra8SourcePixels)
        {
            return SimulateColorBlindness(bgra8SourcePixels, DeuteranopiaParameters);
        }

        private static byte[] SimulateColorBlindness(byte[] bgra8SourcePixels, Brettel1997Parameters brettel1997Parameters)
        {
            return null;
        }
    }
}
