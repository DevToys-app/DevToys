#nullable enable

using DevTools.Localization;
using System;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.Base64EncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("Base64 Encoder/Decoder")]
    [Order(0)]
    internal sealed class Base64EncoderDecoderToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.Base64EncoderDecoder.DisplayName;

        public object IconSource { get; }
            = CreatePathIconFromPath(
                "M 0.008 0.008 L 0.008 31.992 L 31.992 31.992 L 31.992 0.008 Z M 2.916 2.916 L 29.084 2.916 L 29.084 29.084 L 2.916 29.084 Z M 11.957 10.094 L 8.777 13.774 C 7.823 14.87 7.276 16.273 7.276 17.727 L 7.276 18.18 C 7.276 20.169 8.924 21.816 10.912 21.816 L 12.366 21.816 C 14.353 21.816 16 20.169 16 18.18 C 16 16.193 14.353 14.546 12.366 14.546 L 11.957 14.546 L 14.137 12.002 Z M 21.816 10.184 L 21.816 14.546 L 20.362 14.546 L 20.362 10.231 L 17.454 10.231 L 17.454 17.454 L 21.816 17.454 L 21.816 21.816 L 24.722 21.816 L 24.722 10.184 Z M 10.276 17.454 L 12.366 17.454 C 12.781 17.454 13.092 17.766 13.092 18.18 C 13.092 18.595 12.781 18.908 12.366 18.908 L 10.912 18.908 C 10.497 18.908 10.184 18.595 10.184 18.18 L 10.184 17.727 C 10.184 17.629 10.264 17.55 10.276 17.454 Z");

        public bool CanBeTreatedByTool(string data)
        {
            throw new NotImplementedException();
        }

        public IToolViewModel CreateTool()
        {
            throw new NotImplementedException();
        }
    }
}
