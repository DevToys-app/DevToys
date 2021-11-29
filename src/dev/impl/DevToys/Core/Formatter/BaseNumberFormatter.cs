using System;
using System.Globalization;
using System.Text;
using DevToys.Models;

namespace DevToys.Core.Formatter
{
    internal static class BaseNumberFormatter
    {
        /// <summary>
        /// Based on <see cref="System.ParseNumbers"/>
        /// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ParseNumbers.cs
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="baseNumber">Current base number <see cref="BaseNumberFormat"/></param>
        /// <returns>Value converted to <see cref="BaseNumberFormat.Decimal"/></returns>
        public static long? StringToBase(string value, BaseNumberFormat baseNumber)
        {
            if (value.Length <= 0)
            {
                return null;
            }

            int length = value.Length;
            int index = 0;

            Span<char> values = RemoveFormatting(value);

            // Check for a sign
            int sign = 1;
            if (values[index] == '-')
            {
                if (baseNumber != BaseNumberFormat.Decimal)
                {
                    throw new ArgumentException($"Base {baseNumber} can't have a negative number");
                }

                sign = -1;
                index++;
            }
            else if (values[index] == '+')
            {
                index++;
            }

            long result = 0;
            long maxVal;

            // Allow all non-decimal numbers to set the sign bit.
            if (baseNumber == BaseNumberFormat.Decimal)
            {
                maxVal = 0x7FFFFFFFFFFFFFFF / 10;

                // Read all of the digits and convert to a number
                while (index < length && IsDigit(values[index], baseNumber.BaseNumber, out int current))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (result > maxVal || result < 0)
                    {
                        throw new OverflowException($"Unable to parse the current value exceded max value ({long.MaxValue})");
                    }

                    result = result * baseNumber.BaseNumber + current;
                    index++;
                }

                if (result is < 0 and not 0x800000000000000)
                {
                    throw new OverflowException($"Unable to parse the current value exceded max value ({long.MaxValue})");
                }
            }
            else
            {
                maxVal =
                    baseNumber.BaseNumber == 10 ? 0xfffffffffffffff / 10 :
                    baseNumber.BaseNumber == 16 ? 0xfffffffffffffff / 16 :
                    baseNumber.BaseNumber == 8 ? 0xfffffffffffffff / 8 :
                    0xfffffffffffffff / 2;

                // Read all of the digits and convert to a number
                while (index < values.Length && IsDigit(values[index], baseNumber.BaseNumber, out int current))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (result > maxVal)
                    {
                        throw new OverflowException($"Unable to parse the current value exceded max value ({long.MaxValue})");
                    }

                    long temp = result * baseNumber.BaseNumber + current;

                    if (temp < result) // this means overflow as well
                    {
                        throw new OverflowException($"Unable to parse the current value exceded max value ({long.MaxValue})");
                    }

                    result = temp;
                    index++;
                }
            }

            if (baseNumber == BaseNumberFormat.Decimal)
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
        public static string LongToBase(long number, BaseNumberFormat baseNumber, bool isFormatted)
        {
            Span<char> buffer = stackalloc char[67]; // Longest possible string length for an integer in binary notation with prefix

            // If the number is negative, make it positive and remember the sign.
            long ul;
            bool isNegative = false;
            if (number < 0)
            {
                isNegative = true;

                // For base 10, write out -num, but other bases write out the
                // 2's complement bit pattern
                ul = (10 == baseNumber.BaseNumber) ? -number : number;
            }
            else
            {
                ul = number;
            }

            // Special case the 0.
            int index;
            if (0 == ul)
            {
                buffer[0] = '0';
                index = 1;
            }
            else
            {
                index = 0;
                for (int i = 0; i < buffer.Length; i++) // for loop instead of do{...}while(l!=0) to help JIT eliminate span bounds checks
                {
                    long div = ul / baseNumber.BaseNumber; // TODO https://github.com/dotnet/runtime/issues/5213
                    int charVal = (int)(ul - (div * baseNumber.BaseNumber));
                    ul = div;

                    buffer[i] = (charVal < 10) ?
                        (char)(charVal + '0') :
                        (char)(charVal + 'a' - 10);

                    if (ul == 0)
                    {
                        index = i + 1;
                        break;
                    }
                }
            }

            if (baseNumber == BaseNumberFormat.Decimal)
            {
                // If it was negative, append the sign.
                if (isNegative)
                {
                    buffer[index++] = '-';
                }
            }

            var builder = new StringBuilder();

            for (int builderIndex = --index; builderIndex >= 0; builderIndex--)
            {
                builder.Append(buffer[builderIndex]);
                if (isFormatted && builderIndex != 0 && builderIndex % baseNumber.GroupSize == 0)
                {
                    builder.Append(baseNumber.GroupSeparator);
                }
            }

            // Add padding left for Binary Format
            if (baseNumber == BaseNumberFormat.Binary)
            {
                int reminder = ++index % baseNumber.GroupSize;
                for (int padIndex = 0; reminder != 0 && padIndex < baseNumber.GroupSize - reminder; padIndex++)
                {
                    builder.Insert(0, '0');
                }
            }

            return builder.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Remove formatting (whitespace and Culture separator)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Span<char> RemoveFormatting(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Span<char>.Empty;
            }

            string currentCulture = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            Span<char> values = new char[value.Length];
            int valueIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]) && value[i] != Convert.ToChar(currentCulture))
                {
                    values[valueIndex] = value[i];
                    valueIndex++;
                }
            }
            return values;
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
