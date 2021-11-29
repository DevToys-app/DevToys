#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="int"/> value.
    /// </summary>
    public sealed class BooleanToIntegerConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the value to apply when the input is true.
        /// </summary>
        public int ValueOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the value to apply when the input is false.
        /// </summary>
        public int ValueOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
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
