using System.IO;
using Core;
using Helper;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Submodules.DevToys.PublishBinariesBuilders;

internal sealed class GuiMacOSPublishBinariesBuilder : PublishBinariesBuilder
{
    private readonly AbsolutePath _projectPath;

    public GuiMacOSPublishBinariesBuilder(
        AbsolutePath submodulePath,
        TargetCpuArchitecture architecture)
        : base("DevToys GUI (MacOS)", architecture, selfContained: true)
    {
        _projectPath = submodulePath / "src" / "app" / "dev" / "platforms" / "desktop" / "DevToys.MacOS" / "DevToys.MacOS.csproj";
    }

    internal override void Build(AbsolutePath publishDirectory, AbsolutePath assetsDirectory, Configuration configuration)
    {
        AbsolutePath outputPath = publishDirectory / $"{_projectPath.NameWithoutExtension}-{Architecture.RuntimeIdentifier}";

        Microsoft.Build.Evaluation.Project project = ProjectModelTasks.ParseProject(_projectPath);
        ProjectProperty targetFramework = project.GetProperty("TargetFramework");

        DotNetBuild(
            s => s
            .SetProjectFile(_projectPath)
            .SetConfiguration(configuration)
            .SetFramework(targetFramework.EvaluatedValue)
            .SetRuntime(Architecture.RuntimeIdentifier)
            .SetPlatform(Architecture.PlatformTarget)
            .SetSelfContained(SelfContained)
            .SetPublishSingleFile(false)
            .SetPublishReadyToRun(false)
            .SetPublishTrimmed(true) // HACK: Required for MacOS. However, <LinkMode>None</LinkMode> in the CSPROJ disables trimming.
            .SetVerbosity(DotNetVerbosity.quiet)
            .SetProcessArgumentConfigurator(_ => _
                .Add("/p:RuntimeIdentifierOverride=" + Architecture.RuntimeIdentifier)
                .Add("/p:CreatePackage=False") /* Will NOT create an installable .pkg */
                .Add($"/bl:\"{outputPath}.binlog\""))
            .SetOutputDirectory(outputPath));

        // Copy DevToys.Tools to the app
        AbsolutePath appFile = outputPath / "DevToys.app";
        if (!appFile.DirectoryExists())
        {
            throw new FileNotFoundException("Unable to find DevToys.app file");
        }
        
        NuGetHelper.UnpackNuGetPackage(
            NuGetHelper.FindDevToysToolsNuGetPackage(publishDirectory),
            appFile / "Contents" / "Resources" / "Plugins" / "DevToys.Tools");
        
        OutputPath = outputPath;
    }
}
