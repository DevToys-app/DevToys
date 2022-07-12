#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DevToys.Models;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter
{
    internal static class NumberBaseFormatter
    {
        private static NumberBaseConverterStrings Strings => LanguageManager.Instance.NumberBaseConverter;

        /// <summary>
        /// Based on <see cref="System.ParseNumbers"/>
        /// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ParseNumbers.cs
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="baseNumber">Current base number <see cref="NumberBaseFormat"/></param>
        /// <returns>Value converted to <see cref="NumberBaseFormat.Decimal"/></returns>
        public static long? StringToBase(string value, NumberBaseFormat baseNumber)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int index = 0;

            Span<char> spanValue = value!.ToCharArray();
            int length = RemoveFormatting(spanValue);

            // Check for a sign
            int sign = 1;
            if (spanValue[index] == '-')
            {
                if (baseNumber != NumberBaseFormat.Decimal)
                {
                    throw new ArgumentException($"Base {baseNumber} can't have a negative number");
                }

                sign = -1;
                index++;
            }
            else if (spanValue[index] == '+')
            {
                index++;
            }

            long result = GetLong(baseNumber, spanValue, index, length);

            if (baseNumber == NumberBaseFormat.Decimal)
            {
                result *= sign;
            }

            return result;
        }

        /// <summary>
        /// Based on <see cref="System.ParseNumbers"/>
        /// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ParseNumbers.cs
        /// </summary>
        /// <param name="number">Current number to convert</param>
        /// <param name="baseNumber"></param>
        /// <param name="isFormatted">Define if the number need to base formatted</param>
        /// <returns></returns>
        public static string LongToBase(long number, NumberBaseFormat baseNumber, bool isFormatted)
        {
            Span<char> buffer = stackalloc char[67]; // Longest possible string length for an integer in binary notation with prefix

            // If the number is negative, make it positive and remember the sign.
            ulong ul;
            bool isNegative = false;
            if (number < 0)
            {
                isNegative = true;

                // For base 10, write out -num, but other bases write out the
                // 2's complement bit pattern
                ul = baseNumber == NumberBaseFormat.Decimal ? (ulong)-number : (ulong)number;
            }
            else
            {
                ul = (ulong)number;
            }

            // Special case the 0.
            int index;
            if (0 == ul)
            {
                buffer[0] = baseNumber.Dictionary[0];
                index = 1;
            }
            else
            {
                index = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    ulong div = ul / (ulong)baseNumber.BaseNumber;
                    int charVal = (int)(ul % (ulong)baseNumber.BaseNumber);
                    ul = div;

                    buffer[i] = baseNumber.Dictionary[charVal];

                    if (ul == 0)
                    {
                        index = i + 1;
                        break;
                    }
                }
            }

            if (baseNumber == NumberBaseFormat.Decimal)
            {
                // If it was negative, append the sign.
                if (isNegative)
                {
                    buffer[index++] = '-';
                }
            }

            return FormatNumber(buffer, baseNumber, isFormatted, index);
        }

        /// <summary>
        /// Format <paramref name="number"/> based on <paramref name="baseNumber"/> format definition 
        /// </summary>
        /// <param name="number">String representation of the number</param>
        /// <param name="baseNumber">Current base number <see cref="NumberBaseFormat"/></param>
        /// <returns>Formatted number based on <paramref name="baseNumber"/> format definition</returns>
        public static string FormatNumber(string number, NumberBaseFormat baseNumber)
        {
            char[] charArray = RemoveFormatting(number).ToCharArray();
            Array.Reverse(charArray);
            return FormatNumber(charArray, baseNumber, true, charArray.Length);
        }

        private static string FormatNumber(ReadOnlySpan<char> buffer, NumberBaseFormat baseNumber, bool isFormatted, int index)
        {
            var builder = new StringBuilder();
            if (buffer[index - 1] == '-')
            {
                builder.Append(buffer[--index]);
            }

            for (int builderIndex = --index; builderIndex >= 0; builderIndex--)
            {
                builder.Append(buffer[builderIndex]);
                if (isFormatted && builderIndex != 0 && builderIndex % baseNumber.GroupSize == 0)
                {
                    builder.Append(baseNumber.GroupSeparator);
                }
            }

            // Add padding left for Binary Format
            if (baseNumber == NumberBaseFormat.Binary)
            {
                int reminder = ++index % baseNumber.GroupSize;
                for (int padIndex = 0; reminder != 0 && padIndex < baseNumber.GroupSize - reminder; padIndex++)
                {
                    builder.Insert(0, '0');
                }
            }

            if (baseNumber.Dictionary.AllowsFormatting)
            {
                return builder.ToString().ToUpperInvariant();
            }
            return builder.ToString();
        }

        /// <summary>
        /// Remove formatting (whitespace and Culture separator)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveFormatting(string? value)
        {
            if (string.IsNullOrWhiteSpace(value!))
            {
                return string.Empty;
            }

            Span<char> valueSpan = value!.ToCharArray();
            int length = RemoveFormatting(valueSpan);
            var result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                result.Append(valueSpan[i]);
            }
            return result.ToString();
        }

        private static int RemoveFormatting(Span<char> values)
        {
            if (values.Length == 0)
            {
                return 0;
            }

            string currentCulture = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            int maxLength = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (!char.IsWhiteSpace(values[i]) && values[i] != Convert.ToChar(currentCulture))
                {
                    values[maxLength] = values[i];
                    maxLength++;
                }
            }
            return maxLength;
        }

        private static long GetLong(NumberBaseFormat baseNumber, ReadOnlySpan<char> spanValue, int index, int length)
        {
            ulong result = 0;
            ulong maxVal;
            if (baseNumber == NumberBaseFormat.Decimal)
            {
                maxVal = 0x7FFFFFFFFFFFFFFF / 10;
            }
            else
            {
                maxVal = 0xffffffffffffffff / (ulong)baseNumber.BaseNumber;
            }
            // Read all of the digits and convert to a number
            while (index < length)
            {
                if (!IsValidChar(spanValue[index], baseNumber, out int current))
                {
                    throw new InvalidOperationException(string.Format(Strings.ValueInvalid, baseNumber.DisplayName));
                }

                if (baseNumber == NumberBaseFormat.Decimal)
                {
                    // Check for overflows - this is sufficient & correct.
                    if (result > maxVal || result < 0)
                    {
                        throw new OverflowException(string.Format(Strings.ValueOverflow, long.MaxValue));
                    }

                    result = result * (ulong)baseNumber.BaseNumber + (ulong)current;
                    index++;
                }
                else
                {
                    // Check for overflows - this is sufficient & correct.
                    if (result > maxVal)
                    {
                        throw new OverflowException(string.Format(Strings.ValueOverflow, long.MaxValue));
                    }

                    ulong temp = result * (ulong)baseNumber.BaseNumber + (ulong)current;

                    if (temp < result) // this means overflow as well
                    {
                        throw new OverflowException(string.Format(Strings.ValueOverflow, long.MaxValue));
                    }

                    result = temp;
                    index++;
                }
            }

            if (baseNumber == NumberBaseFormat.Decimal && (long)result is < 0 and not 0x800000000000000)
            {
                throw new OverflowException(string.Format(Strings.ValueOverflow, long.MaxValue));
            }
            return (long)result;
        }

        private static bool IsValidChar(char c, NumberBaseFormat baseNumber, out int result)
        {
            for (result = 0; result < baseNumber.BaseNumber; result++)
            {
                if (baseNumber.Dictionary.AllowsFormatting)
                {
                    if (char.ToLowerInvariant(baseNumber.Dictionary[result]) == char.ToLowerInvariant(c))
                    {
                        return true;
                    }
                }
                else
                {
                    if (baseNumber.Dictionary[result] == c)
                    {
                        return true;
                    }
                }
            }
            result = -1;
            return false;
        }

        private static bool IsDigit(char c, int radix, out int result)
        {
            int tmp;

            if ((uint)(c - '0') <= 9)
            {
                result = tmp = c - '0';
            }
            else if ((uint)(c - 'A') <= 'Z' - 'A')
            {
                result = tmp = c - 'A' + 10;
            }
            else if ((uint)(c - 'a') <= 'z' - 'a')
            {
                result = tmp = c - 'a' + 10;
            }
            else
            {
                result = -1;
                return false;
            }
            return tmp < radix;
        }
    }
}
