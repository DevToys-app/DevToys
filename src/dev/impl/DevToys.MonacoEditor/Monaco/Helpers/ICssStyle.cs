#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    [JsonConverter(typeof(CssStyleConverter))]
    public interface ICssStyle
    {
        string? Name { get; }

        string ToCss();
    }
}
