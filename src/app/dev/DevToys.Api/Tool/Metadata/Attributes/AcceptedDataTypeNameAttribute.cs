namespace DevToys.Api;

/// <summary>
/// Defines a data type accepted by the <see cref="IGuiTool"/> as an input. This information is used to identify what tool can
/// be recommended based on Smart Detection finding.
/// The value of <see cref="DataTypeName"/> should match a name defined by a <see cref="DataTypeNameAttribute.DataTypeName"/> in a <see cref="IDataTypeDetector"/>.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AcceptedDataTypeNameAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the accepted data type.
    /// </summary>
    public string DataTypeName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptedDataTypeNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the accepted data type.</param>
    public AcceptedDataTypeNameAttribute(string name)
    {
        Guard.IsNotEmpty(name);
        DataTypeName = name;
    }
}
