#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Enable rendering of whitespace.
    /// Defaults to none.
    /// </summary>
    [JsonConverter(typeof(RenderWhitespaceConverter))]
    public enum RenderWhitespace
    {
        All,
        Boundary,
        None,
        Selection
    }
}
