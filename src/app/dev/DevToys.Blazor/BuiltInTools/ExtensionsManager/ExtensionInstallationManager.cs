using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.BuiltInTools.ExtensionsManager;

public static class ExtensionInstallationManager
{
    private static readonly ILogger logger = typeof(ExtensionInstallationManager).Log();

    public static string PreferredExtensionInstallationFolder { get; set; } = string.Empty;

    public static string[] ExtensionInstallationFolders { get; set; } = Array.Empty<string>();

    public static void UninstallExtensionsScheduledForRemoval()
    {
        DateTime startTime = DateTime.UtcNow;
        if (!Directory.Exists(PreferredExtensionInstallationFolder))
        {
            return;
        }

        int uninstalledExtensions = 0;
        string scheduledForUninstallFilePath = Path.Combine(PreferredExtensionInstallationFolder, "uninstall.txt");
        if (File.Exists(scheduledForUninstallFilePath))
        {
            logger.LogInformation("There is one or more extension scheduled to be uninstalled.");
            Process[] appInstances = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            // Only uninstall if there's only 1 instance of the app running.
            if (appInstances.Length == 1)
            {
                string[] lines = File.ReadAllLines(scheduledForUninstallFilePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string extensionInstallationPath = lines[i];

                    try
                    {
                        // Make sure we're not deleting something that's not in the extension folder. Could be a security concern otherwise.
                        if (ExtensionInstallationFolders.Any(path => extensionInstallationPath.StartsWith(path)))
                        {
                            if (Directory.Exists(extensionInstallationPath))
                            {
                                Directory.Delete(extensionInstallationPath, true);
                                uninstalledExtensions++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unable to uninstall extension '{extensionInstallationPath}'. Try to start the app with administrator rights.", extensionInstallationPath);
                    }
                }

                File.Delete(scheduledForUninstallFilePath);
            }
            else
            {
                logger.LogWarning("Extensions won't be uninstall yet because another instance of the app is running (extensions may be in use at the moment). We will retry next time the app start.");
            }
        }

        double elapsedMilliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;
        logger.LogInformation("Finished uninstalling {uninstalledExtensions} extensions in {elapsedMilliseconds}ms", uninstalledExtensions, elapsedMilliseconds);
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
        logger.LogInformation("Scheduled to uninstall extension '{extensionInstallationPath}' next time the app start.", extensionInstallationPath);
    }
}
