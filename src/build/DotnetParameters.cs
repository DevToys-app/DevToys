using Nuke.Common.IO;

internal sealed record DotnetParameters
{
    internal readonly AbsolutePath ProjectOrSolutionPath;

    internal readonly string RuntimeIdentifier;

    internal readonly string TargetFramework;

    internal readonly bool Portable;

    internal readonly string? Platform;

    internal readonly string OutputPath;

    public DotnetParameters(AbsolutePath projectOrSolutionPath, string runtimeIdentifier, string targetFramework, bool portable, string? platform = null)
    {
        ProjectOrSolutionPath = projectOrSolutionPath;
        RuntimeIdentifier = runtimeIdentifier;
        TargetFramework = targetFramework;
        Portable = portable;
        Platform = platform;
        OutputPath = $"{ProjectOrSolutionPath.Name}-{TargetFramework}-{RuntimeIdentifier}{(portable ? "-portable" : string.Empty)}";
    }
}
