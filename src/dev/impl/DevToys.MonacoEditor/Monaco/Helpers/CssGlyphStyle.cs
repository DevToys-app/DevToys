#nullable enable

using DevToys.MonacoEditor.CodeEditorControl;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    public sealed class CssGlyphStyle : ICssStyle
    {
        public System.Uri? GlyphImage { get; set; }

        public string? Name { get; private set; }

        public CssGlyphStyle(CodeEditorCore editor)
        {
            Name = CssStyleBroker.GetInstance(editor).Register(this);
        }

        public string ToCss()
        {
            return CssStyleBroker.WrapCssClassName(this, string.Format("background: url(\"{0}\");", GlyphImage?.AbsoluteUri));
        }
    }
}
