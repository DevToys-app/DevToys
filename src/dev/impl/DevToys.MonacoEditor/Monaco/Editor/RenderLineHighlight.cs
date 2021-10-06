#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Enable rendering of current line highlight.
    /// Defaults to all.
    /// </summary>
    [JsonConverter(typeof(RenderLineHighlightConverter))]
    public enum RenderLineHighlight
    {
        All,
        Gutter,
        Line,
        None
    }
}
