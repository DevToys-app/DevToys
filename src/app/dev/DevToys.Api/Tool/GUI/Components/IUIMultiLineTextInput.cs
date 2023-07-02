namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit unformatted text on multiple lines.
/// </summary>
public interface IUIMultiLineTextInput : IUISingleLineTextInput
{
    /// <summary>
    /// Gets the list of spans to highlight in the text document.
    /// </summary>
    IReadOnlyList<TextSpan> HighlightedSpans { get; }

    /// <summary>
    /// Gets the programming language name to use when colorizing the text in the control.
    /// </summary>
    string SyntaxColorizationLanguageName { get; }

    /// <summary>
    /// Gets whether the element can be expanded to take the size of the whole tool boundaries.
    /// </summary>
    bool IsExtendableToFullScreen { get; }

    /// <summary>
    /// Raised when <see cref="HighlightedSpans"/> is changed.
    /// </summary>
    event EventHandler? HighlightedSpansChanged;

    /// <summary>
    /// Raised when <see cref="SyntaxColorizationLanguageName"/> is changed.
    /// </summary>
    event EventHandler? SyntaxColorizationLanguageNameChanged;

    /// <summary>
    /// Raised when <see cref="IsExtendableToFullScreen"/> is changed.
    /// </summary>
    event EventHandler? IsExtendableToFullScreenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}, SyntaxColorizationLanguageName = {{{nameof(SyntaxColorizationLanguageName)}}}")]
internal class UIMultilineTextInput : UISingleLineTextInput, IUIMultiLineTextInput
{
    private IReadOnlyList<TextSpan>? _highlightedSpans;
    private string? _syntaxColorizationLanguageName;
    private bool _isExtendableToFullScreen;

    internal UIMultilineTextInput(string? id)
        : base(id)
    {
    }

    public IReadOnlyList<TextSpan> HighlightedSpans
    {
        get => _highlightedSpans ?? Array.Empty<TextSpan>();
        internal set => SetPropertyValue(ref _highlightedSpans, value, HighlightedSpansChanged);
    }

    public string SyntaxColorizationLanguageName
    {
        get => _syntaxColorizationLanguageName ?? string.Empty;
        internal set => SetPropertyValue(ref _syntaxColorizationLanguageName, value, SyntaxColorizationLanguageNameChanged);
    }

    public bool IsExtendableToFullScreen
    {
        get => _isExtendableToFullScreen;
        internal set => SetPropertyValue(ref _isExtendableToFullScreen, value, IsExtendableToFullScreenChanged);
    }

    public event EventHandler? HighlightedSpansChanged;
    public event EventHandler? SyntaxColorizationLanguageNameChanged;
    public event EventHandler? IsExtendableToFullScreenChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display or edit unformatted text on multiple lines.
    /// </summary>
    /// <remarks>
    /// This component is powered by Monaco Editor.
    /// </remarks>
    public static IUIMultiLineTextInput MultilineTextInput()
    {
        return MultilineTextInput(null);
    }

    /// <summary>
    /// A component that can be used to display or edit unformatted text on multiple lines.
    /// </summary>
    /// <remarks>
    /// This component is powered by Monaco Editor.
    /// </remarks>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIMultiLineTextInput MultilineTextInput(string? id)
    {
        return new UIMultilineTextInput(id);
    }

    /// <summary>
    /// A component that can be used to display or edit unformatted text on multiple lines.
    /// </summary>
    /// <remarks>
    /// This component is powered by Monaco Editor.
    /// </remarks>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="programmingLanguageName">the programming language name to use to colorize the text in the control.</param>
    public static IUIMultiLineTextInput MultilineTextInput(string? id, string programmingLanguageName)
    {
        return new UIMultilineTextInput(id).Language(programmingLanguageName);
    }

    /// <summary>
    /// Sets the list of spans to highlight in the text document.
    /// </summary>
    public static IUIMultiLineTextInput Highlight(this IUIMultiLineTextInput element, params TextSpan[] spans)
    {
        ((UIMultilineTextInput)element).HighlightedSpans = spans;
        return element;
    }

    /// <summary>
    /// Sets the programming language name to use to colorize the text in the control.
    /// </summary>
    public static IUIMultiLineTextInput Language(this IUIMultiLineTextInput element, string programmingLanguageName)
    {
        ((UIMultilineTextInput)element).SyntaxColorizationLanguageName = programmingLanguageName;
        return element;
    }

    /// <summary>
    /// Indicates that the control can be extended to take the size of the whole tool boundaries.
    /// </summary>
    public static IUIMultiLineTextInput Extendable(this IUIMultiLineTextInput element)
    {
        ((UIMultilineTextInput)element).IsExtendableToFullScreen = true;
        return element;
    }

    /// <summary>
    /// Indicates that the control can not be extended to take the size of the whole tool boundaries.
    /// </summary>
    public static IUIMultiLineTextInput NotExtendable(this IUIMultiLineTextInput element)
    {
        ((UIMultilineTextInput)element).IsExtendableToFullScreen = false;
        return element;
    }
}
