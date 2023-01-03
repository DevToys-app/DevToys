namespace DevToys.Api;

/// <summary>
/// Defines the priority of this component over others.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OrderAttribute : Attribute
{
    private string _before = string.Empty;
    private string _after = string.Empty;

    /// <summary>
    /// Gets or sets the internal name of a component to compare with.
    /// The value should corresponds to an existing <see cref="NameAttribute.InternalComponentName"/> value.
    /// </summary>
    public string Before
    {
        get => _before;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _before = value;
        }
    }

    /// <summary>
    /// Gets or sets the internal name of a component to compare with.
    /// The value should corresponds to an existing <see cref="NameAttribute.InternalComponentName"/> value.
    /// </summary>
    public string After
    {
        get => _after;
        set
        {
            Guard.IsNotNullOrEmpty(value);
            _after = value;
        }
    }
}
