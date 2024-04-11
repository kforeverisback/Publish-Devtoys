using System.Threading.Tasks;
using Core;
using Serilog;

namespace Tasks;

internal static class RestoreTask
{
    internal static async Task RunAsync(SubmoduleBase[] submodules)
    {
        foreach (SubmoduleBase submodule in submodules)
        {
            Log.Information("Restoring {Value} repository's dependencies.", submodule.Name);
            await submodule.RestoreAsync();
            Log.Information("Restoration completed.");
        }
    }
}
