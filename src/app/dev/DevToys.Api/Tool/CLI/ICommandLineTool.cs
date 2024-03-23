using Microsoft.Extensions.Logging;

namespace DevToys.Api;

/// <summary>
/// Represents the factory for command line tool.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(ICommandLineTool))]
///         [Name("Base64 Encode / Decoder")]
///         [CommandName(
///             Name = "base64",
///             Alias = "b64",
///             DescriptionResourceName = nameof(Strings.Base64Description),
///             ResourceManagerBaseName = "MyProject.Strings")]
///         [TargetPlatform(Platform.Windows)]  // Optional
///         [TargetPlatform(Platform.MacOS)]    // Optional
///         internal sealed class Base64CommandLineTool : ICommandLineTool
///         {
///             [CommandLineOption(Name = "file", Alias = "f", IsRequired = true, DescriptionResourceName = nameof(Strings.Base64FileOptionDescription))]
///             internal FileInfo? File { get; set; }
///             
///             [CommandLineOption(Name = "utf8", DescriptionResourceName = nameof(Strings.Utf8OptionDescription))]
///             internal bool Utf8 { get; set; } = true; // Default value is true.
///             
///             public ValueTask&lt;int&gt; InvokeAsync(ILogger logger, CancellationToken cancellationToken)
///             {
///                 // [...]
///                 return 0; // Exit code.
///             }
///         }
///     </code>
/// </example>
/// </remarks>
public interface ICommandLineTool
{
    /// <summary>
    /// Invoked when the user ran the app using the command and options defined by the current <see cref="ICommandLineTool"/>.
    /// </summary>
    /// <param name="logger">A logger, for reporting relevant telemetry information about health and performance of the tool.</param>
    /// <param name="cancellationToken">Gets canceled when the user wants to quit the app.</param>
    /// <returns>An Exit Code.</returns>
    /// <remarks>
    /// Using <paramref name="logger"/>:
    /// - DO report errors.
    /// - DO report information about performance of some tasks, if relevant.
    /// - DO report some system information, but only if it can truly be helpful when investigating performance or compatibility issues.
    /// - DO NOT report what the user input in the app as it might contains user personal information.
    /// </remarks>
    ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken);
}
