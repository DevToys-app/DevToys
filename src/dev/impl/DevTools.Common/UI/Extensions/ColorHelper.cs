#nullable enable

using DevTools.Core;
using System;
using System.Reflection;
using Windows.UI;

namespace DevTools.Common.UI.Extensions
{
    internal static class ColorHelper
    {
        /// <summary>
        /// Creates a <see cref="Windows.UI.Color"/> from a XAML color <see cref="string"/>. Any format used in XAML should work.
        /// </summary>
        /// <param name="colorString">The XAML color string.</param>
        /// <returns>The created <see cref="Windows.UI.Color"/>.</returns>
        public static Color ToColor(this string colorString)
        {
            Arguments.NotNullOrEmpty(colorString, nameof(colorString));

            if (colorString[0] == '#')
            {
                switch (colorString.Length)
                {
                    case 9:
                        {
                            uint num4 = Convert.ToUInt32(colorString.Substring(1), 16);
                            byte a = (byte)(num4 >> 24);
                            byte r2 = (byte)((num4 >> 16) & 0xFF);
                            byte g2 = (byte)((num4 >> 8) & 0xFF);
                            byte b9 = (byte)(num4 & 0xFF);
                            return Color.FromArgb(a, r2, g2, b9);
                        }
                    case 7:
                        {
                            uint num3 = Convert.ToUInt32(colorString.Substring(1), 16);
                            byte r = (byte)((num3 >> 16) & 0xFF);
                            byte g = (byte)((num3 >> 8) & 0xFF);
                            byte b8 = (byte)(num3 & 0xFF);
                            return Color.FromArgb(byte.MaxValue, r, g, b8);
                        }
                    case 5:
                        {
                            ushort num2 = Convert.ToUInt16(colorString.Substring(1), 16);
                            byte b4 = (byte)(num2 >> 12);
                            byte b5 = (byte)((num2 >> 8) & 0xF);
                            byte b6 = (byte)((num2 >> 4) & 0xF);
                            byte b7 = (byte)(num2 & 0xF);
                            b4 = (byte)((b4 << 4) | b4);
                            b5 = (byte)((b5 << 4) | b5);
                            b6 = (byte)((b6 << 4) | b6);
                            b7 = (byte)((b7 << 4) | b7);
                            return Color.FromArgb(b4, b5, b6, b7);
                        }
                    case 4:
                        {
                            ushort num = Convert.ToUInt16(colorString.Substring(1), 16);
                            byte b = (byte)((num >> 8) & 0xF);
                            byte b2 = (byte)((num >> 4) & 0xF);
                            byte b3 = (byte)(num & 0xF);
                            b = (byte)((b << 4) | b);
                            b2 = (byte)((b2 << 4) | b2);
                            b3 = (byte)((b3 << 4) | b3);
                            return Color.FromArgb(byte.MaxValue, b, b2, b3);
                        }
                    default:
                        throw new FormatException($"The {colorString} string passed in the colorString argument is not a recognized Color format.");
                }
            }

            if (colorString.Length > 3 && colorString[0] == 's' && colorString[1] == 'c' && colorString[2] == '#')
            {
                string[] array = colorString.Split(',');
                if (array.Length == 4)
                {
                    double num5 = double.Parse(array[0].Substring(3));
                    double num6 = double.Parse(array[1]);
                    double num7 = double.Parse(array[2]);
                    double num8 = double.Parse(array[3]);
                    return Color.FromArgb((byte)(num5 * 255.0), (byte)(num6 * 255.0), (byte)(num7 * 255.0), (byte)(num8 * 255.0));
                }

                if (array.Length == 3)
                {
                    double num9 = double.Parse(array[0].Substring(3));
                    double num10 = double.Parse(array[1]);
                    double num11 = double.Parse(array[2]);
                    return Color.FromArgb(byte.MaxValue, (byte)(num9 * 255.0), (byte)(num10 * 255.0), (byte)(num11 * 255.0));
                }

                throw new FormatException($"The {colorString} string passed in the colorString argument is not a recognized Color format (sc#[scA,]scR,scG,scB).");
            }

            PropertyInfo declaredProperty = typeof(Colors).GetTypeInfo().GetDeclaredProperty(colorString);
            if (declaredProperty != null)
            {
                return (Color)declaredProperty.GetValue(null);
            }

            throw new FormatException($"The {colorString} string passed in the colorString argument is not a recognized Color.");
        }
    }
}
