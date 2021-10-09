#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// New model decorations.
    /// </summary>
    public sealed class ModelDeltaDecoration
    {
        [JsonProperty("options")]
        public ModelDecorationOptions Options { get; private set; }

        [JsonProperty("range")]
        public IRange Range { get; private set; }

        public ModelDeltaDecoration(IRange range, ModelDecorationOptions options)
        {
            Range = range;
            Options = options;
        }
    }
}
