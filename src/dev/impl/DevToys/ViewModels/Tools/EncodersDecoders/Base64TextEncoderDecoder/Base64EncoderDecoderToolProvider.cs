#nullable enable

using System;
using System.Composition;
using System.Text;
using System.Text.RegularExpressions;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Base64EncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Base64 Encoder/Decoder")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("base64")]
    [Order(1)]
    internal sealed class Base64EncoderDecoderToolProvider : IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.Base64EncoderDecoder.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.Base64EncoderDecoder.SearchDisplayName;

        public string? Description => LanguageManager.Instance.Base64EncoderDecoder.Description;

        public string AccessibleName => LanguageManager.Instance.Base64EncoderDecoder.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.Base64EncoderDecoder.SearchKeywords;

        public string IconGlyph => "\u0100";

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public Base64EncoderDecoderToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            string? trimmedData = data.Trim();
            bool isBase64 = Base64Helper.IsBase64DataStrict(trimmedData);

            return isBase64;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<Base64EncoderDecoderToolViewModel>();
        }
    }
}
