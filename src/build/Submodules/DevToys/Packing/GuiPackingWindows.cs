using InnoSetup.ScriptBuilder;
using Nuke.Common.IO;
using Serilog;
using Submodules.DevToys.PublishBinariesBuilders;

namespace Submodules.DevToys.Packing;

internal static class GuiPackingWindows
{
    internal static void Pack(AbsolutePath packDirectory, AbsolutePath repositoryDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Zip(packDirectory, guiWindowsPublishBinariesBuilder);
        CreateSetup(packDirectory, repositoryDirectory, guiWindowsPublishBinariesBuilder);
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

    private static void CreateSetup(AbsolutePath packDirectory, AbsolutePath repositoryDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Log.Information("Creating installer for DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        AbsolutePath innoSetupScriptFile
            = GenerateInnoSetupScript(
                versionNumber: "2.0.0-prev.0", // TODO
                isPreview: true, // TODO
                packDirectory,
                repositoryDirectory,
                guiWindowsPublishBinariesBuilder);

        Log.Information(string.Empty);
    }

    private static void CreateMSIX(AbsolutePath packDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        Log.Information("Creating Microsoft Store package for DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        // todo: implement MSIX creation

        Log.Information(string.Empty);
    }

    private static AbsolutePath GenerateInnoSetupScript(
        string versionNumber,
        bool isPreview,
        AbsolutePath packDirectory,
        AbsolutePath repositoryDirectory,
        GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        string appName = "DevToys";
        if (isPreview)
        {
            appName = "DevToys Preview";
        }

        AbsolutePath binDirectory = guiWindowsPublishBinariesBuilder.OutputPath!;
        AbsolutePath exeFile = guiWindowsPublishBinariesBuilder.OutputPath / "DevToys.Windows.exe";
        AbsolutePath archiveFile = packDirectory / $"devtoys_setup_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}.exe";
        AbsolutePath iconFile = repositoryDirectory / "assets" / "logo" / "Icon-Windows.ico";

        AbsolutePath innoSetupScriptFile = binDirectory.Parent / $"devtoys_setup_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}.iss";

        BuilderUtils.Build(
            builder =>
            {
                builder.Setup
                    // Basic info
                    .Create(appName)
                    .AppPublisher("DevToys")
                    .DefaultGroupName(appName)
                    .AppPublisherURL("https://devtoys.app")
                    .AppSupportURL("https://github.com/DevToys-app/DevToys/issues")
                    .AppVersion(versionNumber)
                    // Paths
                    .OutputDir(packDirectory)
                    .OutputBaseFilename($"devtoys_setup_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}_{versionNumber}")
                    .DefaultDirName(@$"{InnoConstants.Shell.UserProgramFiles}\{appName}") // userpf = C:\Users\{username}\AppData\Local\Programs
                    .PrivilegesRequired(PrivilegesRequired.Lowest)
                    .LicenseFile(binDirectory / "LICENSE.md")
                    // Compression
                    .Compression("lzma")
                    .SolidCompression(YesNo.Yes)
                    // Architecture
                    .ArchitecturesAllowed(Architectures.X86 | Architectures.X64 | Architectures.Arm64)
                    .ArchitecturesInstallIn64BitMode(ArchitecturesInstallIn64BitMode.X64 | ArchitecturesInstallIn64BitMode.Arm64)
                    // UX
                    .SetupIconFile(iconFile)
                    .UninstallDisplayIcon(iconFile)
                    .WizardStyle(WizardStyle.Modern)
                    .ShowLanguageDialog(YesNo.No)
                    .DisableDirPage(YesNo.Yes)
                    .DisableProgramGroupPage(YesNo.Yes);

                // Task to create desktop icon
                builder.Tasks
                    .CreateEntry("desktopicon", "{cm:CreateDesktopIcon}")
                    .GroupDescription("{cm:AdditionalIcons}")
                    .Flags(TaskFlags.Unchecked);

                // Files
                builder.Files
                    .CreateEntry(@$"{binDirectory}\*", InnoConstants.Directories.App)
                    .Flags(FileFlags.IgnoreVersion | FileFlags.RecurseSubdirs);

                builder.Icons
                    // Start menu icon
                    .CreateEntry(@$"{InnoConstants.Shell.UserPrograms}\{appName}", @$"{InnoConstants.Directories.App}\{exeFile.Name}")
                    // Desktop icon
                    .CreateEntry(@$"{InnoConstants.Shell.UserDesktop}\{appName}", @$"{InnoConstants.Directories.App}\{exeFile.Name}")
                    .Tasks("desktopicon");

                // Run app after installation
                builder.Run
                    .CreateEntry($@"{InnoConstants.Directories.App}\{exeFile.Name}")
                    .Flags(RunFlags.NoWait | RunFlags.PostInstall | RunFlags.SkipIfSilent);

            },
            path: innoSetupScriptFile);

        return innoSetupScriptFile;
    }
}
