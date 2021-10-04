#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevToys.UI.Converters
{
    /// <summary>
    /// Converts a null value to a <see cref="Visibility"/> value.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public bool EnforceNonWhiteSpaceString { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value?.GetType() == typeof(string))
            {
                if (IsInverted)
                {
                    if (EnforceNonWhiteSpaceString)
                    {
                        return !string.IsNullOrWhiteSpace((string)value) ? Visibility.Visible : Visibility.Collapsed;
                    }
                    return !string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
                }

                if (EnforceNonWhiteSpaceString)
                {
                    return !string.IsNullOrWhiteSpace((string)value) ? Visibility.Visible : Visibility.Collapsed;
                }
                return string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (IsInverted)
            {
                return value != null ? Visibility.Visible : Visibility.Collapsed;
            }
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
