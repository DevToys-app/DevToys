using System.Runtime.InteropServices;

namespace DevToys.Blazor.Core.Helpers;

internal static class Shell
{
    internal static void OpenFileInShell(string url)
    {
        Guard.IsNotNullOrWhiteSpace(url);

        try
        {
            var startInfo = new ProcessStartInfo(url);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
