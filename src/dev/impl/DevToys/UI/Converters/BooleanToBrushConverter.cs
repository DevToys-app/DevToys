#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="Brush"/> value.
    /// </summary>
    public sealed class BooleanToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the value to apply when the input is true.
        /// </summary>
        public Brush? ValueOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the value to apply when the input is false.
        /// </summary>
        public Brush? ValueOnFalse { get; set; }

        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            bool? valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return valueBool.Value ? ValueOnTrue : ValueOnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
