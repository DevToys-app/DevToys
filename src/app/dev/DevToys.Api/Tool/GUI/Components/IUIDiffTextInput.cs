namespace DevToys.Api;

/// <summary>
/// A component that can be used to display side by side or inlined texts and highlight differences between them.
/// This component is powered by Monaco Editor.
/// </summary>
public interface IUIDiffTextInput : IUISinglelineTextInput
{
    /// <summary>
    /// Gets the text displayed to the right of the diff view.
    /// </summary>
    string RightText { get; }

    /// <summary>
    /// Gets whether the text diff control should show differences side by side or inlined.
    /// </summary>
    bool InlineMode { get; }

    /// <summary>
    /// Raised when <see cref="RightText"/> is changed.
    /// </summary>
    event EventHandler? RightTextChanged;

    /// <summary>
    /// Raised when <see cref="InlineMode"/> is changed.
    /// </summary>
    event EventHandler? InlineModeChanged;
}
