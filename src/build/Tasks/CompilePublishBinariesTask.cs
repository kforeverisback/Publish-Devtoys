﻿using System.Threading.Tasks;
using Core;
using Nuke.Common.IO;

namespace Tasks;

internal static class CompilePublishBinariesTask
{
    internal static async ValueTask RunAsync(AbsolutePath rootDirectory, SubmoduleBase[] submodules, Configuration configuration)
    {
        AbsolutePath publishDirectory = rootDirectory / "publish";

        foreach (SubmoduleBase submodule in submodules)
        {
            await submodule.BuildPublishBinariesAsync(publishDirectory, configuration);
        }
    }
}
