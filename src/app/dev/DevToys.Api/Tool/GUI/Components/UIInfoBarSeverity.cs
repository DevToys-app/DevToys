namespace DevToys.Api;

/// <summary>
/// Represents the severity level of a <see cref="IUIInfoBar"/>.
/// </summary>
public enum UIInfoBarSeverity
{
    /// <summary>
    /// Informational severity.
    /// </summary>
    Informational,

    /// <summary>
    /// Error severity.
    /// </summary>
    Error,

    /// <summary>
    /// Success severity.
    /// </summary>
    Success,

    /// <summary>
    /// Warning severity.
    /// </summary>
    Warning
}
