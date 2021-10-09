#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Control the side of the minimap in editor.
    /// Defaults to 'right'.
    /// </summary>
    [JsonConverter(typeof(SideConverter))]
    public enum Side
    {
        Left,
        Right
    };
}
