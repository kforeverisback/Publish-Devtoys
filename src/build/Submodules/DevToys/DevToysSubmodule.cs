using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Helper;
using Nuke.Common.IO;
using static Core.TargetCpuArchitecture;

namespace Submodules.DevToys;

internal sealed class DevToysSubmodule : SubmoduleBase
{
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

    internal override IEnumerable<PublishBinariesBuilder> GetPublishBinariesBuilder()
    {
        if (OperatingSystem.IsMacOS())
        {
            return GetMacOSProjectsToPublish();
        }
        else if (OperatingSystem.IsWindows())
        {
            return GetWindowsProjectsToPublish();
        }
        else if (OperatingSystem.IsLinux())
        {
            return GetLinuxProjectsToPublish();
        }

        return Array.Empty<PublishBinariesBuilder>();
    }

    private IEnumerable<PublishBinariesBuilder> GetMacOSProjectsToPublish()
    {
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
        // CLI
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_X64, selfContained: true);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_Arm, selfContained: true);

        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_X64, selfContained: false);
        yield return new CliPublishBinariesBuilder(RepositoryDirectory, Linux_Arm, selfContained: false);
    }
}
