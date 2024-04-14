using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.PowerShell;
using Serilog;

namespace Helper;

internal static class ShellHelper
{
    internal static Task RunCommandAsync(string command)
    {
        if (OperatingSystem.IsWindows())
        {
            return RunPowerShellCommandAsync(command);
        }
        else
        {
            return RunUnixCommandAsync(command);
        }
    }

    internal static Task RunScriptAsync(AbsolutePath script)
    {
        if (OperatingSystem.IsWindows())
        {
            return RunPowerShellScriptAsync(script);
        }
        else
        {
            return RunUnixScriptAsync(script);
        }
    }

    private static Task RunPowerShellScriptAsync(AbsolutePath script)
    {
        if (script.Extension == ".ps1")
        {
            System.Collections.Generic.IReadOnlyCollection<Output> results
                = PowerShellTasks
                    .PowerShell(_ => _
                        .SetFile(script)
                        .SetProcessLogOutput(true)
                        .SetNoLogo(true)
                        .SetNoProfile(true));
        }
        else if (script.Extension == ".cmd")
        {
            IProcess process = ProcessTasks.StartProcess("cmd", $"/c \"{script}\"");
            process.AssertWaitForExit();
        }

        return Task.CompletedTask;
    }

    private static Task RunPowerShellCommandAsync(string command)
    {
        System.Collections.Generic.IReadOnlyCollection<Output> results
            = PowerShellTasks
                .PowerShell(_ => _
                    .SetCommand(command)
                    .SetProcessLogOutput(true)
                    .SetNoLogo(true)
                    .SetNoProfile(true));
        return Task.CompletedTask;
    }

    private static Task RunUnixScriptAsync(AbsolutePath script)
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

        string escapedScriptPathArgs = script.ToString().Replace("\"", "\\\"");
        return RunUnixCommandAsync($"{bashProgram} {escapedScriptPathArgs}");
    }

    private static async Task<int> RunUnixCommandAsync(string command)
    {
        var source = new TaskCompletionSource<int>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = EnvironmentInfo.WorkingDirectory
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
                source.SetException(new Exception($"Command `{command}` failed with exit code `{process.ExitCode}`"));
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
            Log.Error(e, "Command {} failed", command);
            source.SetException(e);
        }

        return await source.Task;
    }
}
