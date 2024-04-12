﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Helper;
using Nuke.Common.IO;
using Serilog;
using Submodules.DevToys.Packing;
using Submodules.DevToys.PublishBinariesBuilders;
using static Core.TargetCpuArchitecture;

namespace Submodules.DevToys;

internal sealed class DevToysSubmodule : SubmoduleBase
{
    private ImmutableArray<PublishBinariesBuilder> publishBinariesBuilders = ImmutableArray<PublishBinariesBuilder>.Empty;

    public DevToysSubmodule(AbsolutePath repositoryDirectory)
        : base("DevToys", repositoryDirectory / "submodules" / "DevToys")
    {
    }

    internal override async ValueTask RestoreAsync()
    {
        AbsolutePath initPath = RepositoryDirectory / "init.cmd";
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

    internal override ValueTask BuildPublishBinariesAsync(AbsolutePath publishDirectory, Configuration configuration)
    {
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

            builder.Build(publishDirectory, configuration);

            Log.Information(string.Empty);
        }

        return ValueTask.CompletedTask;
    }

    internal override ValueTask PackPublishBinariesAsync(AbsolutePath packDirectory, Configuration configuration)
    {
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
                    // TODO
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
                GuiPackingWindows.Pack(guiWindowsPublishBinariesBuilder);
            }
            // TODO: Mac and Linux GUI

            Log.Information(string.Empty);
        }

        return ValueTask.CompletedTask;
    }

    private IEnumerable<PublishBinariesBuilder> GetMacOSProjectsToPublish()
    {
        // GUI
        // TODO

        // CLI
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_X64, selfContained: true);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_Arm64, selfContained: true);

        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_X64, selfContained: false);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, MacOs_Arm64, selfContained: false);
    }

    private IEnumerable<PublishBinariesBuilder> GetWindowsProjectsToPublish()
    {
        // GUI
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_X86, selfContained: true);
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_X64, selfContained: true);
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_Arm64, selfContained: true);

        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_X86, selfContained: false);
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_X64, selfContained: false);
        yield return new GuiWindowsPublishBinariesBuilder(RepositoryDirectory, Windows_Arm64, selfContained: false);

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
}
