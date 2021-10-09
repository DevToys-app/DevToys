#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    [JsonConverter(typeof(LineNumbersTypeConverter))]
    public enum LineNumbersType { Interval, Off, On, Relative };
}
