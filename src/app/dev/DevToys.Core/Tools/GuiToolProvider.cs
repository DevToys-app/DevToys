using DevToys.Api;

namespace DevToys.Core.Tools;

/// <summary>
/// Provides information about tools in the app.
/// </summary>
[Export(typeof(GuiToolProvider))]
public sealed class GuiToolProvider
{
    [ImportingConstructor]
    public GuiToolProvider(
        [ImportMany] IEnumerable<Lazy<IGuiTool, GuiToolMetadata>> guiTools)
    {

    }

    /// <summary>
    /// Gets a flat list containing all the tools available.
    /// </summary>
    internal IEnumerable<string> GetAllTools()
    {
        return null;
    }
}
