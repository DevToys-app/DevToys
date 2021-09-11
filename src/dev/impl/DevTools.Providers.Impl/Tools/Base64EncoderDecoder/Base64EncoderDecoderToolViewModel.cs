#nullable enable

using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.Base64EncoderDecoder
{
    [Export(typeof(Base64EncoderDecoderToolViewModel))]
    public class Base64EncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        public Type View { get; } = typeof(Base64EncoderDecoderToolPage);
    }
}
