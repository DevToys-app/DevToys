using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

public sealed class EditorFindOptions
{
    [JsonProperty("addExtraSpaceOnTop", NullValueHandling = NullValueHandling.Ignore)]
    public bool? AddExtraSpaceOnTop { get; set; }

    /// <summary>
    /// Controls if Find in Selection flag is turned on in the editor.
    /// </summary>
    [JsonProperty("autoFindInSelection", NullValueHandling = NullValueHandling.Ignore)]
    public AutoFindInSelection? AutoFindInSelection { get; set; }

    /// <summary>
    /// Controls if we seed search string in the Find Widget with editor selection.
    /// </summary>
    [JsonProperty("seedSearchStringFromSelection", NullValueHandling = NullValueHandling.Ignore)]
    public bool? SeedSearchStringFromSelection { get; set; }
}
