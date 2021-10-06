#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Render horizontal or vertical scrollbar.
    /// Defaults to 'auto'.
    /// </summary>
    [JsonConverter(typeof(ScrollbarBehaviorConverter))]
    public enum ScrollbarBehavior
    {
        Auto,
        Hidden,
        Visible
    }
}
