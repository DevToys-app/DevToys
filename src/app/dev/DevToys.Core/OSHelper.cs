namespace DevToys.Core;

public static class OSHelper
{
    public static bool IsOsSupported(IReadOnlyList<Platform> targetPlatforms)
    {
        if (targetPlatforms.Count > 0)
        {
            Platform currentPlatform;
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                currentPlatform = Platform.MacOS;
            }
            else if (OperatingSystem.IsWindows())
            {
                currentPlatform = Platform.Windows;
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                currentPlatform = Platform.Linux;
            }
            else
            {
                ThrowHelper.ThrowPlatformNotSupportedException();
                return false;
            }

            if (!targetPlatforms.Contains(currentPlatform))
            {
                return false;
            }
        }

        return true;
    }

    public static void OpenFileInShell(string fileOrUrl, string? arguments = null)
    {
        Guard.IsNotNullOrWhiteSpace(fileOrUrl);

        try
        {
            var startInfo = new ProcessStartInfo(fileOrUrl, arguments!);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (OperatingSystem.IsWindows())
            {
                fileOrUrl = fileOrUrl.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(fileOrUrl, arguments!) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                Process.Start("xdg-open", new[] { fileOrUrl, arguments! });
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                Process.Start("open", new[] { fileOrUrl, arguments! });
            }
            else
            {
                throw;
            }
        }
    }
}
