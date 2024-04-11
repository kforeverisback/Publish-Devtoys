using Core;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Submodules.DevToys;

internal sealed class GuiWindowsPublishBinariesBuilder : PublishBinariesBuilder
{
    private readonly AbsolutePath _projectPath;

    private AbsolutePath? _outputPath;

    public GuiWindowsPublishBinariesBuilder(
        AbsolutePath submodulePath,
        TargetCpuArchitecture architecture,
        bool selfContained)
        : base("DevToys GUI (Windows)", architecture, selfContained)
    {
        _projectPath = submodulePath / "src" / "app" / "dev" / "platforms" / "desktop" / "DevToys.Windows" / "DevToys.Windows.csproj";
    }

    internal override void Build(AbsolutePath outputDirectory, Configuration configuration)
    {
        _outputPath = outputDirectory / $"{_projectPath.NameWithoutExtension}-{Architecture.RuntimeIdentifier}{(SelfContained ? "-portable" : "")}";

        Microsoft.Build.Evaluation.Project project = ProjectModelTasks.ParseProject(_projectPath);
        ProjectProperty targetFramework = project.GetProperty("TargetFramework");

        DotNetPublish(
            s => s
            .SetProject(_projectPath)
            .SetConfiguration(configuration)
            .SetFramework(targetFramework.EvaluatedValue)
            .SetRuntime(Architecture.RuntimeIdentifier)
            .SetPlatform(Architecture.PlatformTarget)
            .SetSelfContained(SelfContained)
            .SetPublishSingleFile(false)
            .SetPublishReadyToRun(false)
            .SetPublishTrimmed(false)
            .SetVerbosity(DotNetVerbosity.quiet)
            .SetProcessArgumentConfigurator(_ => _
                .Add("/p:RuntimeIdentifierOverride=" + Architecture.RuntimeIdentifier)
                .Add("/p:Unpackaged=" + SelfContained)
                .Add($"/bl:\"{_outputPath}.binlog\""))
            .SetOutput(_outputPath));
    }
}
