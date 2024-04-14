using Core;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using Submodules.DevToys.PublishBinariesBuilders;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Submodules.DevToys.Packing.MacOS;

internal static class GuiPackingMacOS
{
    internal static void Pack(
        AbsolutePath packDirectory,
        AbsolutePath devToysRepositoryDirectory,
        GuiMacOSPublishBinariesBuilder guiMacOsPublishBinariesBuilder,
        Configuration configuration)
    {
        bool isPreview = true; // TODO

        Log.Information("Copying .app file of DevToys {architecutre}...", guiMacOsPublishBinariesBuilder.Architecture.RuntimeIdentifier);

        AbsolutePath appFile = guiMacOsPublishBinariesBuilder.OutputPath / "DevToys.app";
        FileSystemTasks.CopyDirectoryRecursively(appFile, packDirectory / guiMacOsPublishBinariesBuilder.Architecture.PlatformTarget / $"DevToys.app", DirectoryExistsPolicy.Fail);

        Log.Information(string.Empty);
        
        // TODO: To get a PKG with DevToys.Tools included, we will likely need to some changes in DevToys.MacOS to include the DevToys.Tools into bundle resources.
        
        // Log.Information("Creating installer for DevToys {architecutre}...", guiMacOsPublishBinariesBuilder.Architecture.RuntimeIdentifier);
        //
        // AbsolutePath projectPath = devToysRepositoryDirectory / "src" / "app" / "dev" / "platforms" / "desktop" / "DevToys.MacOS" / "DevToys.MacOS.csproj";
        // AbsolutePath outputPath = packDirectory / guiMacOsPublishBinariesBuilder.Architecture.PlatformTarget;
        //
        // Microsoft.Build.Evaluation.Project project = ProjectModelTasks.ParseProject(projectPath);
        // ProjectProperty targetFramework = project.GetProperty("TargetFramework");
        //
        // DotNetPublish(
        //     s => s
        //         .SetProject(projectPath)
        //         .SetConfiguration(configuration)
        //         .SetFramework(targetFramework.EvaluatedValue)
        //         .SetRuntime(guiMacOsPublishBinariesBuilder.Architecture.RuntimeIdentifier)
        //         .SetPlatform(guiMacOsPublishBinariesBuilder.Architecture.PlatformTarget)
        //         .SetSelfContained(guiMacOsPublishBinariesBuilder.SelfContained)
        //         .SetPublishSingleFile(false)
        //         .SetPublishReadyToRun(false)
        //         .SetPublishTrimmed(true) // HACK: Required for MacOS. However, <LinkMode>None</LinkMode> in the CSPROJ disables trimming.
        //         .SetVerbosity(DotNetVerbosity.quiet)
        //         .SetProcessArgumentConfigurator(_ => _
        //             .Add("/p:RuntimeIdentifierOverride=" + guiMacOsPublishBinariesBuilder.Architecture.RuntimeIdentifier)
        //             .Add("/p:CreatePackage=True") /* Will create an installable .pkg */
        //             /* .Add($"/bl:\"{outputPath}.binlog\"") */)
        //         .SetOutput(outputPath));
        //
        // Log.Information(string.Empty);
    }
}
