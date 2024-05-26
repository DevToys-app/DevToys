using NuGet.Packaging;

namespace DevToys.Blazor.BuiltInTools.ExtensionsManager;

internal record ExtensionInstallationResult(bool AlreadyInstalled, NuspecReader NuspecReader, string ExtensionInstallationPath)
{
}
