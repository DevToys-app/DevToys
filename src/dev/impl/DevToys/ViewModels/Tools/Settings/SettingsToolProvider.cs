#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DevToys.ViewModels.Settings
{
    [Export(typeof(IToolProvider))]
    [Name("Settings")]
    [ProtocolName("settings")]
    [CompactOverlaySize(width: 400, height: 500)]
    [IsFooterItem]
    internal sealed class SettingsToolProvider : ObservableRecipient, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.Settings.DisplayName;

        public object IconSource { get; }

        [ImportingConstructor]
        public SettingsToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;

            IconSource
                = new TaskCompletionNotifier<IconElement>(
                    ThreadHelper.RunOnUIThreadAsync(() =>
                    {
                        return Task.FromResult<IconElement>(
                            new AnimatedIcon
                            {
                                Source = new AnimatedSettingsVisualSource(),
                                FallbackIconSource = new Microsoft.UI.Xaml.Controls.FontIconSource
                                {
                                    FontFamily = (FontFamily)Application.Current.Resources["FluentSystemIcons"],
                                    Glyph = "\uF6A9"
                                }
                            });
                    }));
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<SettingsToolViewModel>();
        }
    }
}
