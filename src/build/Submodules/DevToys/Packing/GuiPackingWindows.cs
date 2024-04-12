using InnoSetup.ScriptBuilder;
using Nuke.Common.IO;
using Serilog;
using Submodules.DevToys.PublishBinariesBuilders;

namespace Submodules.DevToys.Packing;

internal static class GuiPackingWindows
{
    internal static void Pack(AbsolutePath packDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Zip(packDirectory, guiWindowsPublishBinariesBuilder);
        CreateSetup(packDirectory, guiWindowsPublishBinariesBuilder);
        CreateMSIX(packDirectory, guiWindowsPublishBinariesBuilder);
    }

    private static void Zip(AbsolutePath packDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Log.Information("Zipping DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        AbsolutePath archiveFile = packDirectory / $"devtoys_win_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}_portable.zip";

        if (guiWindowsPublishBinariesBuilder.OutputPath.DirectoryExists())
        {
            guiWindowsPublishBinariesBuilder.OutputPath.ZipTo(
                archiveFile,
                filter: null,
                compressionLevel: System.IO.Compression.CompressionLevel.SmallestSize,
                fileMode: System.IO.FileMode.Create);
        }

        Log.Information(string.Empty);
    }

    private static void CreateSetup(AbsolutePath packDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Log.Information("Creating installer for DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        AbsolutePath archiveFile = packDirectory / $"devtoys_setup_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}.exe";

        if (guiWindowsPublishBinariesBuilder.OutputPath.DirectoryExists())
        {
            BuilderUtils.Build(
                builder =>
                {
                    builder.Setup
                        .Create("DevToys")
                        .AppVersion("2.0.0-prev.0") // TODO
                        .DefaultDirName(@"{userpf}\DevToys") // userpf = C:\Users\{username}\AppData\Local\Programs
                        .PrivilegesRequired(PrivilegesRequired.Lowest)
                        .OutputBaseFilename($"devtoys_setup_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}_2.0.0-prev.0") // TODO
                        .SetupIconFile("ToolsIcon.ico")
                        .UninstallDisplayIcon("ToolsIcon.ico")
                        .DisableDirPage(YesNo.Yes);
                    builder.Files
                        .CreateEntry(@"bin\*", InnoConstants.App)
                        .Flags(FileFlags.IgnoreVersion | FileFlags.RecurseSubdirs);
                },
                path: guiWindowsPublishBinariesBuilder.OutputPath!.Parent / $"devtoys_setup_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}.iss");
        }

        Log.Information(string.Empty);
    }

    private static void CreateMSIX(AbsolutePath packDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Log.Information("Creating Microsoft Store package for DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        // todo: implement MSIX creation

        Log.Information(string.Empty);
    }
}
