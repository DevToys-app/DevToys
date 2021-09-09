#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevTools.Common.UI.Converters
{
    /// <summary>
    /// Converts a <see cref="double"/> value to a <see cref="GridLength"/> value.
    /// </summary>
    public sealed class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double valueDouble)
            {
                return new GridLength(valueDouble);
            }

            throw new Exception("Double expected");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
