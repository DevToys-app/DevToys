///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class IEditorOption<V>
{
    public EditorOption Id { get; set; }

    public string? Name { get; set; }

    public V? DefaultValue { get; set; }
}
