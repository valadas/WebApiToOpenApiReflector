using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "PR Validation",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] {"*"},
    OnPushBranches = new[] {"main", "develop"},
    InvokedTargets = new[] {"Compile"},
    FetchDepth = 0,
    JobConcurrencyCancelInProgress = true,
    PublishArtifacts = true
)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion] readonly GitVersion GitVersion;

    [Solution] readonly Solution Solution;

    private Project project => Solution.GetProject("Tool");

    private AbsolutePath artifactsDirectory => RootDirectory / "artifacts";
    private AbsolutePath nugetDirectory => RootDirectory / "nuget";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            artifactsDirectory.CreateOrCleanDirectory();
            nugetDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Produces(artifactsDirectory)
        .Executes(() =>
        {
            if (IsLocalBuild)
            {
                DotNetPublish(s => s
                    .SetProject(project)
                    .SetConfiguration(Configuration)
                    .SetNoRestore(true)
                    .SetAssemblyVersion(GitVersion.MajorMinorPatch)
                    .SetFileVersion(GitVersion.MajorMinorPatch)
                    .SetInformationalVersion(GitVersion.FullSemVer)
                    .SetOutput(artifactsDirectory));
            }
            else
            {
                var runtimeIdentifiers = new List<string>
                {
                    "win-x64",
                    "linux-x64",
                    "osx-x64"
                };

                runtimeIdentifiers.ForEach(runtimeIdentifier =>
                {
                    DotNetPublish(s => s
                        .SetProject(project)
                        .SetConfiguration(Configuration)
                        .SetAssemblyVersion(GitVersion.MajorMinorPatch)
                        .SetFileVersion(GitVersion.MajorMinorPatch)
                        .SetInformationalVersion(GitVersion.FullSemVer)
                        .EnablePublishSingleFile()
                        .SetRuntime(runtimeIdentifier)
                        .SetOutput(artifactsDirectory / runtimeIdentifier));
                });
            }
        });

    Target Publish => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.MajorMinorPatch)
                .SetFileVersion(GitVersion.MajorMinorPatch)
                .SetInformationalVersion(GitVersion.FullSemVer)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetOutputDirectory(nugetDirectory));
        });
}
