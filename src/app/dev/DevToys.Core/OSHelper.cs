namespace DevToys.Core;

public static class OSHelper
{
    public static bool IsOsSupported(IReadOnlyList<Platform> targetPlatforms)
    {
        if (targetPlatforms.Count > 0)
        {
            Platform currentPlatform;
            if (OperatingSystem.IsBrowser())
            {
                currentPlatform = Platform.WASM;
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
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
}
