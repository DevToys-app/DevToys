#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="GridLength"/> value.
    /// </summary>
    public sealed class BooleanToGridLengthConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the grid length to apply when the input is true.
        /// </summary>
        public GridLength GridLengthOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the grid length to apply when the input is false.
        /// </summary>
        public GridLength GridLengthOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return valueBool.Value ? GridLengthOnTrue : GridLengthOnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
