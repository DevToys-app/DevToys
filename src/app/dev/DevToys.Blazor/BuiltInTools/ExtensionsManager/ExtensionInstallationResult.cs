using NuGet.Packaging;

namespace DevToys.Blazor.BuiltInTools.ExtensionsManager;

internal class ExtensionInstallationResult
{
    internal bool HasSucceeded => string.IsNullOrWhiteSpace(ErrorMessage);
    internal bool AlreadyInstalled { get; }
    internal NuspecReader NuspecReader { get; }
    internal string ExtensionInstallationPath { get; } = string.Empty;
    internal string ErrorMessage { get; } = string.Empty;

    internal ExtensionInstallationResult(NuspecReader nuspecReader, string errorMessage)
    {
        ErrorMessage = errorMessage;
        NuspecReader = nuspecReader;
    }

    internal ExtensionInstallationResult(bool alreadyInstalled, NuspecReader nuspecReader, string extensionInstallationPath)
    {
        AlreadyInstalled = alreadyInstalled;
        NuspecReader = nuspecReader;
        ExtensionInstallationPath = extensionInstallationPath;
    }
}

