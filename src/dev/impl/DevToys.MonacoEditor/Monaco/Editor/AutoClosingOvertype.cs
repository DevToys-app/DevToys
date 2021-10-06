using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Options for typing over closing quotes or brackets.
    /// </summary>
    [JsonConverter(typeof(AutoClosingOvertypeConverter))]
    public enum AutoClosingOvertype
    {
        Always,
        Auto,
        Never
    }
}
