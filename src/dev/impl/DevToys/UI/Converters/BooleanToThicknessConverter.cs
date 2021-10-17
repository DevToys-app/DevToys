#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="Thickness"/> value.
    /// </summary>
    public sealed class BooleanToThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the thickness to apply when the input is true.
        /// </summary>
        public Thickness ThicknessOnTrue { get; set; }

        /// <summary>
        /// Gets or sets the thickness to apply when the input is false.
        /// </summary>
        public Thickness ThicknessOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return valueBool.Value ? ThicknessOnTrue : ThicknessOnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
