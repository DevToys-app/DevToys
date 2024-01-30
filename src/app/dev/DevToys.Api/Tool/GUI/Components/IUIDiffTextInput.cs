namespace DevToys.Api;

/// <summary>
/// A component that can be used to display side by side or inlined texts and highlight differences between them.
/// </summary>
public interface IUIDiffTextInput : IUISingleLineTextInput
{
    /// <summary>
    /// Gets the original text displayed to the left of the diff view.
    /// </summary>
    /// <remarks>
    /// This is the same value than <see cref="IUISingleLineTextInput.Text"/>.
    /// </remarks>
    string OriginalText { get; }

    /// <summary>
    /// Gets the modified text displayed to the right of the diff view.
    /// </summary>
    string ModifiedText { get; }

    /// <summary>
    /// Gets whether the text diff control should show differences side by side or inlined.
    /// </summary>
    bool InlineMode { get; }

    /// <summary>
    /// Gets whether the element can be expanded to take the size of the whole tool boundaries.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    bool IsExtendableToFullScreen { get; }

    /// <summary>
    /// Raised when <see cref="ModifiedText"/> is changed.
    /// </summary>
    event EventHandler? ModifiedTextChanged;

    /// <summary>
    /// Raised when <see cref="InlineMode"/> is changed.
    /// </summary>
    event EventHandler? InlineModeChanged;

    /// <summary>
    /// Raised when <see cref="IsExtendableToFullScreen"/> is changed.
    /// </summary>
    event EventHandler? IsExtendableToFullScreenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal class UIDiffTextInput : UISingleLineTextInput, IUIDiffTextInput
{
    private bool _inline;
    private string? _modifiedText;
    private bool _isExtendableToFullScreen;

    internal UIDiffTextInput(string? id)
        : base(id)
    {
    }

    public string OriginalText
    {
        get => Text;
        set => Text = value;
    }

    public string ModifiedText
    {
        get => _modifiedText ?? string.Empty;
        internal set => SetPropertyValue(ref _modifiedText, value, ModifiedTextChanged);
    }

    public bool InlineMode
    {
        get => _inline;
        internal set => SetPropertyValue(ref _inline, value, InlineModeChanged);
    }

    public bool IsExtendableToFullScreen
    {
        get => _isExtendableToFullScreen;
        internal set => SetPropertyValue(ref _isExtendableToFullScreen, value, IsExtendableToFullScreenChanged);
    }

    public event EventHandler? ModifiedTextChanged;
    public event EventHandler? InlineModeChanged;
    public event EventHandler? IsExtendableToFullScreenChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display side by side or inlined texts and highlight differences between them.
    /// </summary>
    public static IUIDiffTextInput DiffTextInput()
    {
        return DiffTextInput(null);
    }

    /// <summary>
    /// A component that can be used to display side by side or inlined texts and highlight differences between them.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIDiffTextInput DiffTextInput(string? id)
    {
        return new UIDiffTextInput(id);
    }

    /// <summary>
    /// Sets the unformatted original text of the control to compare with the modified one.
    /// </summary>
    public static T OriginalText<T>(this T element, string text) where T : IUIDiffTextInput
    {
        if (element is UIDiffTextInput strongElement)
        {
            strongElement.OriginalText = text;
        }
        return element;
    }

    /// <summary>
    /// Sets the unformatted modified text of the control to compare with the original one.
    /// </summary>
    public static T ModifiedText<T>(this T element, string text) where T : IUIDiffTextInput
    {
        if (element is UIDiffTextInput strongElement)
        {
            strongElement.ModifiedText = text;
        }
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDiffTextInput.InlineMode"/> to true.
    /// </summary>
    public static IUIDiffTextInput InlineView(this IUIDiffTextInput element)
    {
        ((UIDiffTextInput)element).InlineMode = true;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDiffTextInput.InlineMode"/> to false.
    /// </summary>
    public static IUIDiffTextInput SplitView(this IUIDiffTextInput element)
    {
        ((UIDiffTextInput)element).InlineMode = false;
        return element;
    }

    /// <summary>
    /// Indicates that the control can be extended to take the size of the whole tool boundaries.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    public static IUIDiffTextInput Extendable(this IUIDiffTextInput element)
    {
        ((UIDiffTextInput)element).IsExtendableToFullScreen = true;
        return element;
    }

    /// <summary>
    /// Indicates that the control can not be extended to take the size of the whole tool boundaries.
    /// </summary>
    public static IUIDiffTextInput NotExtendable(this IUIDiffTextInput element)
    {
        ((UIDiffTextInput)element).IsExtendableToFullScreen = false;
        return element;
    }
}
