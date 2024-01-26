namespace DevToys.Api;

/// <summary>
/// Defines the targeted platform for this component.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TargetPlatformAttribute : Attribute
{
    /// <summary>
    /// Gets the target platform.
    /// </summary>
    public Platform TargetPlatform { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TargetPlatformAttribute"/> class.
    /// </summary>
    /// <param name="platform">The target platform.</param>
    public TargetPlatformAttribute(Platform platform)
    {
        TargetPlatform = platform;
    }
}
