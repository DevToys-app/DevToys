using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.PowerShell;
using Serilog;

internal static class RestoreTask
{
    internal static Task RestoreDependenciesAsync(AbsolutePath rootDirectory)
    {
        if (OperatingSystem.IsWindows())
        {
            return RestoreWindowsAsync(rootDirectory);
        }
        else
        {
            return RestoreUnixAsync(rootDirectory);
        }
    }

    private static Task RestoreWindowsAsync(AbsolutePath rootDirectory)
    {
        System.Collections.Generic.IReadOnlyCollection<Output> results
            = PowerShellTasks
            .PowerShell(_ => _
                .SetFile(rootDirectory / "init.ps1")
                .SetProcessLogOutput(true)
                .SetProcessWorkingDirectory(rootDirectory)
                .SetNoLogo(true)
                .SetNoProfile(true));
        return Task.CompletedTask;
    }

    private static Task RestoreUnixAsync(AbsolutePath rootDirectory)
    {
        return Bash(rootDirectory / "ini.sh");
    }

    public static Task<int> Bash(string cmd)
    {
        var source = new TaskCompletionSource<int>();
        string escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        process.Exited += (sender, args) =>
        {
            Log.Warning(process.StandardError.ReadToEnd());
            Log.Information(process.StandardOutput.ReadToEnd());
            if (process.ExitCode == 0)
            {
                source.SetResult(0);
            }
            else
            {
                source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
            }

            process.Dispose();
        };

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            Log.Error(e, "Command {} failed", cmd);
            source.SetException(e);
        }

        return source.Task;
    }
}
