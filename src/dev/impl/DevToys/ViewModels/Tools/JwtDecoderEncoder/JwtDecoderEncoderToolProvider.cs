#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Helpers;

namespace DevToys.ViewModels.Tools.JwtDecoderEncoder
{
    [Export(typeof(IToolProvider))]
    [Name("Jwt Decoder / Encoder")]
    [ProtocolName("jwt")]
    [Order(1)]
    [NotScrollable]
    internal sealed class JwtDecoderEncoderToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JwtDecoderEncoder.DisplayName;

        public string AccessibleName => LanguageManager.Instance.JwtDecoderEncoder.AccessibleName;

        public object IconSource => CreatePathIconFromPath(nameof(JwtDecoderEncoderToolProvider));

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
