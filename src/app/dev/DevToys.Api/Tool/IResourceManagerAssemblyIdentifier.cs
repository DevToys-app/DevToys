namespace DevToys.Api;

/// <summary>
/// Represents the factory for command line tool.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(IResourceManagerAssemblyIdentifier))]
///         [Name(nameof(MyResourceManagerAssemblyIdentifier))]
///         internal sealed class MyResourceManagerAssemblyIdentifier : IResourceManagerAssemblyIdentifier
///         {
///         }
///     </code>
/// </example>
/// </remarks>
public interface IResourceManagerAssemblyIdentifier
{
}
