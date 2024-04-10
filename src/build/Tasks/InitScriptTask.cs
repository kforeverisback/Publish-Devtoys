using System.Threading.Tasks;
using Helper;
using Nuke.Common.IO;
using Serilog;

namespace Tasks;

internal static class InitScriptTask
{
    internal static async Task RunAsync(AbsolutePath repositoryRootDirectory)
    {
        AbsolutePath devToysPath = repositoryRootDirectory / "submodules" / "DevToys" / "init.cmd";

        Log.Information("Initializing DevToys repository.");
        await ShellHelper.RunScriptAsync(devToysPath);
    }
}
