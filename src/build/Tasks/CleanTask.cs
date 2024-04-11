﻿using Core;
using Nuke.Common.IO;
using Serilog;

namespace Tasks;

internal static class CleanTask
{
    internal static void Run(SubmoduleBase[] submodules)
    {
        foreach (SubmoduleBase submodule in submodules)
        {
            Log.Information("Cleaning {Value} repository.", submodule.Name);

            foreach (AbsolutePath directory in submodule.GetDirectoriesToClean())
            {
                directory.CreateOrCleanDirectory();
                Log.Information("Deleted {Value} directory.", directory);
            }
        }
    }
}