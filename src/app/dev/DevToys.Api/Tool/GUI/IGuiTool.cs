namespace DevToys.Api;

/// <summary>
/// Represents the factory for tool with a GUI.
/// </summary>
/// <example>
///     <code>
///         [Export(typeof(IGuiTool))]
///         [Name("Base64 Encode / Decoder")]
///         [Author("John Doe")]
///         [ToolDisplayInformation(
///             IconFontName = "Fluent System-Regular",
///             IconGlyph = "\u0108",
///             ResourceManagerBaseName = "MyProject.Strings",
///             MenuDisplayTitleResourceName = nameof(Strings.MenuDisplayTitle),
///             SearchDisplayTitleResourceName = nameof(Strings.SearchDisplayTitle),
///             DescriptionResourceName = nameof(Strings.Description),
///             AccessibleNameResourceName = nameof(Strings.AccessibleName),
///             SearchKeywordsResourceName = nameof(Strings.SearchKeywords))]
///         [TargetPlatform(Platform.Windows)]            // Optional
///         [TargetPlatform(Platform.WASM)]               // Optional
///         [Parent("Encoders / Decoders")]               // Optional
///         [Order(Before = "Base64 Image Decoder")]      // Optional
///         [NonSearchable]                               // Optional
///         [NonFavorable]                                // Optional
///         [NoCompactOverlaySupport]                     // Optional
///         [MenuPlacement(MenuPlacement.Footer)]         // Optional
///         [CompactOverlaySize(height: 200, width: 250)] // Optional
///         internal sealed class Base64GuiTool : IGuiTool
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
