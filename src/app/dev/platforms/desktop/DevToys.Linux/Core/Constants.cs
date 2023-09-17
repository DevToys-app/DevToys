﻿namespace DevToys.Linux.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = GetAppCacheDirectory();

    internal static string PluginInstallationFolder => Path.Combine(AppCacheDirectory, "Plugins");

    private static string GetAppCacheDirectory()
    {
        string? applicationDataRootFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrEmpty(applicationDataRootFolder))
        {
            applicationDataRootFolder = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        }

        if (string.IsNullOrEmpty(applicationDataRootFolder))
        {
            string userHomeFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            applicationDataRootFolder = Path.Combine(userHomeFolderPath, ".local", "share");
        }

        return Path.Combine(applicationDataRootFolder, "devtoys");
    }
}
