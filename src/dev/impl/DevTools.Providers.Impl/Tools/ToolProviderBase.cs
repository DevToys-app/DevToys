#nullable enable

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace DevTools.Providers.Impl.Tools
{
    internal abstract class ToolProviderBase : ObservableRecipient
    {
        protected const string AssetsFolderPath = "ms-appx:///DevTools.Providers.Impl/Assets/";

        protected static PathIcon CreatePathIconFromPath(string pathMarkup)
        {
            return new PathIcon
            {
                Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathMarkup)
            };
        }
    }
}
