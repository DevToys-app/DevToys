#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    [JsonConverter(typeof(MultipleConverter))]
    public enum Multiple
    {
        Goto,
        GotoAndPeek,
        Peek
    }
}
