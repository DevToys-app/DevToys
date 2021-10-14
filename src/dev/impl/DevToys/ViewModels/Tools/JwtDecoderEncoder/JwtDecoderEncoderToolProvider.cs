#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using DevToys.Helpers;
using System.Composition;

namespace DevToys.ViewModels.Tools.JwtDecoderEncoder
{
    [Export(typeof(IToolProvider))]
    [Name("Jwt Decoder / Encoder")]
    [ProtocolName("jwtdecoderencoder")]
    [Order(0)]
    [NotScrollable]
    internal sealed class JwtDecoderEncoderToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JwtDecoderEncoder.DisplayName;

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
