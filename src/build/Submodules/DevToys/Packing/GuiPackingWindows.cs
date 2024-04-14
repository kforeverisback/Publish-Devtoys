using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Core;
using InnoSetup.ScriptBuilder;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.InnoSetup;
using Serilog;
using Submodules.DevToys.PublishBinariesBuilders;
using WindowsTooling.AppxManifest;
using WindowsTooling.Progress;
using WindowsTooling.Sdk;

namespace Submodules.DevToys.Packing;

internal static class GuiPackingWindows
{
    internal static async Task PackAsync(AbsolutePath packDirectory, AbsolutePath devToysRepositoryDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        bool isPreview = true; // TODO

        Zip(packDirectory, guiWindowsPublishBinariesBuilder);
        CreateSetup(packDirectory, devToysRepositoryDirectory, guiWindowsPublishBinariesBuilder, isPreview);
        await CreateMSIXAsync(packDirectory, guiWindowsPublishBinariesBuilder, isPreview);
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
                fileMode: FileMode.Create);
        }

        Log.Information(string.Empty);
    }

    private static void CreateSetup(AbsolutePath packDirectory, AbsolutePath devToysRepositoryDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder, bool isPreview)
    {
        Log.Information("Creating installer for DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        AbsolutePath innoSetupScriptFile
            = GenerateInnoSetupScript(
                versionNumber: "2.0.0-prev.0", // TODO
                isPreview,
                packDirectory,
                devToysRepositoryDirectory,
                guiWindowsPublishBinariesBuilder);

        AbsolutePath innoSetupCompiler = NuGetToolPathResolver.GetPackageExecutable("Tools.InnoSetup", "ISCC.exe");

        InnoSetupTasks.InnoSetup(config => config
            .SetProcessToolPath(innoSetupCompiler)
            .SetScriptFile(innoSetupScriptFile)
            .SetOutputDir(packDirectory));

        Log.Information(string.Empty);
    }

    private static async Task CreateMSIXAsync(AbsolutePath packDirectory, GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder, bool isPreview)
    {
        Log.Information("Creating Microsoft Store package for DevToys {architecutre}...", guiWindowsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        AbsolutePath sourceMappingFile
            = await CreateAppxManifestAsync(
                versionNumber: "2.0.0-prev.0", // TODO
                isPreview,
                guiWindowsPublishBinariesBuilder);

        AbsolutePath msixFile = packDirectory / $"devtoys_{guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}.msix";

        var progress = new Progress<ProgressData>(data =>
        {
            Log.Information(data.Message);
        });

        var sdk = new MakeAppxWrapper();
        await sdk.Pack(
            MakeAppxPackOptions.CreateFromMapping(
                sourceMappingFile,
                msixFile,
                compress: true,
                validate: true),
            progress,
            CancellationToken.None);

        Log.Information(string.Empty);
    }

    private static AbsolutePath GenerateInnoSetupScript(
        string versionNumber,
        bool isPreview,
        AbsolutePath packDirectory,
        AbsolutePath devToysRepositoryDirectory,
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
        AbsolutePath iconFile = devToysRepositoryDirectory / "assets" / "logo" / "Icon-Windows.ico";

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

    private static async Task<AbsolutePath> CreateAppxManifestAsync(
        string versionNumber,
        bool isPreview,
        GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
    {
        string displayName = "DevToys";
        string packageName = "DevToys";
        if (isPreview)
        {
            displayName = "DevToys Preview";
            packageName = "DevToys-Preview";
        }

        var fileListBuilder = new PackageFileListBuilder();
        fileListBuilder.AddDirectory(guiWindowsPublishBinariesBuilder.OutputPath, recursive: true, targetRelativeDirectory: string.Empty);

        AppxPackageArchitecture architecture;
        if (guiWindowsPublishBinariesBuilder.Architecture == TargetCpuArchitecture.Windows_Arm64)
        {
            architecture = AppxPackageArchitecture.Arm64;
        }
        else if (guiWindowsPublishBinariesBuilder.Architecture == TargetCpuArchitecture.Windows_X64)
        {
            architecture = AppxPackageArchitecture.x64;
        }
        else if (guiWindowsPublishBinariesBuilder.Architecture == TargetCpuArchitecture.Windows_X86)
        {
            architecture = AppxPackageArchitecture.x86;
        }
        else
        {
            throw new NotSupportedException($"Unsupported platform target: {guiWindowsPublishBinariesBuilder.Architecture.PlatformTarget}");
        }

        var options = new AppxManifestCreatorOptions
        {
            CreateLogo = true,
            EntryPoints = ["DevToys.Windows.exe"],
            PackageArchitecture = architecture,
            PackageName = packageName,
            PackageDisplayName = displayName,
            PackageDescription = "A Swiss Army knife for developers.",
            PublisherName = "CN=etiennebaudoux",
            PublisherDisplayName = "etiennebaudoux",
            Version = new Version(0, 0, 0, 0) // TODO
        };

        var temporaryFiles = new List<string>();
        var manifestCreator = new AppxManifestCreator();
        await foreach (CreatedItem result in manifestCreator.CreateManifestForDirectory(new DirectoryInfo(guiWindowsPublishBinariesBuilder.OutputPath), options, CancellationToken.None))
        {
            temporaryFiles.Add(result.SourcePath);

            if (result.PackageRelativePath == null)
            {
                continue;
            }

            fileListBuilder.AddFile(result.SourcePath, result.PackageRelativePath);
        }

        string tempFileList = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".list");
        temporaryFiles.Add(tempFileList);

        string tempManifestPath = Path.Combine(Path.GetTempPath(), "AppxManifest-" + Guid.NewGuid().ToString("N") + ".xml");
        temporaryFiles.Add(tempManifestPath);

        string srcManifest = fileListBuilder.GetManifestSourcePath();
        if (srcManifest == null || !File.Exists(srcManifest))
        {
            throw new InvalidOperationException("The selected folder cannot be packed because it has no manifest, and MSIX Hero was unable to create one. A manifest can be only created if the selected folder contains any executable file.");
        }

        // Copy manifest to a temporary file
        var injector = new MsixHeroBrandingInjector();
        await using (FileStream manifestStream = File.OpenRead(fileListBuilder.GetManifestSourcePath()))
        {
            XDocument xml = await XDocument.LoadAsync(manifestStream, LoadOptions.None, CancellationToken.None);
            await injector.Inject(xml);
            await File.WriteAllTextAsync(tempManifestPath, xml.ToString(SaveOptions.None), CancellationToken.None);
            fileListBuilder.AddManifest(tempManifestPath);
        }

        await File.WriteAllTextAsync(tempFileList, fileListBuilder.ToString(), CancellationToken.None);

        return tempFileList;
    }
}
