#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    public interface IEditorConstructionOptions : IEditorOptions
    {
        [JsonProperty("dimension", NullValueHandling = NullValueHandling.Ignore)]
        IDimension Dimension { get; set; }
    }
}
