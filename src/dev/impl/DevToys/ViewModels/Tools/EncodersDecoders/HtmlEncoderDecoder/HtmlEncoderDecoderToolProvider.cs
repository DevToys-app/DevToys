#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.HtmlEncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Html Encoder/Decoder")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("html")]
    [Order(0)]
    internal sealed class HtmlEncoderDecoderToolProvider : ToolProviderBase, IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.HtmlEncoderDecoder.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.HtmlEncoderDecoder.SearchDisplayName;

        public string? Description => LanguageManager.Instance.HtmlEncoderDecoder.Description;

        public string AccessibleName => LanguageManager.Instance.HtmlEncoderDecoder.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.HtmlEncoderDecoder.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("HtmlEncoder.svg");

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
