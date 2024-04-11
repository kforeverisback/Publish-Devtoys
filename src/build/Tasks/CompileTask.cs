using Core;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Tasks;

internal static class CompileTask
{
    internal static void Run(SubmoduleBase[] submodules, Configuration configuration)
    {
        foreach (SubmoduleBase submodule in submodules)
        {
            foreach (AbsolutePath solution in submodule.GetSolutions())
            {
                BuildSolution(solution, configuration);
            }
        }
    }

    private static void BuildSolution(AbsolutePath solutionPath, Configuration configuration)
    {
        Log.Information("Compiling {Value} solution.", solutionPath);

        Solution solution = solutionPath.ReadSolution();

        DotNetBuild(s => s
            .SetProjectFile(solution)
            .SetConfiguration(configuration)
            .SetVerbosity(DotNetVerbosity.quiet));

        Log.Information("Compilation completed.");
    }
}
