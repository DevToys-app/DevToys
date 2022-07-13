#nullable enable

using System;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Base64ImageEncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Base64 Image Encoder/Decoder")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("base64img")]
    [Order(2)]
    internal sealed class Base64ImageEncoderDecoderToolProvider : IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.Base64ImageEncoderDecoder.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.Base64ImageEncoderDecoder.SearchDisplayName;

        public string? Description => LanguageManager.Instance.Base64ImageEncoderDecoder.Description;

        public string AccessibleName => LanguageManager.Instance.Base64ImageEncoderDecoder.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.Base64ImageEncoderDecoder.SearchKeywords;

        public string IconGlyph => "\u0102";

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public Base64ImageEncoderDecoderToolProvider(IMefProvider mefProvider)
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

            if (trimmedData is not null
                && (trimmedData.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/bmp;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/gif;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/x-icon;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/svg+xml;base64,", StringComparison.OrdinalIgnoreCase)
                || trimmedData.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<Base64ImageEncoderDecoderToolViewModel>();
        }
    }
}
