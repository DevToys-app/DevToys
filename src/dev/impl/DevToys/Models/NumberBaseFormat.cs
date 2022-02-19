using System;
using System.Globalization;

namespace DevToys.Models
{
    public class NumberBaseFormat : IEquatable<NumberBaseFormat>
    {
        private static NumberBaseConverterStrings Strings => LanguageManager.Instance.NumberBaseConverter;

        public static readonly NumberBaseFormat Binary = new(
            displayName: Strings.BinaryLabel,
            value: Radix.Binary,
            baseNumber: 2,
            groupSize: 4,
            groupSeparator: ' ');

        public static readonly NumberBaseFormat Octal = new(
            displayName: Strings.OctalLabel,
            value: Radix.Octal,
            baseNumber: 8,
            groupSize: CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes[0],
            groupSeparator: ' ');

        public static readonly NumberBaseFormat Decimal = new(
            displayName: Strings.DecimalLabel,
            value: Radix.Decimal,
            baseNumber: 10,
            groupSize: CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes[0],
            groupSeparator: Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator));

        public static readonly NumberBaseFormat Hexadecimal = new(
            displayName: Strings.HexadecimalLabel,
            value: Radix.Hexdecimal,
            baseNumber: 16,
            groupSize: 4,
            groupSeparator: ' ');

        public string DisplayName { get; }

        public Radix Value { get; }

        public int BaseNumber { get; }

        public int GroupSize { get; }

        public char GroupSeparator { get; }

        internal NumberBaseFormat(string displayName, Radix value, int baseNumber, int groupSize, char groupSeparator)
        {
            DisplayName = displayName;
            Value = value;
            BaseNumber = baseNumber;
            GroupSize = groupSize;
            GroupSeparator = groupSeparator;
        }

        public bool Equals(NumberBaseFormat other)
        {
            return other.Value == Value && other.BaseNumber == BaseNumber;
        }
    }
}
