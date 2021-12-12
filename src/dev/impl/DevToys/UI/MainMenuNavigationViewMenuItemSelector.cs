#nullable enable

using System;
using DevToys.Api.Tools;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.UI
{
    public sealed class MainMenuNavigationViewMenuItemSelector : DataTemplateSelector
    {
        public DataTemplate ToolProvider { get; set; } = null!;

        public DataTemplate Separator { get; set; } = null!;

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is MatchedToolProvider)
            {
                return ToolProvider;
            }
            else if (item is Microsoft.UI.Xaml.Controls.NavigationViewItemSeparator navigationViewItemSeparator)
            {
                return Separator;
            }

            throw new NotSupportedException();
        }
    }
}
