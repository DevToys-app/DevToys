namespace DevToys.UI.Framework.Controls;

/// <summary>
/// Specifies the position that a <see cref="BackdropWindow"/> will be shown in when it is first opened. Used by the <see cref="BackdropWindow.WindowStartupLocation"/> property.
/// </summary>
public enum WindowStartupLocation
{
    /// <summary>
    /// The startup location of a <see cref="BackdropWindow"/> is the center of the <see cref="BackdropWindow"/> that owns it.
    /// </summary>
    CenterOwner,

    /// <summary>
    /// The startup location of a <see cref="BackdropWindow"/> is the center of the screen that contains the mouse cursor.
    /// </summary>
    CenterScreen,

    /// <summary>
    /// The startup location of a <see cref="BackdropWindow"/> is set from code, or defers to the default OS location.
    /// </summary>
    Manual
}
