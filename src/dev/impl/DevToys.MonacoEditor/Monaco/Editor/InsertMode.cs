#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Overwrite word ends on accept. Default to false.
    /// </summary>
    [JsonConverter(typeof(InsertModeConverter))]
    public enum InsertMode
    {
        Insert,
        Replace
    }
}
