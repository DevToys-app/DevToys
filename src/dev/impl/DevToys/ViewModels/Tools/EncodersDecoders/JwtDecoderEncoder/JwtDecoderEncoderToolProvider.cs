#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Helpers;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.JwtDecoderEncoder
{
    [Export(typeof(IToolProvider))]
    [Name("Jwt Decoder / Encoder")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("jwt")]
    [Order(1)]
    [NotScrollable]
    internal sealed class JwtDecoderEncoderToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.JwtDecoderEncoder.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.JwtDecoderEncoder.SearchDisplayName;

        public string? Description => LanguageManager.Instance.JwtDecoderEncoder.Description;

        public string AccessibleName => LanguageManager.Instance.JwtDecoderEncoder.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.JwtDecoderEncoder.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("JWT.svg");

        [ImportingConstructor]
        public JwtDecoderEncoderToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return JwtHelper.IsValid(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JwtDecoderEncoderToolViewModel>();
        }
    }
}
