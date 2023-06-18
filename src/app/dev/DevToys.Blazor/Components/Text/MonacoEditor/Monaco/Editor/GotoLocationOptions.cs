///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for go to location
/// </summary>
public class GotoLocationOptions
{
    public string? Multiple { get; set; }

    public string? MultipleDefinitions { get; set; }

    public string? MultipleTypeDefinitions { get; set; }

    public string? MultipleDeclarations { get; set; }

    public string? MultipleImplementations { get; set; }

    public string? MultipleReferences { get; set; }

    public string? AlternativeDefinitionCommand { get; set; }

    public string? AlternativeTypeDefinitionCommand { get; set; }

    public string? AlternativeDeclarationCommand { get; set; }

    public string? AlternativeImplementationCommand { get; set; }

    public string? AlternativeReferenceCommand { get; set; }
}
