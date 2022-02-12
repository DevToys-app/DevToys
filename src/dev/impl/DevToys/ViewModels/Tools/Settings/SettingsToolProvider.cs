#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DevToys.ViewModels.Settings
{
    [Export(typeof(IToolProvider))]
    [Name("Settings")]
    [ProtocolName("settings")]
    [CompactOverlaySize(width: 400, height: 500)]
    [MenuPlacement(MenuPlacement.Footer)]
    internal sealed class SettingsToolProvider : ObservableRecipient, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.Settings.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.Settings.SearchDisplayName;

        public string? Description => LanguageManager.Instance.Settings.Description;

        public string AccessibleName => MenuDisplayName;

        public string? SearchKeywords => LanguageManager.Instance.Settings.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource
            => new TaskCompletionNotifier<IconElement>(() =>
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

        [ImportingConstructor]
        public SettingsToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
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
