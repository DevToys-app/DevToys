#nullable enable

using System;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.Models
{
    public class ColorPickerModeDisplayPair : IEquatable<ColorPickerModeDisplayPair>
    {
        private static ColorPickerStrings Strings => LanguageManager.Instance.ColorPicker;

        public static readonly ColorPickerModeDisplayPair HSV = new(Strings.ModeHSV, ColorSpectrumComponents.HueSaturation);

        public static readonly ColorPickerModeDisplayPair HSL = new(Strings.ModeHSL, ColorSpectrumComponents.SaturationValue);

        public string DisplayName { get; }

        public ColorSpectrumComponents Value { get; }

        private ColorPickerModeDisplayPair(string displayName, ColorSpectrumComponents value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(ColorPickerModeDisplayPair other)
        {
            return other.Value == Value;
        }
    }
}
