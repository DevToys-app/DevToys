namespace DevToys.Api;

/// <summary>
/// Describes how a text in an element should wrap when it reached the maximum horizontal space it can take.
/// </summary>
public enum UITextWrapMode
{
    /// <summary>
    /// Wrap automatically or not depending on the user's settings.
    /// </summary>
    Auto,

    /// <summary>
    /// Wrap the text to a new line.
    /// </summary>
    Wrap,

    /// <summary>
    /// Do not wrap the text to a new line.
    /// </summary>
    NoWrap
}
