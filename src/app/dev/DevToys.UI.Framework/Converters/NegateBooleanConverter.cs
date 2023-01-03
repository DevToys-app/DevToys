#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#endif

namespace DevToys.UI.Framework.Converters;

/// <summary>
/// Convert a <see cref="bool"/> to its inverted <see cref="bool"/> value.
/// </summary>
public sealed class NegateBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool? valueBool = value as bool?;
        if (valueBool == null)
        {
            ThrowHelper.ThrowInvalidOperationException();
        }

        return !valueBool.Value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
