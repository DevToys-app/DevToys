using System;
using static System.Net.Mime.MediaTypeNames;

namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit unformatted text on multiple lines.
/// </summary>
/// <remarks>
/// This component is powered by Monaco Editor.
/// </remarks>
public interface IUIMultilineLineTextInput : IUISinglelineTextInput
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
    /// Raised when <see cref="HighlightedSpans"/> is changed.
    /// </summary>
    event EventHandler? HighlightedSpansChanged;

    /// <summary>
    /// Raised when <see cref="SyntaxColorizationLanguageName"/> is changed.
    /// </summary>
    event EventHandler? SyntaxColorizationLanguageNameChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal class UIMultilineTextInput : UISinglelineTextInput, IUIMultilineLineTextInput
{
    private IReadOnlyList<TextSpan>? _highlightedSpans;
    private string? _syntaxColorizationLanguageName;

    internal UIMultilineTextInput(string? id)
        : base(id)
    {
    }

    public IReadOnlyList<TextSpan> HighlightedSpans
    {
        get => _highlightedSpans ?? Array.Empty<TextSpan>();
        internal set
        {
            _highlightedSpans = value;
            HighlightedSpansChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string SyntaxColorizationLanguageName
    {
        get => _syntaxColorizationLanguageName ?? string.Empty;
        internal set
        {
            _syntaxColorizationLanguageName = value;
            SyntaxColorizationLanguageNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? HighlightedSpansChanged;
    public event EventHandler? SyntaxColorizationLanguageNameChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display or edit unformatted text on multiple lines.
    /// </summary>
    /// <remarks>
    /// This component is powered by Monaco Editor.
    /// </remarks>
    public static IUIMultilineLineTextInput MultilineTextInput()
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
    public static IUIMultilineLineTextInput MultilineTextInput(string? id)
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
    public static IUIMultilineLineTextInput MultilineTextInput(string? id, string programmingLanguageName)
    {
        return new UIMultilineTextInput(id).Language(programmingLanguageName);
    }

    /// <summary>
    /// Sets the list of spans to highlight in the text document.
    /// </summary>
    public static IUIMultilineLineTextInput Highlight(this IUIMultilineLineTextInput element, params TextSpan[] spans)
    {
        ((UIMultilineTextInput)element).HighlightedSpans = spans;
        return element;
    }

    /// <summary>
    /// Sets the programming language name to use to colorize the text in the control.
    /// </summary>
    public static IUIMultilineLineTextInput Language(this IUIMultilineLineTextInput element, string programmingLanguageName)
    {
        ((UIMultilineTextInput)element).SyntaxColorizationLanguageName = programmingLanguageName;
        return element;
    }
}
