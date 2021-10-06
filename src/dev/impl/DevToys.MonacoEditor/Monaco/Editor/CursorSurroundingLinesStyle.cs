#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Controls when `cursorSurroundingLines` should be enforced
    /// Defaults to `default`, `cursorSurroundingLines` is not enforced when cursor position is
    /// changed
    /// by mouse.
    /// </summary>
    [JsonConverter(typeof(CursorSurroundingLinesStyleConverter))]
    public enum CursorSurroundingLinesStyle
    {
        All,
        Default
    }
}
