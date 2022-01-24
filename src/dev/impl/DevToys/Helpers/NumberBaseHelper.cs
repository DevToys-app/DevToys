#nullable enable

using System.Globalization;

namespace DevToys.Helpers
{
    internal static class NumberBaseHelper
    {
        /// <summary>
        /// Detects whether the given string is a valid Hexadecimal Number or not.
        /// </summary>
        internal static bool IsValidHexadecimal(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return long.TryParse(input, NumberStyles.HexNumber, null, out _);
        }

        /// <summary>
        /// Detects whether the given string is a valid Binary Number or not.
        /// </summary>
        internal static bool IsValidBinary(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input!.Replace("\0", string.Empty);

            foreach (char @char in input!)
            {
                if (@char is not '0' and not '1')
                {
                    return false;
                }
            }

            return true;
        }

    }
}
