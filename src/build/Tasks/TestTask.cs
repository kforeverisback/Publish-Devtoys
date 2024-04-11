﻿using Core;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Tasks;

internal static class TestTask
{
    internal static void Run(SubmoduleBase[] submodules, Configuration configuration)
    {
        foreach (SubmoduleBase submodule in submodules)
        {
            foreach (AbsolutePath project in submodule.GetTestProjects())
            {
                RunTestProject(project, configuration);
            }
        }
    }

    private static void RunTestProject(AbsolutePath projectPath, Configuration configuration)
    {
        Log.Information("Running tests from {Value}.", projectPath.Name);

        DotNetTest(s => s
            .SetProjectFile(projectPath)
            .SetConfiguration(configuration)
            .SetVerbosity(DotNetVerbosity.quiet));

        Log.Information("Tests completed.");
        Log.Information(string.Empty);
    }
}
