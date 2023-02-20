namespace DevToys.Api;

/// <summary>
/// Represents the declaration of a tool with a GUI.
/// </summary>
/// <remarks>
/// The project containing the <see cref="System.Resources.ResourceManager"/> with all the strings for the tool should contain an implementation
/// of <see cref="IResourceManagerAssemblyIdentifier"/> as shown in the example.
/// </remarks>
/// <example>
///     <code>
///         [Export(typeof(IGuiTool))]
///         [Name("Base64 Encoder / Decoder")]
///         [Author("John Doe")]
///         [ToolDisplayInformation(
///             IconFontName = "FluentSystemIcons",
///             IconGlyph = "\u0108",
///             GroupName = "Encoders / Decoders",                                                  // <seealso cref="GuiToolGroup"/>
///             ResourceManagerAssemblyIdentifier = nameof(MyResourceManagerAssemblyIdentifier),    // <seealso cref="IResourceManagerAssemblyIdentifier"/>
///             ResourceManagerBaseName = "MyProject.Strings",
///             ShortDisplayTitleResourceName = nameof(MyProject.Strings.ShortDisplayTitle),
///             LongDisplayTitleResourceName = nameof(MyProject.Strings.LongDisplayTitle),          // Optional
///             DescriptionResourceName = nameof(MyProject.Strings.Description),                    // Optional
///             AccessibleNameResourceName = nameof(MyProject.Strings.AccessibleName),              // Optional
///             SearchKeywordsResourceName = nameof(MyProject.Strings.SearchKeywords))]             // Optional
///         [TargetPlatform(Platform.Windows)]                                                      // Optional
///         [TargetPlatform(Platform.WASM)]                                                         // Optional
///         [Order(Before = "Base64 Image Decoder")]                                                // Optional
///         [MenuPlacement(MenuPlacement.Footer)]                                                   // Optional
///         [NotSearchable]                                                                         // Optional
///         [NotFavorable]                                                                          // Optional
///         [NoCompactOverlaySupport]                                                               // Optional
///         [CompactOverlaySize(height: 200, width: 250)]                                           // Optional
///         internal sealed class Base64GuiTool : IGuiTool
///         {
///         }
///
///         [Export(typeof(IResourceManagerAssemblyIdentifier))]
///         [Name(nameof(MyResourceManagerAssemblyIdentifier))]
///         public sealed class MyResourceManagerAssemblyIdentifier : IResourceManagerAssemblyIdentifier
///         {
///         }
///     </code>
/// </example>
public interface IGuiTool
{
    /// <summary>
    /// Gets the view for the tool.
    /// </summary>
    UIElement View { get; }
}
