using Newtonsoft.Json;
using System.Text;
using Windows.UI;
using Windows.UI.Text;

namespace DevToys.MonacoEditor.Monaco.Helpers;

/// <summary>
/// Inline styles modify the text style itself and are useful for manipulating the colors and styles of text to indicate conditions.
/// </summary>
[JsonConverter(typeof(CssStyleConverter))]
public sealed class CssInlineStyle : ICssStyle
{
    public TextDecoration TextDecoration { get; set; }

    public FontWeight? FontWeight { get; set; }

    public FontStyle FontStyle { get; set; }

    // TODO: Provide Cursor: https://developer.mozilla.org/en-US/docs/Web/CSS/cursor

    // Setting a background inline will override any CssLineStyle.
    public Color? BackgroundColor { get; set; }

    public Color? ForegroundColor { get; set; }

    public uint Id { get; }

    public string Name { get; }

    public CssInlineStyle()
    {
        Id = CssStyleBroker.Register(this);
        Name = "generated-style-" + Id;
    }

    public string ToCss()
    {
        var output = new StringBuilder(40);
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

        if (BackgroundColor.HasValue)
        {
            output.AppendLine(string.Format("background: rgba({0:d},{1:d},{2:d},{3:f});", BackgroundColor.Value.R,
                                                                                          BackgroundColor.Value.G,
                                                                                          BackgroundColor.Value.B,
                                                                                          BackgroundColor.Value.A / 255f));
        }

        if (ForegroundColor.HasValue)
        {
            output.AppendLine(string.Format("color: rgba({0:d},{1:d},{2:d},{3:f}) !important;", ForegroundColor.Value.R,
                                                                                                ForegroundColor.Value.G,
                                                                                                ForegroundColor.Value.B,
                                                                                                ForegroundColor.Value.A / 255f));
        }

        return this.WrapCssClassName(output.ToString());
    }
}
