namespace DevToys.Api;

/// <summary>
/// Describes how a child element is vertically positioned or stretched within a parent's layout slot.
/// </summary>
public enum UIVerticalAlignment
{
    /// <summary>
    /// The child element is aligned to the top of the parent's layout slot.
    /// </summary>
    Top,

    /// <summary>
    /// The child element is aligned to the center of the parent's layout slot.
    /// </summary>
    Center,

    /// <summary>
    /// The child element is aligned to the bottom of the parent's layout slot.
    /// </summary>
    Bottom,

    /// <summary>
    /// The child element stretches to fill the parent's layout slot.
    /// </summary>
    Stretch
}
