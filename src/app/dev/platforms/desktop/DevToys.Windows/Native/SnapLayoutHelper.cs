using Microsoft.Win32;

namespace DevToys.Windows.Native;

internal static class SnapLayoutHelper
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string RegistryValueName = "EnableSnapAssistFlyout";

    internal static bool IsSnapLayoutEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        object? registryValueObject = key?.GetValue(RegistryValueName);

        if (registryValueObject == null)
        {
            return true;
        }

        int registryValue = (int)registryValueObject;

        return registryValue > 0;
    }
}
