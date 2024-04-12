using System.Threading.Tasks;
using Core;
using Nuke.Common.IO;

namespace Tasks;

internal static class PackPublishBinariesTask
{
    internal static async ValueTask RunAsync(AbsolutePath rootDirectory, SubmoduleBase[] submodules, Configuration configuration)
    {
        AbsolutePath artifactDirectory = rootDirectory / "artifacts";

        foreach (SubmoduleBase submodule in submodules)
        {
            await submodule.PackPublishBinariesAsync(artifactDirectory, configuration);
        }
    }
}
