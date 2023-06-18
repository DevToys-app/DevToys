using System.Text.Json;

///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// All computed editor options.
/// </summary>
public class ComputedEditorOptions
{
    private readonly List<string> _options;

    public ComputedEditorOptions(List<string> options)
    {
        _options = options;
    }

    public T? Get<T>(EditorOption id)
    {
        return JsonSerializer.Deserialize<T>(_options[(int)id]);
    }
}
