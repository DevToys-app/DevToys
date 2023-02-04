using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Convert a <see cref="bool"/> to a <see cref="Visibility"/> value.
/// </summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> when the input is true.
    /// </summary>
    public Visibility VisibilityIfTrue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> when the input is false.
    /// </summary>
    public Visibility VisibilityIfFalse { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool? valueBool = value as bool?;
        if (valueBool == null)
        {
            return DependencyProperty.UnsetValue;
        }

        if (valueBool.Value)
        {
            return VisibilityIfTrue;
        }
        else
        {
            return VisibilityIfFalse;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
