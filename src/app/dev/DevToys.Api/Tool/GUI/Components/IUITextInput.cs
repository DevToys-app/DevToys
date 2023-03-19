namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit unformatted text.
/// </summary>
public interface IUITextInput : IUITitledElement
{
    /// <summary>
    /// Gets whether the user can edit the text or not.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets whether the `Copy` button should be displayed on top of the editor, even when it is not read-only.
    /// </summary>
    bool CanCopyWhenEditable { get; }

    /// <summary>
    /// Gets the text displayed or typed by the user.
    /// </summary>
    bool Text { get; }

    /// <summary>
    /// Raised when <see cref="Text"/> is changed.
    /// </summary>
    event EventHandler? TextChanged;
}
