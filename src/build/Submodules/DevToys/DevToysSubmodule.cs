using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Core;
using Helper;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using Submodules.DevToys.Packing.MacOS;
using Submodules.DevToys.Packing.Windows;
using Submodules.DevToys.PublishBinariesBuilders;
using static Core.TargetCpuArchitecture;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Submodules.DevToys;

internal sealed class DevToysSubmodule : SubmoduleBase
{
    private AbsolutePath? _devToysApiOutputPath;
    private ImmutableArray<PublishBinariesBuilder> publishBinariesBuilders = ImmutableArray<PublishBinariesBuilder>.Empty;

    public DevToysSubmodule(AbsolutePath repositoryDirectory)
        : base("DevToys", repositoryDirectory / "submodules" / "DevToys")
    {
    }

    internal override async ValueTask RestoreAsync()
    {
        AbsolutePath initPath;
        if (OperatingSystem.IsWindows())
        {
            initPath = RepositoryDirectory / "init.cmd";
        }
        else
        {
            initPath = RepositoryDirectory / "init.sh";
        }
        await ShellHelper.RunScriptAsync(initPath);
    }

    internal override IEnumerable<AbsolutePath> GetSolutions()
    {
        if (OperatingSystem.IsMacOS())
        {
            yield return RepositoryDirectory / "src" / "DevToys-MacOS.sln";
        }
        else if (OperatingSystem.IsWindows())
        {
            yield return RepositoryDirectory / "src" / "DevToys-Windows.sln";
        }
        else if (OperatingSystem.IsLinux())
        {
            yield return RepositoryDirectory / "src" / "DevToys-Linux.sln";
        }
    }

    internal override ValueTask BuildPublishBinariesAsync(AbsolutePath publishDirectory, AbsolutePath assetsDirectory, Configuration configuration)
    {
        BuildDevToysApiNuGetPackage(publishDirectory, configuration);

        if (OperatingSystem.IsMacOS())
        {
            this.publishBinariesBuilders = GetMacOSProjectsToPublish().ToImmutableArray();
        }
        else if (OperatingSystem.IsWindows())
        {
            this.publishBinariesBuilders = GetWindowsProjectsToPublish().ToImmutableArray();
        }
        else if (OperatingSystem.IsLinux())
        {
            this.publishBinariesBuilders = GetLinuxProjectsToPublish().ToImmutableArray();
        }

        foreach (PublishBinariesBuilder builder in this.publishBinariesBuilders)
        {
            Log.Information(
                "Building {PublishBinariesBuilderName} for {Architecture} (self-contained: {SelfContained})",
                builder.Name,
                builder.Architecture.RuntimeIdentifier,
                builder.SelfContained);

            builder.Build(publishDirectory, assetsDirectory, configuration);

            Log.Information(string.Empty);
        }

        return ValueTask.CompletedTask;
    }

    internal override async ValueTask PackPublishBinariesAsync(AbsolutePath packDirectory, Configuration configuration)
    {
        Log.Information(messageTemplate: "Copying DevToys.Api NuGet package to artifacts");

        _devToysApiOutputPath
            .GetFiles("*.nupkg")
            .ForEach(x => x
                .MoveToDirectory(packDirectory));

        Log.Information(string.Empty);

        foreach (PublishBinariesBuilder builder in this.publishBinariesBuilders)
        {
            Log.Information(
                "Packing {PublishBinariesBuilderName} for {Architecture} (self-contained: {SelfContained})",
                builder.Name,
                builder.Architecture.RuntimeIdentifier,
                builder.SelfContained);

            if (builder is CliPublishBinariesBuilder cliPublishBinariesBuilder)
            {
                if (OperatingSystem.IsMacOS())
                {
                    CliPackingMacOS.Pack(packDirectory, cliPublishBinariesBuilder);
                }
                else if (OperatingSystem.IsWindows())
                {
                    CliPackingWindows.Pack(packDirectory, cliPublishBinariesBuilder);
                }
                else if (OperatingSystem.IsLinux())
                {
                    // TODO
                }
            }
            else if (builder is GuiWindowsPublishBinariesBuilder guiWindowsPublishBinariesBuilder)
            {
                await GuiPackingWindows.PackAsync(packDirectory, RepositoryDirectory, guiWindowsPublishBinariesBuilder);
            }
            else if (builder is GuiMacOSPublishBinariesBuilder guiMacOsPublishBinariesBuilder)
            {
                GuiPackingMacOS.Pack(packDirectory, RepositoryDirectory, guiMacOsPublishBinariesBuilder, configuration);
            }
            // TODO: Linux GUI

            Log.Information(string.Empty);
        }
    }

    private IEnumerable<PublishBinariesBuilder> GetMacOSProjectsToPublish()
    {
        // GUI
        yield return new GuiMacOSPublishBinariesBuilder(RepositoryDirectory, MacOs_X64);
        yield return new GuiMacOSPublishBinariesBuilder(RepositoryDirectory, MacOs_Arm64);

        // CLI
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_X64, selfContained: true);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_Arm64, selfContained: true);

        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_X64, selfContained: false);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_Arm64, selfContained: false);
    }

    private IEnumerable<PublishBinariesBuilder> GetWindowsProjectsToPublish()
    {
        // GUI
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_X86);
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_X64);
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_Arm64);

        // CLI
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Windows_X86, selfContained: true);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Windows_X64, selfContained: true);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Windows_Arm64, selfContained: true);

        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Windows_X86, selfContained: false);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Windows_X64, selfContained: false);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Windows_Arm64, selfContained: false);
    }

    private IEnumerable<PublishBinariesBuilder> GetLinuxProjectsToPublish()
    {
        // GUI
        // TODO

        // CLI
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_X64, selfContained: true);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_Arm, selfContained: true);

        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_X64, selfContained: false);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_Arm, selfContained: false);
    }

    private void BuildDevToysApiNuGetPackage(AbsolutePath publishDirectory, Configuration configuration)
    {
        Log.Information("Building DevToys.Api NuGet package");

        _devToysApiOutputPath = publishDirectory / $"DevToys.Api";
        AbsolutePath projectPath = RepositoryDirectory / "src" / "app" / "dev" / "DevToys.Api" / "DevToys.Api.csproj";

        Microsoft.Build.Evaluation.Project project = ProjectModelTasks.ParseProject(projectPath);
        ProjectProperty targetFramework = project.GetProperty("TargetFramework");

        DotNetPack(
            s => s
            .SetProject(projectPath)
            .SetConfiguration(configuration)
            .SetPublishSingleFile(false)
            .SetPublishReadyToRun(false)
            .SetPublishTrimmed(false)
            .SetVerbosity(DotNetVerbosity.quiet)
            .SetProcessArgumentConfigurator(_ => _
                .Add($"/bl:\"{_devToysApiOutputPath}.binlog\""))
            .SetOutputDirectory(_devToysApiOutputPath));

        Log.Information(string.Empty);
    }
}
