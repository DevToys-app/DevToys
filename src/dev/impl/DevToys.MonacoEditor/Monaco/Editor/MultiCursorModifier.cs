#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// The modifier to be used to add multiple cursors with the mouse.
    /// Defaults to 'alt'
    /// </summary>
    [JsonConverter(typeof(MultiCursorModifierConverter))]
    public enum MultiCursorModifier
    {
        Alt,
        CtrlCmd
    }
}
