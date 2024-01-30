namespace DevToys.Api;

/// <summary>
/// Defines an option for a <see cref="ICommandLineTool"/>.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class CommandLineOptionAttribute : CommandNameAttribute
{
    /// <summary>
    /// Gets the name of the option. Example, "file".
    /// Implicitly, an option named "file" will be usable in a command line through "--file &lt;value&gt;".
    /// </summary>
    public new string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    /// <summary>
    /// Gets or sets the alias name of the option. Example, "f".
    /// Implicitly, an option named "f" will be usable in a command line through "-f &lt;value&gt;".
    /// </summary>
    public new string Alias
    {
        get => base.Alias;
        set => base.Alias = value;
    }

    /// <summary>
    /// Gets or sets whether the option is required.
    /// </summary>
    public bool IsRequired { get; set; }
}
