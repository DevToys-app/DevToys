using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static AppVersionTask;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static RestoreTask;
using Project = Nuke.Common.ProjectModel.Project;

#pragma warning disable IDE1006 // Naming Styles
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Release'")]
    readonly Configuration Configuration = Configuration.Release;

    [Parameter("The target platform")]
    readonly PlatformTarget[]? PlatformTargets;

    [Parameter("Runs unit tests")]
    readonly bool RunTests;

    [Solution]
    readonly Solution? WindowsSolution;

    [Solution]
    readonly Solution? MacSolution;

    Target PreliminaryCheck => _ => _
        .Before(Clean)
        .Executes(() =>
        {
            if (PlatformTargets == null || PlatformTargets.Length == 0)
            {
                Assert.Fail("Parameter `--platform-targets` is missing. Please check `build.sh --help`.");
                return;
            }

            if (PlatformTargets.Contains(PlatformTarget.Windows) && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 0, 0))
            {
                Assert.Fail("To build Windows UWP app, you need to run on Windows 10 or later.");
                return;
            }

            if (PlatformTargets.Contains(PlatformTarget.MacOS) && !OperatingSystem.IsMacOS())
            {
                Assert.Fail("To build macOS app, you need to run on macOS Ventura 13.1 or later.");
                return;
            }

            Log.Information("Preliminary checks are successful.");
        });

    Target Clean => _ => _
        .DependsOn(PreliminaryCheck)
        .Executes(() =>
        {
            if (!Debugger.IsAttached)
            {
                RootDirectory.GlobDirectories("bin", "obj", "publish").ForEach(EnsureCleanDirectory);
            }
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(async () =>
        {
            if (!Debugger.IsAttached)
            {
                await RestoreDependenciesAsync(RootDirectory);
            }
        });

    Target SetVersion => _ => _
        .DependentFor(Publish)
        .After(Restore)
        .OnlyWhenDynamic(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            SetAppVersion(RootDirectory);
        });

#pragma warning disable IDE0051 // Remove unused private members
    Target UnitTests => _ => _
        .DependentFor(Publish)
        .After(Restore)
        .After(SetVersion)
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
        .DependsOn(SetVersion)
        .DependsOn(Restore)
        .Executes(() =>
        {
            if (PlatformTargets!.Contains(PlatformTarget.Windows))
            {
                // DevToys MAUI Blazor
                foreach (DotnetParameters dotnetParameters in GetDotnetParametersForMauiBlazorApp())
                {
                    Log.Information($"Publishing {dotnetParameters.ProjectOrSolutionPath + " - " + dotnetParameters.TargetFramework + " - " + dotnetParameters.RuntimeIdentifier} ...");
                    DotNetPublish(s => s
                        .SetProject(dotnetParameters.ProjectOrSolutionPath)
                        .SetConfiguration(Configuration)
                        .SetFramework(dotnetParameters.TargetFramework)
                        .SetRuntime(dotnetParameters.RuntimeIdentifier)
                        .SetSelfContained(true)
                        .SetPublishSingleFile(false)
                        .SetPublishReadyToRun(false)
                        .SetPublishTrimmed(false) // TODO?
                        .SetVerbosity(DotNetVerbosity.Quiet)
                        .SetProcessArgumentConfigurator(_ => _
                            .Add("/p:RuntimeIdentifierOverride=" + dotnetParameters.RuntimeIdentifier))
                        .SetOutput(RootDirectory / "publish" / dotnetParameters.OutputPath));
                }

                // DevToys WASDK
                foreach (DotnetParameters dotnetParameters in GetDotnetParametersForWasdkApp())
                {
                    Log.Information($"Publishing {dotnetParameters.ProjectOrSolutionPath + " - " + dotnetParameters.TargetFramework + " - " + dotnetParameters.RuntimeIdentifier + (dotnetParameters.Portable ? "-Portable" : string.Empty)} ...");
                    DotNetPublish(s => s
                        .SetProject(dotnetParameters.ProjectOrSolutionPath)
                        .SetConfiguration(Configuration)
                        .SetFramework(dotnetParameters.TargetFramework)
                        .SetRuntime(dotnetParameters.RuntimeIdentifier)
                        .SetPlatform(dotnetParameters.Platform)
                        .SetSelfContained(true)
                        .SetPublishSingleFile(false)
                        .SetPublishReadyToRun(false)
                        .SetPublishTrimmed(false) // TODO?
                        .SetVerbosity(DotNetVerbosity.Quiet)
                        .SetProcessArgumentConfigurator(_ => _
                            .Add("/p:RuntimeIdentifierOverride=" + dotnetParameters.RuntimeIdentifier)
                            .Add("/p:Unpackaged=" + dotnetParameters.Portable))
                        .SetOutput(RootDirectory / "publish" / dotnetParameters.OutputPath));
                }
            }

            if (PlatformTargets!.Contains(PlatformTarget.MacOS))
            {
                // DevToys Uno Mac Catalyst and MAUI Blazor
                Project[] projects = {
                    MacSolution!.GetProject("DevToys.MacOS"),
                    MacSolution.GetProject("DevToys.MauiBlazor")
                };

                foreach (Project project in projects)
                {
                    foreach (DotnetParameters dotnetParameters in GetDotnetParametersForMauiBlazorApp())
                    {
                        Log.Information($"Publishing {dotnetParameters.ProjectOrSolutionPath + " - " + dotnetParameters.TargetFramework + " - " + dotnetParameters.RuntimeIdentifier} ...");
                        DotNetBuild(s => s
                            .SetProjectFile(dotnetParameters.ProjectOrSolutionPath)
                            .SetConfiguration(Configuration)
                            .SetPublishSingleFile(false) // Not supported by MacCatalyst as it would require UseAppHost to be true, which isn't supported on Mac
                            .SetPublishReadyToRun(false)
                            .SetPublishTrimmed(true) /* Required for MacCatalyst*/
                            .SetVerbosity(DotNetVerbosity.Quiet)
                            .SetNoRestore(true) /* workaround for https://github.com/xamarin/xamarin-macios/issues/15664#issuecomment-1233123515 */
                            .SetProcessArgumentConfigurator(_ => _
                                .Add("/p:RuntimeIdentifierOverride=" + dotnetParameters.RuntimeIdentifier)
                                .Add("/p:CreatePackage=True")) /* Will create an installable .pkg */
                            .SetOutputDirectory(RootDirectory / "publish" / dotnetParameters.OutputPath));
                    }
                }
            }

            if (PlatformTargets!.Contains(PlatformTarget.CLI))
            {
                // DevToys CLI
                foreach (DotnetParameters dotnetParameters in GetDotnetParametersForCliApp())
                {
                    Log.Information($"Publishing {dotnetParameters.ProjectOrSolutionPath + " - " + dotnetParameters.TargetFramework + " - " + dotnetParameters.RuntimeIdentifier} ...");
                    DotNetPublish(s => s
                        .SetProject(dotnetParameters.ProjectOrSolutionPath)
                        .SetConfiguration(Configuration)
                        .SetFramework(dotnetParameters.TargetFramework)
                        .SetRuntime(dotnetParameters.RuntimeIdentifier)
                        .SetSelfContained(dotnetParameters.Portable)
                        .SetPublishSingleFile(true)
                        .SetPublishReadyToRun(true)
                        .SetPublishTrimmed(dotnetParameters.Portable)
                        .SetVerbosity(DotNetVerbosity.Quiet)
                        .SetOutput(RootDirectory / "publish" / dotnetParameters.OutputPath));
                }
            }
        });

    IEnumerable<DotnetParameters> GetDotnetParametersForCliApp()
    {
        string publishProject = "DevToys.CLI";
        Project project;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            project = MacSolution!.GetProject(publishProject);
            foreach (string targetFramework in project.GetTargetFrameworks())
            {
                yield return new DotnetParameters(project.Path, "osx-x64", targetFramework, portable: false);
                yield return new DotnetParameters(project.Path, "osx-x64", targetFramework, portable: true);

                yield return new DotnetParameters(project.Path, "osx-arm64", targetFramework, portable: false);
                yield return new DotnetParameters(project.Path, "osx-arm64", targetFramework, portable: true);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            project = WindowsSolution!.GetProject(publishProject);
            foreach (string targetFramework in project.GetTargetFrameworks())
            {
                yield return new DotnetParameters(project.Path, "win10-x64", targetFramework, portable: false);
                yield return new DotnetParameters(project.Path, "win10-x64", targetFramework, portable: true);

                yield return new DotnetParameters(project.Path, "win10-arm64", targetFramework, portable: false);
                yield return new DotnetParameters(project.Path, "win10-arm64", targetFramework, portable: true);

                yield return new DotnetParameters(project.Path, "win10-x86", targetFramework, portable: false);
                yield return new DotnetParameters(project.Path, "win10-x86", targetFramework, portable: true);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // TODO
        }
    }

    IEnumerable<DotnetParameters> GetDotnetParametersForWasdkApp()
    {
        string publishProject = "DevToys.Wasdk";
        Project project;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            project = WindowsSolution!.GetProject(publishProject);
            foreach (string targetFramework in project.GetTargetFrameworks())
            {
                yield return new DotnetParameters(project.Path, "win10-arm64", targetFramework, portable: false, platform: "arm64");
                yield return new DotnetParameters(project.Path, "win10-x64", targetFramework, portable: false, platform: "x64");
                yield return new DotnetParameters(project.Path, "win10-x86", targetFramework, portable: false, platform: "x86");

                yield return new DotnetParameters(project.Path, "win10-arm64", targetFramework, portable: true, platform: "arm64");
                yield return new DotnetParameters(project.Path, "win10-x64", targetFramework, portable: true, platform: "x64");
                yield return new DotnetParameters(project.Path, "win10-x86", targetFramework, portable: true, platform: "x86");
            }
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    IEnumerable<DotnetParameters> GetDotnetParametersForMauiBlazorApp()
    {
        string publishProject = "DevToys.MauiBlazor";
        Project project;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            project = MacSolution!.GetProject(publishProject);
            foreach (string targetFramework in project.GetTargetFrameworks())
            {
                yield return new DotnetParameters(project.Path, "maccatalyst-arm64", targetFramework, portable: true);
                yield return new DotnetParameters(project.Path, "maccatalyst-x64", targetFramework, portable: true);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            project = WindowsSolution!.GetProject(publishProject);
            foreach (string targetFramework in project.GetTargetFrameworks())
            {
                yield return new DotnetParameters(project.Path, "win10-arm64", targetFramework, portable: true);
                yield return new DotnetParameters(project.Path, "win10-x64", targetFramework, portable: true);
                yield return new DotnetParameters(project.Path, "win10-x86", targetFramework, portable: true);
            }
        }
        else
        {
            throw new NotSupportedException();
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
