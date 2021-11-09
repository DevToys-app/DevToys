#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="ScrollMode"/> value.
    /// </summary>
    public sealed class BooleanToScrollModeConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the scroll mode to apply when the input is true.
        /// </summary>
        public ScrollMode ScrollModeOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the scroll mode to apply when the input is false.
        /// </summary>
        public ScrollMode ScrollModeOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return valueBool.Value ? ScrollModeOnTrue : ScrollModeOnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
