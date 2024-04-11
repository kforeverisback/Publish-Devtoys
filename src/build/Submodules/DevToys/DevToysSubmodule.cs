using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Helper;
using Nuke.Common.IO;

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
        else
        {
            throw new InvalidOperationException("You must run Windows, macOS or Linux.");
        }
    }
}
