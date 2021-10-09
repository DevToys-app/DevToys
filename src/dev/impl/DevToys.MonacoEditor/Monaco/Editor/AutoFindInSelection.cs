#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Controls if Find in Selection flag is turned on in the editor.
    /// </summary>
    [JsonConverter(typeof(AutoFindInSelectionConverter))]
    public enum AutoFindInSelection
    {
        Always,
        Multiline,
        Never
    }
}
