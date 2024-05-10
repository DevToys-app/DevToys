using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;

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

    internal static void ScheduleExtensionToBeUninstalled(string extensionInstallationPath)
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

    internal static async Task<ExtensionInstallationResult> InstallExtensionAsync(SandboxedFileReader nugetPackageFile)
    {
        using Stream nugetPackageStream = await nugetPackageFile.GetNewAccessToFileContentAsync(CancellationToken.None);
        using var reader = new PackageArchiveReader(nugetPackageStream);

        NuspecReader nuspecReader = reader.NuspecReader;

        for (int j = 0; j < ExtensionInstallationFolders.Length; j++)
        {
            string potentialExtensionInstallationPath = Path.Combine(ExtensionInstallationFolders[j], nuspecReader.GetId());
            if (Directory.Exists(potentialExtensionInstallationPath))
            {
                // Extension is already installed.
                return new(AlreadyInstalled: true, nuspecReader, ExtensionInstallationPath: string.Empty);
            }
        }

        string extensionInstallationPath
            = Path.Combine(PreferredExtensionInstallationFolder, nuspecReader.GetId());

        // Unzip the extension.
        string[] pathToExclude = GetPathToExclude().ToArray();
        Directory.CreateDirectory(extensionInstallationPath);
        foreach (string? packagedFile in reader.GetFiles())
        {
            if (!pathToExclude.Any(path => packagedFile.Contains(path, StringComparison.CurrentCultureIgnoreCase)))
            {
                reader.ExtractFile(packagedFile, Path.Combine(extensionInstallationPath, packagedFile), null);
            }
        }

        return new(AlreadyInstalled: false, nuspecReader, extensionInstallationPath);
    }

    private static IEnumerable<string> GetPathToExclude()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return "runtimes/osx/";
            yield return "runtimes/osx-x86/";
            yield return "runtimes/osx-x64/";
            yield return "runtimes/osx-arm64/";
            yield return "runtimes/linux/";
            yield return "runtimes/linux-x86/";
            yield return "runtimes/linux-x64/";
            yield return "runtimes/linux-arm64/";
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    yield return "runtimes/win-x64/";
                    yield return "runtimes/win-arm64/";
                    break;
                case Architecture.X64:
                    yield return "runtimes/win-x86/";
                    yield return "runtimes/win-arm64/";
                    break;
                case Architecture.Arm:
                case Architecture.Arm64:
                    yield return "runtimes/win-x86/";
                    yield return "runtimes/win-x64/";
                    break;
                default:
                    break;
            }
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            yield return "runtimes/osx/";
            yield return "runtimes/osx-x86/";
            yield return "runtimes/osx-x64/";
            yield return "runtimes/osx-arm64/";
            yield return "runtimes/win/";
            yield return "runtimes/win-x86/";
            yield return "runtimes/win-x64/";
            yield return "runtimes/win-arm64/";
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    yield return "runtimes/linux-x64/";
                    yield return "runtimes/linux-arm64/";
                    break;
                case Architecture.X64:
                    yield return "runtimes/linux-x86/";
                    yield return "runtimes/linux-arm64/";
                    break;
                case Architecture.Arm:
                case Architecture.Arm64:
                    yield return "runtimes/linux-x86/";
                    yield return "runtimes/linux-x64/";
                    break;
                default:
                    break;
            }
        }
        else
        {
            yield return "runtimes/linux/";
            yield return "runtimes/linux-x86/";
            yield return "runtimes/linux-x64/";
            yield return "runtimes/linux-arm64/";
            yield return "runtimes/win/";
            yield return "runtimes/win-x86/";
            yield return "runtimes/win-x64/";
            yield return "runtimes/win-arm64/";
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    yield return "runtimes/osx-x64/";
                    yield return "runtimes/osx-arm64/";
                    break;
                case Architecture.X64:
                    yield return "runtimes/osx-x86/";
                    yield return "runtimes/osx-arm64/";
                    break;
                case Architecture.Arm:
                case Architecture.Arm64:
                    yield return "runtimes/osx-x86/";
                    yield return "runtimes/osx-x64/";
                    break;
                default:
                    break;
            }
        }
    }
}
