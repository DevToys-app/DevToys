#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="TextWrapping"/> value.
    /// </summary>
    public sealed class BooleanToTextWrappingConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the text wrapping to apply when the input is true.
        /// </summary>
        public TextWrapping TextWrappingOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the text wrapping to apply when the input is false.
        /// </summary>
        public TextWrapping TextWrappingOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return valueBool.Value ? TextWrappingOnTrue : TextWrappingOnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
