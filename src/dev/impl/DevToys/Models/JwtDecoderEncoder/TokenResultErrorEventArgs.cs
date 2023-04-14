#nullable enable

using System;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.Models.JwtDecoderEncoder
{
    public class TokenResultErrorEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;

        public InfoBarSeverity Severity { get; set; }
    }
}
