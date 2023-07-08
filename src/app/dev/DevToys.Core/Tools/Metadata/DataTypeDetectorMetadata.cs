namespace DevToys.Core.Tools.Metadata;

[DebuggerDisplay($"DataTypeName = {{{nameof(DataTypeName)}}}, DataTypeBaseName = {{{nameof(DataTypeBaseName)}}}")]
public sealed class DataTypeDetectorMetadata
{
    public DataTypeDetectorMetadata(IDictionary<string, object> metadata)
    {
        DataTypeName = metadata.GetValueOrDefault(nameof(DataTypeNameAttribute.DataTypeName)) as string ?? string.Empty;
        DataTypeBaseName = metadata.GetValueOrDefault(nameof(DataTypeNameAttribute.DataTypeBaseName)) as string ?? string.Empty;
        TargetPlatforms = metadata.GetValueOrDefault(nameof(TargetPlatformAttribute.TargetPlatform)) as IReadOnlyList<Platform> ?? Array.Empty<Platform>();
        Guard.IsNotNullOrWhiteSpace(DataTypeName);
    }

    public string DataTypeName { get; }

    public string DataTypeBaseName { get; }

    public IReadOnlyList<Platform> TargetPlatforms { get; }
}
