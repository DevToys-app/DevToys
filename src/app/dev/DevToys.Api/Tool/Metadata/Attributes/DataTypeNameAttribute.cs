namespace DevToys.Api;

/// <summary>
/// Defines a data type name attached to a <see cref="IDataTypeDetector"/>.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DataTypeNameAttribute : Attribute
{
    /// <summary>
    /// Gets the data type name.
    /// </summary>
    public string DataTypeName { get; }

    /// <summary>
    /// Gets the data type base name.
    /// </summary>
    public string? DataTypeBaseName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The data type name.</param>
    /// <param name="baseName">The data type base name.</param>
    public DataTypeNameAttribute(string name, string? baseName = null)
    {
        Guard.IsNotEmpty(name);
        DataTypeName = name;
        DataTypeBaseName = baseName;
    }
}
