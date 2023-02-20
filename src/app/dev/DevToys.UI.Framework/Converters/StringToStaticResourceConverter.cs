using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Convert a <see cref="string"/> to a static resource declared in the app.
/// </summary>
public sealed class StringToStaticResourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string? valueString = value as string;
        if (!string.IsNullOrEmpty(valueString))
        {
            if (Application.Current.Resources.TryGetValue(valueString, out object? result))
            {
                Guard.IsNotNull(result);
                Guard.IsAssignableToType(result, targetType);
                return result;
            }

            ThrowHelper.ThrowInvalidDataException($"{nameof(StringToStaticResourceConverter)}: Trying to load a XAML static resource '{valueString}' but it can't be found.");
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
