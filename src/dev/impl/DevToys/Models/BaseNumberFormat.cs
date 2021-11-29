using System;
using System.Globalization;

namespace DevToys.Models
{
    public class BaseNumberFormat : IEquatable<BaseNumberFormat>
    {
        private static HexConverterStrings Strings => LanguageManager.Instance.HexConverter;

        public static readonly BaseNumberFormat Octal = new(
            displayName: Strings.OctalLabel,
            value: Radix.Octal,
            baseNumber:8,
            groupSize: CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes[0],
            groupSeparator: ' ');

        public static readonly BaseNumberFormat Binary = new(
            displayName: Strings.BinaryLabel,
            value: Radix.Binary,
            baseNumber: 2,
            groupSize: 4,
            groupSeparator: ' ');

        public static readonly BaseNumberFormat Decimal = new(
            displayName: Strings.DecimalLabel,
            value: Radix.Decimal,
            baseNumber: 10,
            groupSize: CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes[0],
            groupSeparator: Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator));

        public static readonly BaseNumberFormat Hexadecimal = new(
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

        private BaseNumberFormat(string displayName, Radix value, int baseNumber, int groupSize, char groupSeparator)
        {
            DisplayName = displayName;
            Value = value;
            BaseNumber = baseNumber;
            GroupSize = groupSize;
            GroupSeparator = groupSeparator;
        }

        public bool Equals(BaseNumberFormat other)
        {
            return other.Value == Value;
        }
    }
}
