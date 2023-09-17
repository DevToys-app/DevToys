namespace DevToys.Linux.Components;

internal record BlazorWebViewOptions
{
    internal required Type RootComponent { get; init; }

    internal string HostPath { get; init; } = Path.Combine("wwwroot", "index.html");

    internal string ContentRoot => Path.GetDirectoryName(Path.GetFullPath(HostPath))!;

    internal string RelativeHostPath => Path.GetRelativePath(ContentRoot, HostPath);
}
