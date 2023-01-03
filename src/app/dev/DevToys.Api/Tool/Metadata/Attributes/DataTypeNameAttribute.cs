namespace DevToys.Api;

/// <summary>
/// Defines a data type name attached to a <see cref="IDataTypeDetector"/>.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DataTypeNameAttribute : Attribute
{
    public string DataTypeName { get; }

    public string? DataTypeBaseName { get; }

    public DataTypeNameAttribute(string name, string? baseName = null)
    {
        Guard.IsNotEmpty(name);
        DataTypeName = name;
        DataTypeBaseName = baseName;
    }
}
