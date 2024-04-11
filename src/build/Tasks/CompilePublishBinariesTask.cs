using Core;
using Nuke.Common.IO;
using Serilog;

namespace Tasks;

internal static class CompilePublishBinariesTask
{
    internal static void Run(AbsolutePath rootDirectory, SubmoduleBase[] submodules, Configuration configuration)
    {
        AbsolutePath publishDirectory = rootDirectory / "publish";

        foreach (SubmoduleBase submodule in submodules)
        {
            foreach (PublishBinariesBuilder publishBinariesBuilder in submodule.GetPublishBinariesBuilder())
            {
                Log.Information(
                    "Building {PublishBinariesBuilderName} for {Architecture} (self-contained: {SelfContained})",
                    publishBinariesBuilder.Name,
                    publishBinariesBuilder.Architecture.RuntimeIdentifier,
                    publishBinariesBuilder.SelfContained);

                publishBinariesBuilder.Build(publishDirectory, configuration);

                Log.Information(string.Empty);
            }
        }
    }
}
