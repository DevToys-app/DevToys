#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="string"/> value.
    /// </summary>
    public sealed class BooleanToStringConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the text to apply when the input is true.
        /// </summary>
        public string? StringOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the text to apply when the input is false.
        /// </summary>
        public string? StringOnFalse { get; set; }

        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            bool? valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return valueBool.Value ? StringOnTrue : StringOnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
