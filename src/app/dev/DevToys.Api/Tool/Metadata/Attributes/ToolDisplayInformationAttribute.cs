namespace DevToys.Api;

/// <summary>
/// Defines the resources to get the information about the <see cref="IGuiTool"/> to be displayed in the UI.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ToolDisplayInformationAttribute : Attribute
{
    private string _resourceManagerBaseName = string.Empty;
    private string _menuDisplayTitleResourceName = string.Empty;
    private string _searchDisplayTitleResourceName = string.Empty;
    private string _descriptionResourceName = string.Empty;
    private string _accessibleNameResourceName = string.Empty;
    private string _searchkeywordsResourceName = string.Empty;
    private string _iconGlyph = string.Empty;

    /// <summary>
    /// Gets or sets the name of the resource manager's base name to use when looking for resource string
    /// for <see cref="MenuDisplayTitleResourceName"/>, <see cref="SearchDisplayTitleResourceName"/>,
    /// <see cref="DescriptionResourceName"/>, <see cref="AccessibleNameResourceName"/> and <see cref="SearchKeywordsResourceName"/>.
    /// </summary>
    public string ResourceManagerBaseName
    {
        get => _resourceManagerBaseName;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _resourceManagerBaseName = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the tool in the main menu of the app, for example "JSON".
    /// </summary>
    public string MenuDisplayTitleResourceName
    {
        get => _menuDisplayTitleResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _menuDisplayTitleResourceName = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the tool that will be displayed in the search bar. Sometimes
    /// it is needed to have a different one than the name showed in the menu to increase
    /// result accuracy. For example, while <see cref="MenuDisplayTitleResourceName"/> could be "JSON"
    /// for a tool that is under the Formatter category, <see cref="SearchDisplayTitleResourceName"/>
    /// could be "JSON Formatter", which can be helpful to differentiate from other similar
    /// tools like "JSON Converter".
    /// </summary>
    public string SearchDisplayTitleResourceName
    {
        get => _searchDisplayTitleResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _searchDisplayTitleResourceName = value;
        }
    }

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    public string DescriptionResourceName
    {
        get => _descriptionResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _descriptionResourceName = value;
        }
    }

    /// <summary>
    /// (optional) Gets or sets the name of the tool that will be told to the user when using screen reader.
    /// </summary>
    public string AccessibleNameResourceName
    {
        get => _accessibleNameResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _accessibleNameResourceName = value;
        }
    }

    /// <summary>
    /// (optional) Gets or sets the keywords of the tool that are searched in the localized environment.
    /// </summary>
    public string SearchKeywordsResourceName
    {
        get => _searchkeywordsResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _searchkeywordsResourceName = value;
        }
    }

    /// <summary>
    /// Gets or sets a glyph for the icon of the tool.
    /// </summary>
    public string IconGlyph
    {
        get => _iconGlyph;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _iconGlyph = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the font to use to display the <see cref="IconGlyph"/>.
    /// </summary>
    public string IconFontName
    {
        get => _iconGlyph;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _iconGlyph = value;
        }
    }
}
