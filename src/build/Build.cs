using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static AppVersion;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

#pragma warning disable IDE1006 // Naming Styles
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The target platform")]
    readonly PlatformTarget[] PlatformTargets;

    [Parameter("https://bit.ly/2OEU0KO - Enabled by default")]
    readonly bool PublishSelfContained = true;

    [Parameter("https://bit.ly/3xvq7FA")]
    readonly bool PublishSingleFile;

    [Parameter("https://bit.ly/3RSEo7w")]
    readonly bool PublishReadyToRun;

    [Parameter("https://bit.ly/3RKZkNH")]
    readonly bool PublishTrimmed;

    [Parameter("Runs unit tests")]
    readonly bool RunTests;

    [Parameter("Do an incremental build")]
    readonly bool IncrementalBuild;

    [Solution]
    readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .OnlyWhenDynamic(() => !IncrementalBuild)
        .Executes(() =>
        {
            RootDirectory.GlobDirectories("bin", "obj", "publish").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetVerbosity(DotNetVerbosity.Quiet));
        });

#pragma warning disable IDE0051 // Remove unused private members
    Target SetVersion => _ => _
        .DependentFor(Compile)
        .After(Restore)
        .OnlyWhenDynamic(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            SetAppVersion(RootDirectory);
        });
#pragma warning restore IDE0051 // Remove unused private members

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            foreach (DotnetParameters dotnetParameters in GetDotnetParameters())
            {
                Log.Information($"Building {dotnetParameters.ProjectOrSolutionPath + "-" + dotnetParameters.TargetFramework + "-" + dotnetParameters.RuntimeIdentifier}...");
                DotNetBuild(s => s
                    .SetProjectFile(dotnetParameters.ProjectOrSolutionPath)
                    .SetConfiguration(Configuration)
                    .SetFramework(dotnetParameters.TargetFramework)
                    .SetRuntime(dotnetParameters.RuntimeIdentifier)
                    .SetSelfContained(dotnetParameters.SelfContained)
                    .SetPublishSingleFile(PublishSingleFile)
                    .SetPublishReadyToRun(PublishReadyToRun)
                    .SetPublishTrimmed(dotnetParameters.PublishTrimmed)
                    .SetNoIncremental(!IncrementalBuild)
                    .SetVerbosity(DotNetVerbosity.Quiet));
            }
        });

#pragma warning disable IDE0051 // Remove unused private members
    Target UnitTests => _ => _
        .DependentFor(Publish)
        .After(Compile)
        .OnlyWhenDynamic(() => RunTests)
        .Executes(() =>
        {
            RootDirectory
                .GlobFiles("**/*Tests.csproj")
                .ForEach(f =>
                    DotNetTest(s => s
                    .SetProjectFile(f)
                    .SetConfiguration(Configuration)
                    .SetVerbosity(DotNetVerbosity.Quiet)));
        });
#pragma warning restore IDE0051 // Remove unused private members

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (DotnetParameters dotnetParameters in GetDotnetParameters())
            {
                Log.Information($"Publishing {dotnetParameters.ProjectOrSolutionPath + "-" + dotnetParameters.TargetFramework + "-" + dotnetParameters.RuntimeIdentifier}...");
                DotNetPublish(s => s
                    .SetProject(dotnetParameters.ProjectOrSolutionPath)
                    .SetConfiguration(Configuration)
                    .SetFramework(dotnetParameters.TargetFramework)
                    .SetRuntime(dotnetParameters.RuntimeIdentifier)
                    .SetSelfContained(dotnetParameters.SelfContained)
                    .SetPublishSingleFile(PublishSingleFile)
                    .SetPublishReadyToRun(PublishReadyToRun)
                    .SetPublishTrimmed(dotnetParameters.PublishTrimmed)
                    .SetVerbosity(DotNetVerbosity.Quiet)
                    .SetOutput(RootDirectory / "publish" / dotnetParameters.ProjectOrSolutionPath.Name + "-" + dotnetParameters.TargetFramework + "-" + dotnetParameters.RuntimeIdentifier));
            }
        });

    IEnumerable<DotnetParameters> GetDotnetParameters()
    {
        PlatformTarget[] platformTargets = PlatformTargets;
        if (PlatformTargets is null || PlatformTargets.Length == 0)
        {
            // If not defined, detect automatically.
            var p = new List<PlatformTarget>
            {
                PlatformTarget.Wasm
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                p.Add(PlatformTarget.MacOS);
                //p.Add(PlatformTarget.iOS);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                p.Add(PlatformTarget.Windows);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                p.Add(PlatformTarget.Linux);
            }

            platformTargets = p.ToArray();
        }

        for (int i = 0; i < platformTargets.Length; i++)
        {
            PlatformTarget platformTarget = platformTargets[i];
            string publishProject;
            Project project;
            switch (platformTarget)
            {
                case PlatformTarget.Wasm:
                    publishProject = "DevToys.Wasm";
                    project = Solution.GetProject(publishProject);
                    foreach (string targetFramework in project.GetTargetFrameworks())
                    {
                        yield return new DotnetParameters(project.Path, runtimeIdentifier: string.Empty, targetFramework, PublishTrimmed, selfContained: false);
                    }
                    break;

                case PlatformTarget.Windows:
                    publishProject = "DevToys.Windows";
                    project = Solution.GetProject(publishProject);
                    foreach (string targetFramework in project.GetTargetFrameworks())
                    {
                        yield return new DotnetParameters(project.Path, "win10-x64", targetFramework, PublishTrimmed, PublishSelfContained);
                        yield return new DotnetParameters(project.Path, "win10-arm64", targetFramework, PublishTrimmed, PublishSelfContained);
                        yield return new DotnetParameters(project.Path, "win10-x86", targetFramework, PublishTrimmed, PublishSelfContained);
                    }
                    break;

                case PlatformTarget.MacOS:
                    throw new NotSupportedException();

                case PlatformTarget.Linux:
                    throw new NotSupportedException();

                default:
                    throw new NotSupportedException();
            }
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
