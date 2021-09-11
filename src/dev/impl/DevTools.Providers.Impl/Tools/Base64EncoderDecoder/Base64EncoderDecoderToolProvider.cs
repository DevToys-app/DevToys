#nullable enable

using DevTools.Core.Threading;
using DevTools.Common;
using System;
using System.Composition;
using System.Text.RegularExpressions;
using System.Text;

namespace DevTools.Providers.Impl.Tools.Base64EncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Base64 Encoder/Decoder")]
    [ProtocolName("base64")]
    [Order(0)]
    internal sealed class Base64EncoderDecoderToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.Base64EncoderDecoder.DisplayName;

        public object IconSource { get; }

        [ImportingConstructor]
        public Base64EncoderDecoderToolProvider(IThread thread)
            : base(thread)
        {
            IconSource = CreatePathIconFromPath(nameof(Base64EncoderDecoderToolProvider));
        }

        public bool CanBeTreatedByTool(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            var trimmedData = data.Trim();
            return IsBase64DataStrict(trimmedData);
        }

        public IToolViewModel CreateTool()
        {
            throw new NotImplementedException();
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

            var equalIndex = data.IndexOf('=');
            var length = data.Length;

            if (!(equalIndex == -1 || equalIndex == length - 1 || (equalIndex == length - 2 && data[length - 1] == '=')))
            {
                return false;
            }

            string? decoded;

            try
            {
                byte[] decodedData = Convert.FromBase64String(data);
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
                if (current == 65533) return false;
                if (!(current == 0x9 
                    || current == 0xA 
                    || current == 0xD 
                    || (current >= 0x20 && current <= 0xD7FF) 
                    || (current >= 0xE000 && current <= 0xFFFD) 
                    || (current >= 0x10000 && current <= 0x10FFFF)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
