#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Helpers;
using DevToys.Shared.Api.Core;

namespace DevToys.ViewModels.Tools.CertificateEncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Certificate Encoder/Decoder")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("certficate")]
    [Order(0)]
    internal sealed class CertificateEncoderDecoderToolProvider : IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.CertificateEncoderDecoder.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.CertificateEncoderDecoder.SearchDisplayName;

        public string? Description => LanguageManager.Instance.CertificateEncoderDecoder.Description;

        public string AccessibleName => LanguageManager.Instance.CertificateEncoderDecoder.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.CertificateEncoderDecoder.SearchKeywords;

        public string IconGlyph => "\u0135";

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public CertificateEncoderDecoderToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return CertificateHelper.TryDecodeCertificate(data, null, out string? _);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<CertificateEncoderDecoderToolViewModel>();
        }
    }
}
