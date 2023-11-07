namespace DevToys.Api;

/// <summary>
/// End of line character preference.
/// </summary>
public enum UITextEndOfLinePreference
{
    /// <summary>
    /// Use the end of line character identified in the text buffer.
    /// </summary>
    TextDefault = 0,

    /// <summary>
    /// Use line feed (\n) as the end of line character.
    /// </summary>
    LF = 1,

    /// <summary>
    /// Use carriage return and line feed (\r\n) as the end of line character.
    /// </summary>
    CRLF = 2
}
