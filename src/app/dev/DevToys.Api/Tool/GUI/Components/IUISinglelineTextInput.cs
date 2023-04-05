namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit unformatted text on a single line.
/// </summary>
public interface IUISinglelineTextInput : IUITitledElement
{
    /// <summary>
    /// Gets whether the user can edit the text or not. Default is false.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets whether the `Copy` button should be displayed on top of the editor, even when it is not read-only. Default is false.
    /// </summary>
    bool CanCopyWhenEditable { get; }

    /// <summary>
    /// Gets the text displayed or typed by the user.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the primary selection of the text control. When the selection length is 0, the span indicates the caret position.
    /// </summary>
    TextSpan Selection { get; }

    /// <summary>
    /// Raised when <see cref="IsReadOnly"/> is changed.
    /// </summary>
    event EventHandler? IsReadOnlyChanged;

    /// <summary>
    /// Raised when <see cref="CanCopyWhenEditable"/> is changed.
    /// </summary>
    event EventHandler? CanCopyWhenEditableChanged;

    /// <summary>
    /// Raised when <see cref="Text"/> is changed.
    /// </summary>
    event EventHandler? TextChanged;

    /// <summary>
    /// Raised when <see cref="Selection"/> is changed.
    /// </summary>
    event EventHandler? SelectionChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal class UISinglelineTextInput : UITitledElement, IUISinglelineTextInput
{
    private string? _text;
    private bool _isReadOnly;
    private bool _canCopyWhenEditable;
    private TextSpan? _selection;

    internal UISinglelineTextInput(string? id)
        : base(id)
    {
    }

    public string Text
    {
        get => _text ?? string.Empty;
        internal set
        {
            _text = value;
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        internal set
        {
            _isReadOnly = value;
            IsReadOnlyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CanCopyWhenEditable
    {
        get => _canCopyWhenEditable;
        internal set
        {
            _canCopyWhenEditable = value;
            CanCopyWhenEditableChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public TextSpan Selection
    {
        get => _selection ?? new TextSpan(0, 0);
        internal set
        {
            _selection = value;
            if (_selection != null)
            {
                if (_selection.StartPosition > Text.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(
                        nameof(TextSpan.StartPosition),
                        "The start position of the selection is greater than the text length.");
                }
                else if (_selection.StartPosition + _selection.Length > Text.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(
                        nameof(TextSpan.Length),
                        "The end position of the selection is greater than the text length.");
                }
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? TextChanged;
    public event EventHandler? IsReadOnlyChanged;
    public event EventHandler? CanCopyWhenEditableChanged;
    public event EventHandler? SelectionChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that can be used to display or edit unformatted text on a single line.
    /// </summary>
    public static IUISinglelineTextInput SinglelineTextInput()
    {
        return SinglelineTextInput(null);
    }

    /// <summary>
    /// Create a component that can be used to display or edit unformatted text on a single line.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISinglelineTextInput SinglelineTextInput(string? id)
    {
        return new UISinglelineTextInput(id);
    }

    /// <summary>
    /// Sets the text input control as read-only.
    /// </summary>
    public static T ReadOnly<T>(this T element) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.IsReadOnly = true;
        }
        return element;
    }

    /// <summary>
    /// Sets the text input control as editable.
    /// </summary>
    public static T Editable<T>(this T element) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.IsReadOnly = false;
        }
        return element;
    }

    /// <summary>
    /// Shows the "copy" button when the editor is editable.
    /// </summary>
    public static T CanCopyWhenEditable<T>(this T element) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.CanCopyWhenEditable = true;
        }
        return element;
    }

    /// <summary>
    /// Hides the "copy" button when the editor is editable.
    /// </summary>
    public static T CannotCopyWhenEditable<T>(this T element) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.CanCopyWhenEditable = false;
        }
        return element;
    }

    /// <summary>
    /// Sets the unformatted text of the control.
    /// </summary>
    public static T Text<T>(this T element, string text) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.Text = text;
        }
        return element;
    }

    /// <summary>
    /// Selects the given span in the text document.
    /// </summary>
    public static T Select<T>(this T element, TextSpan span) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.Selection = span;
        }
        return element;
    }

    /// <summary>
    /// Selects the given span in the text document.
    /// </summary>
    public static T Select<T>(this T element, int start, int length) where T : IUISinglelineTextInput
    {
        if (element is UISinglelineTextInput strongElement)
        {
            strongElement.Selection = new TextSpan(start, length);
        }
        return element;
    }
}
