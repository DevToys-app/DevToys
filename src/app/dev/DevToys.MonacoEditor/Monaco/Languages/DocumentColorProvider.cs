using DevToys.MonacoEditor.Monaco.Editor;

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// A provider of colors for editor models.
/// <seealso href="https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.documentcolorprovider.html">monaco.languages.DocumentColorProvider</seealso>
/// </summary>
public interface DocumentColorProvider
{
    /// <summary>
    /// Provide the string representations for a color.
    /// </summary>
    Task<IEnumerable<ColorPresentation>> ProvideColorPresentationsAsync(IModel model, ColorInformation colorInfo);

    /// <summary>
    /// Provides the color ranges for a specific model.
    /// </summary>
    Task<IEnumerable<ColorInformation>> ProvideDocumentColorsAsync(IModel model);
}
