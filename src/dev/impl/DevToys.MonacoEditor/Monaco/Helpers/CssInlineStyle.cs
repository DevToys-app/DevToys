#nullable enable

using System.Text;
using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    /// <summary>
    /// Inline styles modify the text style itself and are useful for manipulating the colors and styles of text to indicate conditions.
    /// </summary>
    public sealed class CssInlineStyle : ICssStyle
    {
        public TextDecoration TextDecoration { get; set; }
        public FontWeight? FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }

        // TODO: Provide Cursor: https://developer.mozilla.org/en-US/docs/Web/CSS/cursor

        // Setting a background inline will override any CssLineStyle.
        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }

        public string Name { get; private set; }

        public CssInlineStyle(CodeEditor editor)
        {
            Name = CssStyleBroker.GetInstance(editor).Register(this);
        }

        public string ToCss()
        {
            StringBuilder output = new StringBuilder(40);
            if (TextDecoration != TextDecoration.None)
            {
                string text = TextDecoration.ToString().ToLower();
                if (TextDecoration == TextDecoration.LineThrough)
                {
                    text = "line-through";
                }

                output.AppendLine(string.Format("text-decoration: {0};", text));
            }

            if (FontWeight != null && FontWeight.HasValue)
            {
                output.AppendLine(string.Format("font-weight: {0};", FontWeight.Value.Weight));
            }

            if (FontStyle != FontStyle.Normal)
            {
                output.AppendLine(string.Format("font-style: {0};", FontStyle.ToString().ToLower()));
            }

            if (BackgroundColor != null)
            {
                output.AppendLine(string.Format("background: #{0:X2}{1:X2}{2:X2};", BackgroundColor.Color.R,
                                                                                    BackgroundColor.Color.G,
                                                                                    BackgroundColor.Color.B));
            }

            if (ForegroundColor != null)
            {
                output.AppendLine(string.Format("color: #{0:X2}{1:X2}{2:X2} !important;", ForegroundColor.Color.R,
                                                                               ForegroundColor.Color.G,
                                                                               ForegroundColor.Color.B));
            }

            return CssStyleBroker.WrapCssClassName(this, output.ToString());
        }
    }
}
