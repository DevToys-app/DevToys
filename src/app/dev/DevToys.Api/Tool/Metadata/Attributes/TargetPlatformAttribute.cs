namespace DevToys.Api;

/// <summary>
/// Defines the targeted platform for this component.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TargetPlatformAttribute : Attribute
{
    public Platform TargetPlatform { get; }

    public TargetPlatformAttribute(Platform platform)
    {
        TargetPlatform = platform;
    }
}
