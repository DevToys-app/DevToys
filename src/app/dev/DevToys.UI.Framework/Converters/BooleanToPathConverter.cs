using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Convert a <see cref="bool"/> to a <see cref="Geometry"/> value.
/// </summary>
public sealed class BooleanToPathConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="string"/> when the input is true.
    /// </summary>
    public string? PathIfTrue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="string"/> when the input is false.
    /// </summary>
    public string? PathIfFalse { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool? valueBool = value as bool?;
        if (valueBool == null)
        {
            return DependencyProperty.UnsetValue;
        }

        if (valueBool.Value)
        {
            return (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), PathIfTrue);
        }
        else
        {
            return (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), PathIfFalse);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
