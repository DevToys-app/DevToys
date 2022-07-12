#nullable enable

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="Color"/> to a inverted <see cref="SolidColorBrush"/> value.
    /// </summary>
    public sealed class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueColor = value as Color?;
            if (valueColor == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return new SolidColorBrush(valueColor.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

