#nullable enable

using System;

namespace DevToys.UI.Controls.TextEditor
{
    [Flags]
    public enum RtfFormat
    {
        Normal = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }
}
