using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using System.Text.RegularExpressions;

[GitHubActions(
    "PR Validation",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] {"*"},
    OnPushBranches = new[] {"main", "develop"},
    InvokedTargets = new[] {nameof(Compile)},
    FetchDepth = 0,
    JobConcurrencyCancelInProgress = true,
    PublishArtifacts = true
)]
[GitHubActions(
    "Release",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] {"main", "release/*"},
    InvokedTargets = new[] {nameof(ReleaseToGithub), nameof(ReleaseToNuget) },
    ImportSecrets = new[] {nameof(GitHubToken), nameof(NugetApiKey)},
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

    [Parameter("GitHub Token")]
    readonly string GitHubToken;

    [Parameter("Nuget API Key")]
    readonly string NugetApiKey;

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    [Solution] readonly Solution Solution;

    private Nuke.Common.ProjectModel.Project project => Solution.GetProject("Tool");

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

    Target CreateNugetPackage => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Docs)
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

    Target TagRelease => _ => _
    .Executes(() =>
    {
        if (GitRepository.IsOnMainOrMasterBranch())
        {
            Git($"tag v{GitVersion.MajorMinorPatch}");
            Git($"push origin v{GitVersion.MajorMinorPatch}");
        }
        else if (GitRepository.IsOnReleaseBranch())
        {
            Git($"tag v{GitVersion.SemVer}");
            Git($"push origin v{GitVersion.SemVer}");
        }
    });

    Target ReleaseToGithub => _ => _
        .OnlyWhenDynamic(() => GitRepository != null && (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch()))
        .Requires(() => GitHubToken)
        .DependsOn(TagRelease)
        .DependsOn(Compile)
        .Executes(async () =>
        {
            var releaseNotes = GeneratedReleaseNotes();
            var newRelease = new NewRelease(GitRepository.IsOnMainOrMasterBranch() ? $"v{GitVersion.MajorMinorPatch}" : $"v{GitVersion.SemVer}")
            {
                Body = releaseNotes,
                Draft = true,
                Name = GitRepository.IsOnMainOrMasterBranch() ? $"v{GitVersion.MajorMinorPatch}" : $"v{GitVersion.SemVer}",
                TargetCommitish = GitVersion.Sha,
                Prerelease = GitRepository.IsOnReleaseBranch(),
            };
            GitHubTasks.GitHubClient.Credentials = new Credentials(GitHubToken.NotNull());
            var release = await GitHubTasks.GitHubClient.Repository.Release.Create(
                GitRepository.GetGitHubOwner(),
                GitRepository.GetGitHubName(),
                newRelease);
            Serilog.Log.Information($"{release.Name} released !");

            artifactsDirectory.ZipTo(artifactsDirectory / "release.zip");
            var file = artifactsDirectory / "release.zip";
            var artifact = File.OpenRead(file);
            var artifactInfo = new FileInfo(file);
            var assetUpload = new ReleaseAssetUpload()
            {
                FileName = artifactInfo.Name,
                ContentType = "application/zip",
                RawData = artifact
            };
            var asset = await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, assetUpload);
            Serilog.Log.Information($"Asset {asset.Name} published at {asset.BrowserDownloadUrl}");
        });

    Target ReleaseToNuget => _ => _
        .Requires(() => NugetApiKey)
        .DependsOn(CreateNugetPackage)
        .Executes(() =>
        {
            var package = nugetDirectory.GlobFiles("*.nupkg").First();
            NuGetPush(s => s
                .SetApiKey(NugetApiKey)
                .SetTargetPath(package)
                .SetSource("https://api.nuget.org/v3/index.json")
            );
        });

    Target Docs => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => IsLocalBuild)
        .Executes(() =>
        {
            var output = DotNetRun(s => s
                .SetProjectFile(project)
                .SetConfiguration(Configuration)
                .SetNoRestore(true)
                .SetNoBuild(true)
                .SetProcessExitHandler(_ => Serilog.Log.Information("Run completed"))
                .SetApplicationArguments("--help")
            );
            var readme = (RootDirectory / "README.md").ReadAllText();
            var pattern = @"(?<=<!--- BEGIN_TOOL_DOCS ---\>)(.*?)(?=<!--- END_TOOL_DOCS --->)";
            var newText = new StringBuilder()
                .AppendLine()
                .AppendLine("```");
            output.ForEach(line => newText.AppendLine(line.Text));
            newText.AppendLine("```");
            var newReadme = Regex.Replace(readme, pattern, newText.ToString(), RegexOptions.Singleline);
            (RootDirectory / "README.md").WriteAllText(newReadme);
        });

    private string GeneratedReleaseNotes()
    {
        string releaseNotes;

        // Get the milestone
        var milestone = GitHubTasks.GitHubClient.Issue.Milestone.GetAllForRepository(
            GitRepository.GetGitHubOwner(),
            GitRepository.GetGitHubName()).Result
            .Where(m => m.Title == GitVersion.MajorMinorPatch).FirstOrDefault();
        Serilog.Log.Information(milestone.ToJson());
        if (milestone == null)
        {
            Serilog.Log.Warning("Milestone not found for this version");
            releaseNotes = "No release notes for this version.";
            return releaseNotes;
        }

        try
        {
            // Get the PRs
            var prRequest = new PullRequestRequest()
            {
                State = ItemStateFilter.All
            };
            var allPrs = Task.Run(() =>
                GitHubTasks.GitHubClient.Repository.PullRequest.GetAllForRepository(
                        GitRepository.GetGitHubOwner(),
                    GitRepository.GetGitHubName(), prRequest)
            ).Result;

            var pullRequests = allPrs.Where(p =>
                p.Milestone?.Title == milestone.Title &&
                p.Merged == true &&
                p.Milestone?.Title == GitVersion.MajorMinorPatch);
            Serilog.Log.Information(pullRequests.ToJson());

            // Build release notes
            var releaseNotesBuilder = new StringBuilder();
            releaseNotesBuilder
                .AppendLine($"# {GitRepository.GetGitHubName()} {milestone.Title}")
                .AppendLine()
                .AppendLine($"A total of {pullRequests.Count()} pull requests where merged in this release.")
                .AppendLine();

            foreach (var group in pullRequests.GroupBy(p => p.Labels[0]?.Name, (label, prs) => new { label, prs }))
            {
                Serilog.Log.Information(group.ToJson());
                releaseNotesBuilder.AppendLine($"## {group.label}");
                foreach (var pr in group.prs)
                {
                    Serilog.Log.Information(pr.ToJson());
                    releaseNotesBuilder.AppendLine($"- #{pr.Number} {pr.Title}. Thanks @{pr.User.Login}");
                }
            }

            releaseNotes = releaseNotesBuilder.ToString();
            Serilog.Log.Information(releaseNotes);
            return releaseNotes;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Something went wrong with the github api call.");
            throw;
        }
    }
}
