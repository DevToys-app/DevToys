#nullable enable

using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.HashGenerator
{
    [Export(typeof(HashGeneratorToolViewModel))]
    public sealed class HashGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        public Type View { get; } = typeof(HashGeneratorToolPage);
    }
}
