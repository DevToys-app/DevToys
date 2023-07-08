namespace DevToys.Api;

/// <summary>
/// Represents the factory to access some resources of the current assembly such as strings stored in RESX files, or Fonts.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(MyResourceAssemblyIdentifier))]
///         [Name(nameof(MyResourceAssemblyIdentifier))]
///         internal sealed class MyResourceAssemblyIdentifier : IResourceAssemblyIdentifier
///         {
///         }
///     </code>
/// </example>
/// </remarks>
public interface IResourceAssemblyIdentifier
{
    /// <summary>
    /// Get access to fonts that can be used by <see cref="IGuiTool"/>.
    /// </summary>
    /// <remarks>
    /// The font is expected to be a TTF or OTF. WOFF and WOFF2 aren't supported at the moment.
    /// </remarks>
    /// <returns>An array of font definition with a stream to access it.</returns>
    ValueTask<FontDefinition[]> GetFontDefinitionsAsync();
}
