namespace DevToys.Blazor.BuiltInTools.ExtensionsManager;

public static class ExtensionInstallationManager
{
    public static string PreferredExtensionInstallationFolder { get; set; } = string.Empty;

    public static string[] ExtensionInstallationFolders { get; set; } = Array.Empty<string>();

    public static void UninstallExtensionsScheduledForRemoval()
    {
        if (!Directory.Exists(PreferredExtensionInstallationFolder))
        {
            return;
        }

        string scheduledForUninstallFilePath = Path.Combine(PreferredExtensionInstallationFolder, "uninstall.txt");
        if (File.Exists(scheduledForUninstallFilePath))
        {
            Process[] appInstances = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            // Only uninstall if there's only 1 instance of the app running.
            if (appInstances.Length == 1)
            {
                string[] lines = File.ReadAllLines(scheduledForUninstallFilePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    try
                    {
                        // Make sure we're not deleting something that's not in the extension folder. Could be a security concern otherwise.
                        if (ExtensionInstallationFolders.Any(path => lines[i].StartsWith(path)))
                        {
                            if (Directory.Exists(lines[i]))
                            {
                                Directory.Delete(lines[i], true);
                            }
                        }
                    }
                    catch
                    {
                        // TODO: Log error.
                    }
                }

                File.Delete(scheduledForUninstallFilePath);
            }
        }
    }

    public static void ScheduleExtensionToBeUninstalled(string extensionInstallationPath)
    {
        if (!Directory.Exists(PreferredExtensionInstallationFolder))
        {
            Directory.CreateDirectory(PreferredExtensionInstallationFolder);
        }

        string scheduledForUninstallFilePath = Path.Combine(PreferredExtensionInstallationFolder, "uninstall.txt");
        if (!File.Exists(scheduledForUninstallFilePath))
        {
            File.CreateText(scheduledForUninstallFilePath).Dispose();
        }

        File.AppendAllLines(scheduledForUninstallFilePath, new[] { extensionInstallationPath });
    }
}
