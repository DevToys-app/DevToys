#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.UrlEncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("URL Encoder/Decoder")]
    [ProtocolName("url")]
    [Order(0)]
    internal sealed class UrlEncoderDecoderToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.UrlEncoderDecoder.DisplayName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uF4E4"
                        });
                }));

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public UrlEncoderDecoderToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<UrlEncoderDecoderToolViewModel>();
        }
    }
}
