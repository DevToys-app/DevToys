namespace DevToys.Api;

/// <summary>
/// Defines the resources to get the information about the <see cref="IGuiTool"/> to be displayed in the UI.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ToolDisplayInformationAttribute : Attribute
{
    private string _resourceManagerAssemblyIdentifier = string.Empty;
    private string _resourceManagerBaseName = string.Empty;
    private string _shortDisplayTitleResourceName = string.Empty;
    private string _longDisplayTitleResourceName = string.Empty;
    private string _descriptionResourceName = string.Empty;
    private string _accessibleNameResourceName = string.Empty;
    private string _searchKeywordsResourceName = string.Empty;
    private char _iconGlyph;
    private string _iconFontName = string.Empty;
    private string _groupName = string.Empty;

    /// <summary>
    /// Indicates the group in which the tool should appear.
    /// The name should corresponds to an existing <see cref="NameAttribute.InternalComponentName"/> value from an exported
    /// <see cref="GuiToolGroup"/>.
    /// </summary>
    public string GroupName
    {
        get => _groupName;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _groupName = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the <see cref="IResourceAssemblyIdentifier"/> that contains the type indicated by
    /// <see cref="ResourceManagerBaseName"/>.
    /// </summary>
    public string ResourceManagerAssemblyIdentifier
    {
        get => _resourceManagerAssemblyIdentifier;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _resourceManagerAssemblyIdentifier = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the resource manager's base name to use when looking for resource string
    /// for <see cref="ShortDisplayTitleResourceName"/>, <see cref="LongDisplayTitleResourceName"/>,
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
    /// Gets or sets the short title of the tool in the main menu of the app, for example "JSON".
    /// </summary>
    public string ShortDisplayTitleResourceName
    {
        get => _shortDisplayTitleResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _shortDisplayTitleResourceName = value;
        }
    }

    /// <summary>
    /// Gets or sets the long title of the tool that will be displayed in the search bar or at the top of the current tool.
    /// Sometimes it is needed to have a different one than the name showed in the menu to increase
    /// result accuracy. For example, while <see cref="ShortDisplayTitleResourceName"/> could be "JSON"
    /// for a tool that is under the Formatter category, <see cref="LongDisplayTitleResourceName"/>
    /// could be "JSON Formatter", which can be helpful to differentiate from other similar
    /// tools like "JSON Converter".
    /// </summary>
    public string LongDisplayTitleResourceName
    {
        get => _longDisplayTitleResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _longDisplayTitleResourceName = value;
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
        get => _searchKeywordsResourceName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _searchKeywordsResourceName = value;
        }
    }

    /// <summary>
    /// Gets or sets a glyph for the icon of the tool.
    /// </summary>
    public char IconGlyph
    {
        get => _iconGlyph;
        set
        {
            _iconGlyph = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the font to use to display the <see cref="IconGlyph"/>.
    /// </summary>
    public string IconFontName
    {
        get => _iconFontName;
        set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            _iconFontName = value;
        }
    }
}
