using System.Reflection;
using System.Text.Json;
using DevToys.Core.Models;
using DevToys.Core.Version;
using DevToys.Core.Web;
using Microsoft.Extensions.Logging;

namespace DevToys.Core;

public static class AppHelper
{
    /// <summary>
    /// Indicates whether the current instance of the app is a preview/beta version.
    /// </summary>
    public static readonly Lazy<bool> IsPreviewVersion = new(() =>
    {
        var assemblyInformationalVersion
            = (AssemblyInformationalVersionAttribute)
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))!;
        return assemblyInformationalVersion.InformationalVersion.Contains("pre", StringComparison.CurrentCultureIgnoreCase);
    });

    /// <summary>
    /// Gets the value of the specified command line argument.
    /// </summary>
    /// <param name="argumentName">The name of the argument. The name will be interpreted as "--name:". The string should not contains "--" and ":".</param>
    /// <returns>The argument value or <see cref="string.Empty"/> if not found.</returns>
    /// <remarks>
    /// This method will search for the argument in the command line arguments. The search is case-insensitive.
    /// Arguments must be like `--name:value` or `--name: value` or `--name: "value with space"`.
    /// </remarks>
    public static string GetCommandLineArgument(string argumentName)
    {
        return GetCommandLineArgument(Environment.GetCommandLineArgs(), argumentName);
    }

    /// <summary>
    /// Gets the value of the specified command line argument.
    /// </summary>
    /// <param name="arguments">The list of command line arguments</param>
    /// <param name="argumentName">The name of the argument. The name will be interpreted as "--name:". The string should not contains "--" and ":".</param>
    /// <returns>The argument value or <see cref="string.Empty"/> if not found.</returns>
    /// <remarks>
    /// This method will search for the argument in the command line arguments. The search is case-insensitive.
    /// Arguments must be like `--name:value` or `--name: value` or `--name: "value with space"`.
    /// </remarks>
    public static string GetCommandLineArgument(string[] arguments, string argumentName)
    {
        Guard.IsNotNull(arguments);
        Guard.IsNotNullOrWhiteSpace(argumentName);

        argumentName = $"--{argumentName}:";
        for (int i = 0; i < arguments.Length; i++)
        {
            string argument = arguments[i];
            if (argument.StartsWith(argumentName, StringComparison.OrdinalIgnoreCase))
            {
                if (argument.Length == argumentName.Length)
                {
                    if (i + 1 < arguments.Length)
                    {
                        return arguments[i + 1].Trim('\"');
                    }

                    return string.Empty;
                }
                else
                {
                    return argument.Substring(argumentName.Length).Trim('\"');
                }
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Checks whether there is a newer version on GitHub.
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> CheckForUpdateAsync(IWebClientService webClientService, IVersionService versionService, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(webClientService);

        // We do not use Octokit (GitHub SDK) because it's heavy. We'd need to add 1.3 MB to the app size just for this query
        // simple query without token.
        string? releaseListJson
            = await webClientService.SafeGetStringAsync(
                new Uri("https://api.github.com/repos/DevToys-app/DevToys/releases"),
                cancellationToken);

        if (!string.IsNullOrEmpty(releaseListJson))
        {
            try
            {
                List<GitHubRelease>? releases = JsonSerializer.Deserialize<List<GitHubRelease>>(releaseListJson);

                // Get the first release that is not a draft and is a preview
                // version (depending on whether the running DevToys is a preview or not).
                GitHubRelease? potentialRelease
                    = releases?.FirstOrDefault(
                        release => !release.Draft && release.PreRelease == versionService.IsPreviewVersion());

                if (potentialRelease is not null)
                {
                    var assemblyInformationalVersion
                        = (AssemblyInformationalVersionAttribute)Assembly
                            .GetExecutingAssembly()
                            .GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))!;

                    string currentVersion
                        = assemblyInformationalVersion.InformationalVersion
                            .TrimStart('v')
                            .Replace("-alpha", string.Empty)
                            .Replace("-beta", string.Empty)
                            .Replace("-preview", string.Empty)
                            .Replace("-pre", string.Empty);
                    string? releaseVersion
                        = potentialRelease.Name?
                            .TrimStart('v')
                            .Replace("-alpha", string.Empty)
                            .Replace("-beta", string.Empty)
                            .Replace("-preview", string.Empty)
                            .Replace("-pre", string.Empty);

                    if (!string.IsNullOrEmpty(releaseVersion) && !string.IsNullOrEmpty(currentVersion))
                    {
                        if (new System.Version(releaseVersion) > new System.Version(currentVersion))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ILogger logger = typeof(AppHelper).Log();
                logger.LogError(ex, "Error while checking for updates.");
            }
        }

        return false;
    }
}
