#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Control the mouse pointer style, either 'text' or 'default' or 'copy'
    /// Defaults to 'text'
    /// </summary>
    [JsonConverter(typeof(MouseStyleConverter))]
    public enum MouseStyle
    {
        Copy,
        Default,
        Text
    }
}
