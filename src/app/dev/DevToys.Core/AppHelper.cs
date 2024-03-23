using System.Reflection;

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
}
