using System.Runtime.InteropServices;
using DevToys.Api;

namespace DevToys.Core;

public static class OSHelper
{
    public static bool IsOsSupported(IReadOnlyList<Platform> targetPlatforms)
    {
        if (targetPlatforms.Count > 0)
        {
            Platform currentPlatform;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                currentPlatform = Platform.MacCatalyst;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentPlatform = Platform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                currentPlatform = Platform.Linux;
            }
            else if (OperatingSystem.IsBrowser())
            {
                currentPlatform = Platform.WASM;
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
