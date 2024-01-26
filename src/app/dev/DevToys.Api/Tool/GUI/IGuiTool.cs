namespace DevToys.Api;

/// <summary>
/// Represents the declaration of a tool with a GUI.
/// </summary>
/// <remarks>
/// <para>
/// The project containing the <see cref="System.Resources.ResourceManager"/> with all the strings for the tool should contain an implementation
/// of <see cref="IResourceAssemblyIdentifier"/> as shown in the example.
/// </para>
/// <example>
///     <code>
///         [Export(typeof(IGuiTool))]
///         [Name("Base64 Encoder / Decoder")]
///         [ToolDisplayInformation(
///             IconFontName = "FluentSystemIcons",
///             IconGlyph = "\u0108",
///             GroupName = "Encoders / Decoders",                                                  // <seealso cref="GuiToolGroup"/>
///             ResourceManagerAssemblyIdentifier = nameof(MyResourceManagerAssemblyIdentifier),    // <seealso cref="IResourceAssemblyIdentifier"/>
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
/// </remarks>
public interface IGuiTool
{
    /// <summary>
    /// Gets the view for the tool.
    /// </summary>
    UIToolView View { get; }

    /// <summary>
    /// Invoked when the app detected a data compatible with the tool and that the user navigates to this tool in question.
    /// The expected behavior of this method is to pass the <paramref name="parsedData"/> to the <see cref="IUIElement"/>
    /// that fits the given data.
    /// </summary>
    /// <param name="dataTypeName">The data type name, as defined by <see cref="AcceptedDataTypeNameAttribute"/>.</param>
    /// <param name="parsedData">The data returned by the corresponding <see cref="IDataTypeDetector"/>.</param>
    void OnDataReceived(string dataTypeName, object? parsedData);
}
