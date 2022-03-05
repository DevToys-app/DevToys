#nullable enable
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
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.Base16Dictionary);

        public static readonly NumberBaseFormat Octal = new(
            displayName: Strings.OctalLabel,
            value: Radix.Octal,
            baseNumber: 8,
            groupSize: CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes[0],
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.Base16Dictionary);

        public static readonly NumberBaseFormat Decimal = new(
            displayName: Strings.DecimalLabel,
            value: Radix.Decimal,
            baseNumber: 10,
            groupSize: CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes[0],
            groupSeparator: Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator),
            dictionary: NumberBaseDictionary.Base16Dictionary);

        public static readonly NumberBaseFormat Hexadecimal = new(
            displayName: Strings.HexadecimalLabel,
            value: Radix.Hexdecimal,
            baseNumber: 16,
            groupSize: 4,
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.Base16Dictionary);

        public static readonly NumberBaseFormat RFC4648_Base16 = new(
            displayName: "RFC-4648 Base16",
            value: Radix.RFC4648Standard,
            baseNumber: 16,
            groupSize: 4,
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.Base16Dictionary);

        public static readonly NumberBaseFormat RFC4648_Base32 = new(
            displayName: "RFC-4648 Base32",
            value: Radix.RFC4648Standard,
            baseNumber: 32,
            groupSize: 4,
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.RFC4648Base32Dictionary);

        public static readonly NumberBaseFormat RFC4648_Base32_ExtendedHex = new(
            displayName: "RFC-4648 Base32 Extended Hex",
            value: Radix.RFC4648Standard,
            baseNumber: 32,
            groupSize: 4,
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.RFC4648Base32ExHexDictionary);

        public static readonly NumberBaseFormat RFC4648_Base64 = new(
            displayName: "RFC-4648 Base64",
            value: Radix.RFC4648Standard,
            baseNumber: 64,
            groupSize: 4,
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.RFC4648Base64Dictionary);

        public static readonly NumberBaseFormat RFC4648_Base64UrlEncode = new(
            displayName: "RFC-4648 Base64 Url Encode",
            value: Radix.RFC4648Standard,
            baseNumber: 64,
            groupSize: 4,
            groupSeparator: ' ',
            dictionary: NumberBaseDictionary.RFC4648Base64UrlEncodeDictionary);

        public string DisplayName { get; }

        public Radix Value { get; }

        public int BaseNumber { get; set; }

        public int GroupSize { get; }

        public char GroupSeparator { get; }

        public NumberBaseDictionary Dictionary { get; set; }

        internal NumberBaseFormat(string displayName, Radix value, int baseNumber, int groupSize, char groupSeparator, NumberBaseDictionary? dictionary = null)
        {
            DisplayName = displayName;
            Value = value;
            BaseNumber = baseNumber;
            GroupSize = groupSize;
            GroupSeparator = groupSeparator;
            Dictionary = dictionary ?? NumberBaseDictionary.DefaultDictionary;
        }

        internal NumberBaseFormat(NumberBaseFormat other, int baseNumber, NumberBaseDictionary dictionary)
        {
            DisplayName = other.DisplayName;
            Value = other.Value;
            BaseNumber = baseNumber;
            GroupSeparator = other.GroupSeparator;
            GroupSize = other.GroupSize;
            Dictionary = dictionary;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public virtual bool Equals(NumberBaseFormat other)
        {
            return other.Value == Value && other.BaseNumber == BaseNumber && Dictionary.Equals(other.Dictionary);
        }
    }
}
