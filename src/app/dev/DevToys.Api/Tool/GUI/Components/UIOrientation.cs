namespace DevToys.Api;

/// <summary>
/// Defines the different orientations that an element or layout can have.
/// </summary>
[Flags]
public enum UIOrientation
{
    /// <summary>
    /// Element or layout should be horizontally oriented.
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Element or layout should be vertically oriented.
    /// </summary>
    Vertical = 2
}
