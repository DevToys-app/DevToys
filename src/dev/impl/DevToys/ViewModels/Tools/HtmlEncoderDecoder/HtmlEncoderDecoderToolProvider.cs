#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;

namespace DevToys.ViewModels.Tools.HtmlEncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Html Encoder/Decoder")]
    [ProtocolName("html")]
    [Order(0)]
    internal sealed class HtmlEncoderDecoderToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.HtmlEncoderDecoder.DisplayName;

        public string AccessibleName => LanguageManager.Instance.HtmlEncoderDecoder.AccessibleName;

        public object IconSource => CreatePathIconFromPath(nameof(HtmlEncoderDecoderToolProvider));

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public HtmlEncoderDecoderToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<HtmlEncoderDecoderToolViewModel>();
        }
    }
}
