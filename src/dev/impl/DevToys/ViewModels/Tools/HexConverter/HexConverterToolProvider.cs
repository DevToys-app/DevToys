#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.HexConverter
{
    [Export(typeof(IToolProvider))]
    [Name("Hex Converter")]
    [ProtocolName("hex")]
    [Order(0)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class HexConverterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.HexConverter.DisplayName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uFE2C"
                        });
                }));

        [ImportingConstructor]
        public HexConverterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<HexConverterToolViewModel>();
        }
    }
}
