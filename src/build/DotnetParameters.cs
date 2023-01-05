using Nuke.Common.IO;

internal sealed record DotnetParameters
{
    internal readonly AbsolutePath ProjectOrSolutionPath;

    internal readonly string RuntimeIdentifier;

    internal readonly string TargetFramework;

    internal readonly bool Portable;

    internal readonly string OutputPath;

    public DotnetParameters(AbsolutePath projectOrSolutionPath, string runtimeIdentifier, string targetFramework, bool portable)
    {
        ProjectOrSolutionPath = projectOrSolutionPath;
        RuntimeIdentifier = runtimeIdentifier;
        TargetFramework = targetFramework;
        Portable = portable;
        OutputPath = $"{ProjectOrSolutionPath.Name}-{TargetFramework}-{RuntimeIdentifier}{(portable ? "-portable" : string.Empty)}";
    }
}
