using Nuke.Common.IO;
using Submodules.DevToys.PublishBinariesBuilders;

namespace Submodules.DevToys.Packing;

internal static class CliPackingWindows
{
    internal static void Pack(AbsolutePath packDirectory, CliPublishBinariesBuilder cliPublishBinariesBuilder)
    {
        string portable = string.Empty;
        if (cliPublishBinariesBuilder.SelfContained)
        {
            portable = ".portable";
        }

        AbsolutePath archiveFile = packDirectory / $"DevToys-CLI.{cliPublishBinariesBuilder.Architecture.PlatformTarget}{portable}.zip";

        if (cliPublishBinariesBuilder.OutputPath.DirectoryExists())
        {
            cliPublishBinariesBuilder.OutputPath.ZipTo(
                archiveFile,
                filter: null,
                compressionLevel: System.IO.Compression.CompressionLevel.SmallestSize,
                fileMode: System.IO.FileMode.Create);
        }
    }
}
