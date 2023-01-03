namespace DevToys.Api;

/// <summary>
/// Defines the name of the CLI command of a <see cref="ICommandLineTool"/>.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandNameAttribute : Attribute
{
    private string _name = string.Empty;
    private string _alias = string.Empty;
    private string _descriptionResourceName = string.Empty;
    private string _resourceManagerBaseName = string.Empty;

    /// <summary>
    /// Gets the name of the command. Example, "file".
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the alias name of the option. Example, "f".
    /// </summary>
    public string Alias
    {
        get => _alias;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _alias = value;
        }
    }

    /// <summary>
    /// Gets or sets name of the localized resource that provides a description.
    /// </summary>
    public string DescriptionResourceName
    {
        get => _descriptionResourceName;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _descriptionResourceName = value;
        }
    }

    /// <summary>
    /// Gets or sets the name of the resource manager's base name to use when looking for <see cref="DescriptionResourceName"/>.
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
}
