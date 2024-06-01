namespace DevToys.Api;

/// <summary>
/// Describes how the line number should be displayed in the text editor.
/// </summary>
public enum UITextLineNumber
{
    /// <summary>
    /// Automatically show or hide the line number depending on the user's settings.
    /// </summary>
    Auto,

    /// <summary>
    /// Show the line number.
    /// </summary>
    Show,

    /// <summary>
    /// Do not show the line number.
    /// </summary>
    Hide
}
