using System.Reflection;

namespace DevToys.Linux.Components;

internal record BlazorWebViewOptions
{
    internal required Type RootComponent { get; init; }

    internal string HostPath { get; init; } = Path.Combine("wwwroot", "index.html");

    internal string ContentRoot { get; init; } = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;

    internal string RelativeHostPath => Path.GetRelativePath(ContentRoot, HostPath);
}
