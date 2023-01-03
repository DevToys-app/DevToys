using DevToys.MonacoEditor.Extensions;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Helpers;

[JsonConverter(typeof(CssStyleConverter))]
public sealed class CssGlyphStyle : ICssStyle
{
    [JsonIgnore]
    public System.Uri? GlyphImage { get; set; }

    public uint Id { get; }

    public string Name { get; }

    public CssGlyphStyle()
    {
        Id = CssStyleBroker.Register(this);
        Name = "generated-style-" + Id;
    }

    public string ToCss()
    {
        return this.WrapCssClassName(string.Format("background: url(\"{0}\");", GlyphImage?.AbsoluteUriString() ?? string.Empty));
    }
}
