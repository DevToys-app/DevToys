using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.PowerShell;
using Serilog;
using Output = Nuke.Common.Tooling.Output;

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
        return Bash(rootDirectory / "init.sh");
    }

    private static async Task<int> Bash(string cmd)
    {
        string bashProgram;
        if (OperatingSystem.IsMacOS())
        {
            bashProgram = "sh";
        }
        else
        {
            bashProgram = "bash";
        }

        var source = new TaskCompletionSource<int>();
        string escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{bashProgram} {escapedArgs}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        process.Exited += (sender, args) =>
        {
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
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Command {} failed", cmd);
            source.SetException(e);
        }

        return await source.Task;
    }
}
