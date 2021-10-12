#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(IToolProvider))]
    [Name("String Utilities")]
    [ProtocolName("string")]
    [Order(0)]
    [NotScrollable]
    internal sealed class StringUtilitiesToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.StringUtilities.DisplayName;

        public object IconSource { get; }

        [ImportingConstructor]
        public StringUtilitiesToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;

            IconSource
                = new TaskCompletionNotifier<IconElement>(
                    ThreadHelper.RunOnUIThreadAsync(() =>
                    {
                        return Task.FromResult<IconElement>(
                            new FontIcon
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["FluentSystemIcons"],
                                Glyph = "\uF793"
                            });
                    }));
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<StringUtilitiesToolViewModel>();
        }
    }
}
