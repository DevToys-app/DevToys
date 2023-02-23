using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Convert a <see cref="bool"/> to a <see cref="string"/> value.
/// </summary>
public sealed class BooleanToStringConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="string"/> when the input is true.
    /// </summary>
    public string? StringIfTrue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="string"/> when the input is false.
    /// </summary>
    public string? StringIfFalse { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        bool? valueBool = value as bool?;
        if (valueBool == null)
        {
            return DependencyProperty.UnsetValue;
        }

        if (valueBool.Value)
        {
            return StringIfTrue;
        }
        else
        {
            return StringIfFalse;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
