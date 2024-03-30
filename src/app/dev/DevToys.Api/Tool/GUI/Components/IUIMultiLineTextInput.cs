namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit unformatted text on multiple lines.
/// </summary>
public interface IUIMultiLineTextInput : IUISingleLineTextInput
{
    /// <summary>
    /// Gets the list of spans to highlight in the text document.
    /// </summary>
    IReadOnlyList<UIHighlightedTextSpan> HighlightedSpans { get; }

    /// <summary>
    /// Gets the list of tooltip to display on word hover.
    /// </summary>
    IReadOnlyList<UIHoverTooltip> HoverTooltips { get; }

    /// <summary>
    /// Gets the programming language name to use when colorizing the text in the control.
    /// </summary>
    string SyntaxColorizationLanguageName { get; }

    /// <summary>
    /// Gets whether the element can be expanded to take the size of the whole tool boundaries. Default is false.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    bool IsExtendableToFullScreen { get; }

    /// <summary>
    /// Gets how the text should wrap when it reached the maximum horizontal space it can take. Default is <see cref="UITextWrapMode.Auto"/>.
    /// </summary>
    UITextWrapMode WrapMode { get; }

    /// <summary>
    /// Gets the primary selection of the text control. When the selection length is 0, the span indicates the caret position.
    /// </summary>
    TextSpan Selection { get; }

    /// <summary>
    /// Raised when <see cref="HoverTooltips"/> is changed.
    /// </summary>
    event EventHandler? HoverTooltipChanged;

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

    /// <summary>
    /// Raised when <see cref="WrapMode"/> is changed.
    /// </summary>
    event EventHandler? WrapModeChanged;

    /// <summary>
    /// Raised when <see cref="Selection"/> is changed.
    /// </summary>
    event EventHandler? SelectionChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}, SyntaxColorizationLanguageName = {{{nameof(SyntaxColorizationLanguageName)}}}")]
internal class UIMultilineTextInput : UISingleLineTextInput, IUIMultiLineTextInput
{
    private IReadOnlyList<UIHoverTooltip>? _hoverTooltip;
    private IReadOnlyList<UIHighlightedTextSpan>? _highlightedSpans;
    private string? _syntaxColorizationLanguageName;
    private bool _isExtendableToFullScreen;
    private UITextWrapMode _wrapMode = UITextWrapMode.Auto;
    private TextSpan? _selection;

    internal UIMultilineTextInput(string? id)
        : base(id)
    {
    }

    public IReadOnlyList<UIHighlightedTextSpan> HighlightedSpans
    {
        get => _highlightedSpans ?? Array.Empty<UIHighlightedTextSpan>();
        internal set => SetPropertyValue(ref _highlightedSpans, value, HighlightedSpansChanged);
    }

    public IReadOnlyList<UIHoverTooltip> HoverTooltips
    {
        get => _hoverTooltip ?? Array.Empty<UIHoverTooltip>();
        internal set => SetPropertyValue(ref _hoverTooltip, value, HoverTooltipChanged);
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

    public UITextWrapMode WrapMode
    {
        get => _wrapMode;
        internal set => SetPropertyValue(ref _wrapMode, value, WrapModeChanged);
    }

    public TextSpan Selection
    {
        get => _selection ?? new TextSpan(0, 0);
        internal set
        {
            if (value != null)
            {
                if (value.StartPosition > Text.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(
                        nameof(TextSpan.StartPosition),
                        "The start position of the selection is greater than the text length.");
                }
                else if (value.EndPosition > Text.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(
                        nameof(TextSpan.Length),
                        "The end position of the selection is greater than the text length.");
                }
            }

            SetPropertyValue(ref _selection, value, SelectionChanged);
        }
    }

    public event EventHandler? HoverTooltipChanged;
    public event EventHandler? HighlightedSpansChanged;
    public event EventHandler? SyntaxColorizationLanguageNameChanged;
    public event EventHandler? IsExtendableToFullScreenChanged;
    public event EventHandler? WrapModeChanged;
    public event EventHandler? SelectionChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display or edit unformatted text on multiple lines.
    /// </summary>
    /// <remarks>
    /// This component is powered by Monaco Editor.
    /// </remarks>
    public static IUIMultiLineTextInput MultiLineTextInput()
    {
        return MultiLineTextInput(null);
    }

    /// <summary>
    /// A component that can be used to display or edit unformatted text on multiple lines.
    /// </summary>
    /// <remarks>
    /// This component is powered by Monaco Editor.
    /// </remarks>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIMultiLineTextInput MultiLineTextInput(string? id)
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
    public static IUIMultiLineTextInput MultiLineTextInput(string? id, string programmingLanguageName)
    {
        return new UIMultilineTextInput(id).Language(programmingLanguageName);
    }

    /// <summary>
    /// Sets the list of spans to highlight in the text document.
    /// </summary>
    public static IUIMultiLineTextInput Highlight(this IUIMultiLineTextInput element, params UIHighlightedTextSpan[] spans)
    {
        ((UIMultilineTextInput)element).HighlightedSpans = spans;
        return element;
    }

    /// <summary>
    /// Sets the list of tooltips to display on Word hover in the text document.
    /// </summary>
    public static IUIMultiLineTextInput HoverTooltip(this IUIMultiLineTextInput element, params UIHoverTooltip[] tooltips)
    {
        ((UIMultilineTextInput)element).HoverTooltips = tooltips;
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
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
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

    /// <summary>
    /// Indicates that the text in the editor will wrap automatically according to the user's settings.
    /// </summary>
    public static IUIMultiLineTextInput AutoWrap(this IUIMultiLineTextInput element)
    {
        ((UIMultilineTextInput)element).WrapMode = UITextWrapMode.Auto;
        return element;
    }

    /// <summary>
    /// Indicates that the text in the editor will always wrap when it reaches the border of the editor.
    /// </summary>
    public static IUIMultiLineTextInput AlwaysWrap(this IUIMultiLineTextInput element)
    {
        ((UIMultilineTextInput)element).WrapMode = UITextWrapMode.Wrap;
        return element;
    }

    /// <summary>
    /// Indicates that the text in the editor will never wrap when it reaches the border of the editor.
    /// </summary>
    public static IUIMultiLineTextInput NeverWrap(this IUIMultiLineTextInput element)
    {
        ((UIMultilineTextInput)element).WrapMode = UITextWrapMode.NoWrap;
        return element;
    }

    /// <summary>
    /// Selects the given span in the text document.
    /// </summary>
    public static IUIMultiLineTextInput Select(this IUIMultiLineTextInput element, TextSpan span)
    {
        if (element is UIMultilineTextInput strongElement)
        {
            strongElement.Selection = span;
        }
        return element;
    }

    /// <summary>
    /// Selects the given span in the text document.
    /// </summary>
    public static IUIMultiLineTextInput Select(this IUIMultiLineTextInput element, int start, int length)
    {
        if (element is UIMultilineTextInput strongElement)
        {
            strongElement.Selection = new TextSpan(start, length);
        }
        return element;
    }
}
