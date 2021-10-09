#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Accept suggestions on ENTER.
    /// Defaults to 'on'.
    /// </summary>
    [JsonConverter(typeof(AcceptSuggestionOnEnterConverter))]
    public enum AcceptSuggestionOnEnter
    {
        Off,
        On,
        Smart
    }
}
