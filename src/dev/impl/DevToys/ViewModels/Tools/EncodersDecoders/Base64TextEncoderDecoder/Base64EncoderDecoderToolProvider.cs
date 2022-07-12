#nullable enable

using System;
using System.Composition;
using System.Text;
using System.Text.RegularExpressions;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Base64EncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Base64 Encoder/Decoder")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("base64")]
    [Order(1)]
    internal sealed class Base64EncoderDecoderToolProvider : ToolProviderBase, IToolProvider
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
            bool isBase64 = IsBase64DataStrict(trimmedData);

            return isBase64;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<Base64EncoderDecoderToolViewModel>();
        }

        private bool IsBase64DataStrict(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            if (data.Length % 4 != 0)
            {
                return false;
            }

            if (new Regex(@"[^A-Z0-9+/=]", RegexOptions.IgnoreCase).IsMatch(data))
            {
                return false;
            }

            int equalIndex = data.IndexOf('=');
            int length = data.Length;

            if (!(equalIndex == -1 || equalIndex == length - 1 || (equalIndex == length - 2 && data[length - 1] == '=')))
            {
                return false;
            }

            string? decoded;

            try
            {
                byte[]? decodedData = Convert.FromBase64String(data);
                decoded = Encoding.UTF8.GetString(decodedData);
            }
            catch (Exception)
            {
                return false;
            }

            //check for special chars that you know should not be there
            char current;
            for (int i = 0; i < decoded.Length; i++)
            {
                current = decoded[i];
                if (current == 65533)
                {
                    return false;
                }

#pragma warning disable IDE0078 // Use pattern matching
                if (!(current == 0x9
                    || current == 0xA
                    || current == 0xD
                    || (current >= 0x20 && current <= 0xD7FF)
                    || (current >= 0xE000 && current <= 0xFFFD)
                    || (current >= 0x10000 && current <= 0x10FFFF)))
#pragma warning restore IDE0078 // Use pattern matching
                {
                    return false;
                }
            }

            return true;
        }
    }
}
