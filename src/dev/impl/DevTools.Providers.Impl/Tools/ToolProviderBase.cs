#nullable enable

using DevTools.Core;
using DevTools.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace DevTools.Providers.Impl.Tools
{
    internal abstract class ToolProviderBase : ObservableRecipient
    {
        protected const string AssetsFolderPath = "ms-appx:///DevTools.Providers.Impl/Assets/";

        private readonly IThread _thread;

        public ToolProviderBase(IThread thread)
        {
            _thread = Arguments.NotNull(thread, nameof(thread));
        }

        protected TaskCompletionNotifier<IconElement> CreatePathIconFromPath(string resourceName)
        {
            Arguments.NotNullOrWhiteSpace(resourceName, nameof(resourceName));

            return new TaskCompletionNotifier<IconElement>(
                _thread,
                _thread.RunOnUIThreadAsync(() =>
                {
                    string? pathMarkup = Application.Current.Resources[resourceName] as string;
                    Assumes.NotNullOrWhiteSpace(pathMarkup, nameof(pathMarkup));
                    return Task.FromResult<IconElement>(new PathIcon
                    {
                        Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathMarkup)
                    });
                }));
        }
    }
}
