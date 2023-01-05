using System.Collections.Generic;
using System.IO;
using Nuke.Common.IO;
using Serilog;

internal static class AppVersionTask
{
    internal static void SetAppVersion(AbsolutePath rootDirectory)
    {
        string appVersion = GetAppVersion(rootDirectory);

        var csharpUpdater = new CSharpUpdater(appVersion);
        IReadOnlyCollection<AbsolutePath> assemblyVersionFiles
            = rootDirectory.GlobFiles("**/*AssemblyVersion.cs");
        foreach (AbsolutePath file in assemblyVersionFiles)
        {
            Log.Information("Updating app version in {File}...", file);
            csharpUpdater.UpdateFile(file);
        }

        var appxManifestUpdater = new AppxManifestUpdater(appVersion);
        IReadOnlyCollection<AbsolutePath> appxmanifestFiles
            = rootDirectory.GlobFiles("**/*.appxmanifest");
        foreach (AbsolutePath file in appxmanifestFiles)
        {
            Log.Information("Updating app version in {File}...", file);
            appxManifestUpdater.UpdateFile(file);
        }
    }

    private static string GetAppVersion(AbsolutePath rootDirectory)
    {
        AbsolutePath appVersionNumberFile = rootDirectory / "tools" / "app-version-number.txt";
        if (!appVersionNumberFile.FileExists())
        {
            Log.Error("Unable to find the app version number in {AppVersionNumberFile}...", appVersionNumberFile);
            throw new FileNotFoundException("Unable to find the app version number file.", appVersionNumberFile.ToString());
        }

        return File.ReadAllText(appVersionNumberFile);
    }
}
