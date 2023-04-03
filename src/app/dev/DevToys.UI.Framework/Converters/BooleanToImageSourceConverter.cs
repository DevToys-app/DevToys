using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Convert a <see cref="bool"/> to a <see cref="ImageSource"/> value.
/// </summary>
public sealed class BooleanToImageSourceConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="ImageSource"/> when the input is true.
    /// </summary>
    public ImageSource? SourceIfTrue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ImageSource"/> when the input is false.
    /// </summary>
    public ImageSource? SourceIfFalse { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        bool? valueBool = value as bool?;
        if (valueBool == null)
        {
            return DependencyProperty.UnsetValue;
        }

        if (valueBool.Value)
        {
            return SourceIfTrue;
        }
        else
        {
            return SourceIfFalse;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
