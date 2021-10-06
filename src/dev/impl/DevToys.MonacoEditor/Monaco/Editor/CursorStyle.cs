#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Control the cursor style, either 'block' or 'line'.
    /// Defaults to 'line'.
    /// </summary>
    [JsonConverter(typeof(CursorStyleConverter))]
    public enum CursorStyle
    {
        Block,
        BlockOutline,
        Line,
        LineThin,
        Underline,
        UnderlineThin
    }
}
