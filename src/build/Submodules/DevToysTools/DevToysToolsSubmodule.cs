using System.Collections.Generic;
using Core;
using Nuke.Common.IO;

namespace Submodules.DevToysTools;

internal sealed class DevToysToolsSubmodule : SubmoduleBase
{
    public DevToysToolsSubmodule(AbsolutePath repositoryDirectory)
        : base("DevToys.Tools", repositoryDirectory / "submodules" / "DevToys.Tools")
    {
    }

    internal override IEnumerable<AbsolutePath> GetSolutions()
    {
        yield return RepositoryDirectory / "src" / "DevToys.Tools.sln";
    }

    internal override IEnumerable<PublishBinariesBuilder> GetPublishBinariesBuilder()
    {
        yield break;
    }
}
