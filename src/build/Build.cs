﻿using Core;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Serilog;
using Tasks;

internal class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [GitRepository]
    private readonly GitRepository Repository;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private SubmoduleBase[] Submodules { get; } =
    {
        new Submodules.DevToysTools.DevToysToolsSubmodule(RootDirectory),
        new Submodules.DevToys.DevToysSubmodule(RootDirectory)
    };

    private AbsolutePath AssetsDirectory { get; } = RootDirectory / "assets";

    public Target Clean => _ => _
        .Executes(
            () => CleanTask.Run(RootDirectory, Submodules));

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

    public Target Restore => _ => _
        .DependsOn(UpdateSubmodules)
        .Description("Restore dependencies.")
        .Executes(
            () => RestoreTask.RunAsync(Submodules));

    public Target Compile => _ => _
        .DependsOn(Restore)
        .Description("Build solutions.")
        .Executes(
            () => CompileTask.Run(Submodules, Configuration));

    public Target RunTests => _ => _
        .DependsOn(Compile)
        .Description("Run tests.")
        .Executes(
            () => TestTask.Run(Submodules, Configuration));

    public Target CompilePublishBits => _ => _
        .DependsOn(RunTests)
        .Description(description: "Generate publish artifacts.")
        .Executes(
            () => CompilePublishBinariesTask.RunAsync(RootDirectory, AssetsDirectory, Submodules, Configuration));

    public Target Pack => _ => _
        .DependsOn(CompilePublishBits)
        .Description(description: "Generate packages & installers.")
        .Executes(
             () => PackPublishBinariesTask.RunAsync(RootDirectory, Submodules, Configuration));
}
