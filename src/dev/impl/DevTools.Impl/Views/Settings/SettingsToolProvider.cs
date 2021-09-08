#nullable enable

using DevTools.Localization;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using System;
using System.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace DevTools.Impl.Views.Settings
{
    [Export(typeof(IToolProvider))]
    [Name("Settings")]
    [IsFooterItem]
    internal sealed class SettingsToolProvider : ObservableRecipient, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.Settings.DisplayName;

        public object IconSource { get; }
            = new AnimatedIcon
            {
                Source = new AnimatedSettingsVisualSource(),
                FallbackIconSource = new FontIconSource
                {
                    FontFamily = (FontFamily)Application.Current.Resources["FluentSystemIcons"],
                    Glyph = "\uF6A9"
                }
            };

        public bool CanBeTreatedByTool(string data)
        {
            throw new NotImplementedException();
        }

        public IToolViewModel CreateTool()
        {
            throw new NotImplementedException();
        }
    }
}
