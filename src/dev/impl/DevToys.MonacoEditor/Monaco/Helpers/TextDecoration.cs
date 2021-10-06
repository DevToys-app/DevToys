#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    [JsonConverter(typeof(TextDecorationConverter))]
    public enum TextDecoration
    {
        None,
        Underline,
        Overline,
        LineThrough,
        Initial,
        Inherit
    }
}
