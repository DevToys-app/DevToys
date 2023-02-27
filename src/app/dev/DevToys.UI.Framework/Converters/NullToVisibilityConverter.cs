using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Converts a null value to a <see cref="Visibility"/> value.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> when the input is null.
    /// </summary>
    public Visibility VisibilityIfNull { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> when the input is null.
    /// </summary>
    public Visibility VisibilityIfNotNull { get; set; }

    public bool EnforceNonWhiteSpaceString { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value?.GetType() == typeof(string))
        {
            if (EnforceNonWhiteSpaceString)
            {
                return string.IsNullOrWhiteSpace((string)value) ? VisibilityIfNull : VisibilityIfNotNull;
            }

            return string.IsNullOrEmpty((string)value) ? VisibilityIfNull : VisibilityIfNotNull;
        }

        return value == null ? VisibilityIfNull : VisibilityIfNotNull;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
