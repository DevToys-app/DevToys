﻿#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="Enum"/> to a <see cref="Thickness"/> value.
    /// </summary>
    public sealed class EnumToThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the thickness to apply when the input is has the expected enum.
        /// </summary>
        public Thickness ThicknessOnEnumDetected { get; set; }

        /// <summary>
        /// Gets or sets the thickness to apply when the input is doesn't have the expected enum.
        /// </summary>
        public Thickness ThicknessOnEnumNotDetected { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null || value is not Enum)
            {
                return ThicknessOnEnumNotDetected;
            }

            string? currentState = value.ToString();
            string? stateStrings = parameter.ToString();

            string[]? stateStringsSplitted = stateStrings.Split(',');
            for (int i = 0; i < stateStringsSplitted.Length; i++)
            {
                if (string.Equals(currentState, stateStringsSplitted[i].Trim(), StringComparison.Ordinal))
                {
                    return ThicknessOnEnumDetected;
                }
            }

            return ThicknessOnEnumNotDetected;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
