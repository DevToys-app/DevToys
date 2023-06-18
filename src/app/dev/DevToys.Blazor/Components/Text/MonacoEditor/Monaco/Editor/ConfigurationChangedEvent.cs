///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing that the configuration of the editor has changed.
/// </summary>
public class ConfigurationChangedEvent
{
    private readonly List<bool>? _options;

    public ConfigurationChangedEvent(List<bool>? options)
    {
        _options = options;
    }

    public bool HasChanged(EditorOption id)
    {
        return _options is not null && _options[(int)id];
    }
}
