using Newtonsoft.Json;
using System.Text;
using Windows.UI;

namespace DevToys.MonacoEditor.Monaco.Helpers;

/// <summary>
/// Simple Proxy to general CSS Line Styles.
/// Line styles are overlayed behind text in the editor and are useful for highlighting sections of text efficiently
/// </summary>
[JsonConverter(typeof(CssStyleConverter))]
public sealed class CssLineStyle : ICssStyle
{
    [JsonIgnore]
    public Color? BackgroundColor { get; set; }

    [Obsolete("Use ForegroundColor on CssInlineStyle instead, this is an overlay.")]
    [JsonIgnore]
    public Color? ForegroundColor { get; set; }

    public uint Id { get; }

    public string Name { get; }

    public CssLineStyle()
    {
        Id = CssStyleBroker.Register(this);
        Name = "generated-style-" + Id;
    }

    public string ToCss()
    {
        var output = new StringBuilder(40);
        if (BackgroundColor.HasValue)
        {
            //// we need to use rgba function like this due to EdgeHTML, otherwise we could use #RRGGBBAA which would be easier using #{n:X2}...
            output.AppendLine(string.Format("background: rgba({0:d},{1:d},{2:d},{3:f});", BackgroundColor.Value.R,
                                                                                          BackgroundColor.Value.G,
                                                                                          BackgroundColor.Value.B,
                                                                                          BackgroundColor.Value.A / 255f));
        }
#pragma warning disable CS0618 // Type or member is obsolete
        if (ForegroundColor.HasValue)
        {
            output.AppendLine(string.Format("color: rgba({0:d},{1:d},{2:d},{3:f}) !important;", ForegroundColor.Value.R,
                                                                                                ForegroundColor.Value.G,
                                                                                                ForegroundColor.Value.B,
                                                                                                ForegroundColor.Value.A / 255f));
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return this.WrapCssClassName(output.ToString());
    }
}
