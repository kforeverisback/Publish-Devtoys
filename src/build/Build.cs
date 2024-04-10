using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using Tasks;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

internal class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    private readonly GitRepository Repository;

    public Target Clean => _ => _
        .Executes(() =>
        {
            RootDirectory
                .GlobDirectories("bin", "obj", "packages", "publish")
                .ForEach(path => path.CreateOrCleanDirectory());
            Log.Information("Cleaned bin, obj, packages and publish folders.");
        });

    public Target UpdateSubmodules => _ => _
        .DependsOn(Clean)
        .Description("Update submodules.")
        .Executes(async () =>
        {
            Log.Information("Commit = {Value}", Repository.Commit);
            Log.Information("Branch = {Value}", Repository.Branch);
            Log.Information("Tags = {Value}", Repository.Tags);

            Log.Information("main branch = {Value}", Repository.IsOnMainBranch());
            Log.Information("release/* branch = {Value}", Repository.IsOnReleaseBranch());
            Log.Information("hotfix/* branch = {Value}", Repository.IsOnHotfixBranch());

            Log.Information("Https URL = {Value}", Repository.HttpsUrl);
            Log.Information("SSH URL = {Value}", Repository.SshUrl);

            await GitTask.UpdateSubmodulesAsync();
        });

    public Target RestoreDependencies => _ => _
        .DependsOn(UpdateSubmodules)
        .Description("Restore various dependencies.")
        .Executes(
            () => InitScriptTask.RunAsync(RootDirectory));

    public Target Compile => _ => _
        .DependsOn(RestoreDependencies)
        .Description("Build solutions.")
        .Executes(() =>
        {
        });

    public Target RunTests => _ => _
        .DependsOn(Compile)
        .Description("Run tests.")
        .Executes(() =>
        {
        });

    public Target Publish => _ => _
        .DependsOn(RunTests)
        .Description(description: "Generate publish artifacts.")
        .Executes(() =>
        {
        });
}
