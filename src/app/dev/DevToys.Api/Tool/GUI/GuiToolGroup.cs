namespace DevToys.Api;

/// <summary>
/// Represents a group or category in the main menu of the app.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(GuiToolGroup))]
///         [Name("Encoders / Decoders")]
///         [Order(After = "Converters")] // Optional
///         internal sealed class MyGroup : GuiToolGroup
///         {
///             [ImportingConstructor]
///             internal MyGroup()
///             {
///                 IconFontName = "FluentSystemIcons";
///                 IconGlyph = "\u0108";
///                 DisplayTitle = Sample.CommandDescription;
///                 AccessibleName = Sample.CommandDescription;
///             }
///         }
///     </code>
/// </example>
/// </remarks>
[DebuggerDisplay($"DisplayTitle = {{{nameof(DisplayTitle)}}}")]
public class GuiToolGroup
{
    /// <summary>
    /// Gets or sets the name of the font to use to display the <see cref="IconGlyph"/>.
    /// </summary>
    public virtual string IconFontName { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets a glyph for the icon of the group.
    /// </summary>
    public virtual char IconGlyph { get; protected set; }

    /// <summary>
    /// Gets or sets the title to display in the menu.
    /// </summary>
    public virtual string DisplayTitle { get; protected set; } = string.Empty;

    /// <summary>
    /// (optional) Gets or sets the name of the group that will be told to the user when using screen reader.
    /// </summary>
    public virtual string AccessibleName { get; protected set; } = string.Empty;
}
