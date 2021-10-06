#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configure the behaviour when pasting a text with the line count equal to the cursor
    /// count.
    /// Defaults to 'spread'.
    /// </summary>
    [JsonConverter(typeof(MultiCursorPasteConverter))]
    public enum MultiCursorPaste
    { 
        Full,
        Spread
    }
}
