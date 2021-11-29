#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.NumberBaseConverter
{
    [Export(typeof(IToolProvider))]
    [Name("Number Base Converter")]
    [ProtocolName("baseconverter")]
    [Order(0)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class NumberBaseConverterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.NumberBaseConverter.DisplayName;

        public string AccessibleName => LanguageManager.Instance.NumberBaseConverter.AccessibleName;


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
        public NumberBaseConverterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<NumberBaseConverterToolViewModel>();
        }
    }
}
