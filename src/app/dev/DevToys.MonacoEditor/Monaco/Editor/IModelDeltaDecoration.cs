using DevToys.MonacoEditor.JsonConverters;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// New model decorations.
/// </summary>
public sealed class IModelDeltaDecoration
{
    [JsonProperty("options")]
    public IModelDecorationOptions Options { get; private set; }

    [JsonProperty("range"), JsonConverter(typeof(InterfaceToClassConverter<IRange, Range>))]
    public IRange Range { get; private set; }

    public IModelDeltaDecoration(IRange range, IModelDecorationOptions options)
    {
        Range = range;
        Options = options;
    }
}
