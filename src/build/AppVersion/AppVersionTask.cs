using System.Collections.Generic;
using System.IO;
using Nuke.Common.IO;
using Serilog;

internal static class AppVersionTask
{
    internal static void SetAppVersion(AbsolutePath rootDirectory)
    {
        string[] appVersionNumberAppContent = GetAppVersionNumberFileContent(rootDirectory);
        string appVersion = GetAppVersion(appVersionNumberAppContent);
        string sdkVersion = GetSdkVersion(appVersionNumberAppContent);

        var csharpUpdater = new CSharpUpdater(appVersion, sdkVersion);
        IReadOnlyCollection<AbsolutePath> assemblyVersionFiles
            = rootDirectory.GlobFiles("**/*AssemblyVersion.cs");
        foreach (AbsolutePath file in assemblyVersionFiles)
        {
            Log.Information("Updating app version in {File}...", file);
            csharpUpdater.UpdateFile(file);
        }

        var projectUpdater = new ProjectUpdater(sdkVersion);
        IReadOnlyCollection<AbsolutePath> projectFiles
            = rootDirectory.GlobFiles("**/*DevToys.Api.csproj");
        foreach (AbsolutePath file in projectFiles)
        {
            Log.Information("Updating project version in {File}...", file);
            projectUpdater.UpdateFile(file);
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

    private static string[] GetAppVersionNumberFileContent(AbsolutePath rootDirectory)
    {
        AbsolutePath appVersionNumberFile = rootDirectory / "tools" / "app-version-number.txt";
        if (!appVersionNumberFile.FileExists())
        {
            Log.Error("Unable to find the app version number in {AppVersionNumberFile}...", appVersionNumberFile);
            throw new FileNotFoundException("Unable to find the app version number file.", appVersionNumberFile.ToString());
        }

        return File.ReadAllLines(appVersionNumberFile);
    }

    private static string GetAppVersion(string[] appVersionNumberAppContent)
    {
        return GetVersion(appVersionNumberAppContent, "app");
    }

    private static string GetSdkVersion(string[] appVersionNumberAppContent)
    {
        return GetVersion(appVersionNumberAppContent, "sdk");
    }

    private static string GetVersion(string[] appVersionNumberAppContent, string name)
    {
        for (int i = 0; i < appVersionNumberAppContent.Length; i++)
        {
            if (appVersionNumberAppContent[i].StartsWith(name + ":"))
            {
                return appVersionNumberAppContent[i].Substring(name.Length + 1);
            }
        }

        return string.Empty;
    }
}
