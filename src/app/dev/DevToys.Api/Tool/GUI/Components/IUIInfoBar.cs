namespace DevToys.Api;

/// <summary>
/// A component that displays a bar aiming at indicating a relevant information to the user.
/// </summary>
public interface IUIInfoBar : IUIElement
{
    /// <summary>
    /// Gets whether the bar is opened.
    /// Default is false.
    /// </summary>
    bool IsOpened { get; }

    /// <summary>
    /// Gets whether the bar can be closed by the user on demand.
    /// Default is true.
    /// </summary>
    bool IsClosable { get; }

    /// <summary>
    /// Gets whether the icon of the bar should be displayed or not.
    /// Default is true.
    /// </summary>
    bool IsIconVisible { get; }

    /// <summary>
    /// Gets the title to display in the bar.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets the description to display in the bar.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the text of the optional button to display in the bar.
    /// A null or empty value will hide the button. The bar closes when the user click on the button.
    /// </summary>
    string? ActionButtonText { get; }

    /// <summary>
    /// Gets whether the optional button to display in the bar should be accented.
    /// Default is false.
    /// </summary>
    bool IsActionButtonAccent { get; }

    /// <summary>
    /// Gets the severity of the information to display.
    /// Default is <see cref="UIInfoBarSeverity.Informational"/>.
    /// </summary>
    UIInfoBarSeverity Severity { get; }

    /// <summary>
    /// Gets the action to run when the user explicitly close the bar.
    /// </summary>
    Func<ValueTask>? OnCloseAction { get; }

    /// <summary>
    /// Gets the action to run when the user clicks the action button.
    /// The bar closes when the user click on the button.
    /// </summary>
    Func<ValueTask>? OnActionButtonClick { get; }

    /// <summary>
    /// Raised when <see cref="IsOpened"/> is changed.
    /// </summary>
    event EventHandler? IsOpenedChanged;

    /// <summary>
    /// Raised when <see cref="IsClosable"/> is changed.
    /// </summary>
    event EventHandler? IsClosableChanged;

    /// <summary>
    /// Raised when <see cref="IsIconVisible"/> is changed.
    /// </summary>
    event EventHandler? IsIconVisibleChanged;

    /// <summary>
    /// Raised when <see cref="Title"/> is changed.
    /// </summary>
    event EventHandler? TitleChanged;

    /// <summary>
    /// Raised when <see cref="Description"/> is changed.
    /// </summary>
    event EventHandler? DescriptionChanged;

    /// <summary>
    /// Raised when <see cref="ActionButtonText"/> is changed.
    /// </summary>
    event EventHandler? ActionButtonTextChanged;

    /// <summary>
    /// Raised when <see cref="IsActionButtonAccent"/> is changed.
    /// </summary>
    event EventHandler? IsActionButtonAccentChanged;

    /// <summary>
    /// Raised when <see cref="Severity"/> is changed.
    /// </summary>
    event EventHandler? SeverityChanged;

    /// <summary>
    /// Raised when <see cref="OnCloseAction"/> is changed.
    /// </summary>
    event EventHandler? OnCloseActionChanged;

    /// <summary>
    /// Raised when <see cref="OnActionButtonClick"/> is changed.
    /// </summary>
    event EventHandler? OnActionButtonClickChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal sealed class UIInfoBar : UIElement, IUIInfoBar
{
    private bool _isOpened;
    private bool _isClosable = true;
    private bool _isIconVisible = true;
    private string? _title;
    private string? _description;
    private string? _actionButtonText;
    private bool _isActionButtonAccent;
    private UIInfoBarSeverity _severity = UIInfoBarSeverity.Informational;
    private Func<ValueTask>? _onCloseAction;
    private Func<ValueTask>? _onActionButtonClick;

    internal UIInfoBar(string? id)
        : base(id)
    {
    }

    public bool IsOpened
    {
        get => _isOpened;
        internal set => SetPropertyValue(ref _isOpened, value, IsOpenedChanged);
    }

    public bool IsClosable
    {
        get => _isClosable;
        internal set => SetPropertyValue(ref _isClosable, value, IsClosableChanged);
    }

    public bool IsIconVisible
    {
        get => _isIconVisible;
        internal set => SetPropertyValue(ref _isIconVisible, value, IsIconVisibleChanged);
    }

    public string? Title
    {
        get => _title;
        internal set => SetPropertyValue(ref _title, value, TitleChanged);
    }

    public string? Description
    {
        get => _description;
        internal set => SetPropertyValue(ref _description, value, DescriptionChanged);
    }

    public string? ActionButtonText
    {
        get => _actionButtonText;
        internal set => SetPropertyValue(ref _actionButtonText, value, ActionButtonTextChanged);
    }

    public bool IsActionButtonAccent
    {
        get => _isActionButtonAccent;
        internal set => SetPropertyValue(ref _isActionButtonAccent, value, IsActionButtonAccentChanged);
    }

    public UIInfoBarSeverity Severity
    {
        get => _severity;
        internal set => SetPropertyValue(ref _severity, value, SeverityChanged);
    }

    public Func<ValueTask>? OnCloseAction
    {
        get => _onCloseAction;
        internal set => SetPropertyValue(ref _onCloseAction, value, OnCloseActionChanged);
    }

    public Func<ValueTask>? OnActionButtonClick
    {
        get => _onActionButtonClick;
        internal set => SetPropertyValue(ref _onActionButtonClick, value, OnActionButtonClickChanged);
    }

    public event EventHandler? IsOpenedChanged;
    public event EventHandler? IsClosableChanged;
    public event EventHandler? IsIconVisibleChanged;
    public event EventHandler? TitleChanged;
    public event EventHandler? DescriptionChanged;
    public event EventHandler? ActionButtonTextChanged;
    public event EventHandler? IsActionButtonAccentChanged;
    public event EventHandler? SeverityChanged;
    public event EventHandler? OnCloseActionChanged;
    public event EventHandler? OnActionButtonClickChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that displays a bar aiming at indicating a relevant information to the user.
    /// </summary>
    public static IUIInfoBar InfoBar()
    {
        return InfoBar(null);
    }

    /// <summary>
    /// Create a component that displays a bar aiming at indicating a relevant information to the user.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIInfoBar InfoBar(string? id)
    {
        return new UIInfoBar(id);
    }

    /// <summary>
    /// Create a component that displays a bar aiming at indicating a relevant information to the user.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="title">The title to display in the bar.</param>
    public static IUIInfoBar InfoBar(string? id, string title)
    {
        return InfoBar(id).Title(title);
    }

    /// <summary>
    /// Create a component that displays a bar aiming at indicating a relevant information to the user.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="title">The title to display in the bar.</param>
    /// <param name="description">The description to display in the bar.</param>
    public static IUIInfoBar InfoBar(string? id, string title, string description)
    {
        return InfoBar(id, title).Description(description);
    }

    /// <summary>
    /// Create a component that displays a bar aiming at indicating a relevant information to the user.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="title">The title to display in the bar.</param>
    /// <param name="description">The description to display in the bar.</param>
    /// <param name="severity">The severity of the information to display.</param>
    public static IUIInfoBar InfoBar(string? id, string title, string description, UIInfoBarSeverity severity)
    {
        var infoBar = (UIInfoBar)InfoBar(id, title, description);
        infoBar.Severity = severity;
        return infoBar;
    }

    /// <summary>
    /// Sets the <see cref="IUIInfoBar.Title"/> of the bar.
    /// </summary>
    public static IUIInfoBar Title(this IUIInfoBar element, string? title)
    {
        ((UIInfoBar)element).Title = title;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIInfoBar.Description"/> of the bar.
    /// </summary>
    public static IUIInfoBar Description(this IUIInfoBar element, string? description)
    {
        ((UIInfoBar)element).Description = description;
        return element;
    }

    /// <summary>
    /// Sets the optional action button of the bar. The bar closes when the user click on the button.
    /// </summary>
    public static IUIInfoBar WithActionButton(this IUIInfoBar element, string text, bool isAccent, Func<ValueTask> actionOnClick)
    {
        var infoBar = (UIInfoBar)element;
        infoBar.ActionButtonText = text;
        infoBar.IsActionButtonAccent = isAccent;
        infoBar.OnActionButtonClick = actionOnClick;
        return infoBar;
    }

    /// <summary>
    /// Sets the optional action button of the bar. The bar closes when the user click on the button.
    /// </summary>
    public static IUIInfoBar WithActionButton(this IUIInfoBar element, string text, bool isAccent, Action actionOnClick)
    {
        return WithActionButton(
            element,
            text,
            isAccent,
            () =>
            {
                actionOnClick?.Invoke();
                return ValueTask.CompletedTask;
            });
    }

    /// <summary>
    /// Sets the action to run when the user clicks the action button.
    /// </summary>
    public static IUIInfoBar OnClose(this IUIInfoBar element, Func<ValueTask>? actionOnClose)
    {
        ((UIInfoBar)element).OnCloseAction = actionOnClose;
        return element;
    }

    /// <summary>
    /// Sets the action to run when the user clicks the action button.
    /// </summary>
    public static IUIInfoBar OnClose(this IUIInfoBar element, Action? actionOnClose)
    {
        ((UIInfoBar)element).OnCloseAction
            = () =>
            {
                actionOnClose?.Invoke();
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIInfoBar.Severity"/> to <see cref="UIInfoBarSeverity.Informational"/>.
    /// </summary>
    public static IUIInfoBar Informational(this IUIInfoBar element)
    {
        ((UIInfoBar)element).Severity = UIInfoBarSeverity.Informational;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIInfoBar.Severity"/> to <see cref="UIInfoBarSeverity.Error"/>.
    /// </summary>
    public static IUIInfoBar Error(this IUIInfoBar element)
    {
        ((UIInfoBar)element).Severity = UIInfoBarSeverity.Error;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIInfoBar.Severity"/> to <see cref="UIInfoBarSeverity.Warning"/>.
    /// </summary>
    public static IUIInfoBar Warning(this IUIInfoBar element)
    {
        ((UIInfoBar)element).Severity = UIInfoBarSeverity.Warning;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIInfoBar.Severity"/> to <see cref="UIInfoBarSeverity.Success"/>.
    /// </summary>
    public static IUIInfoBar Success(this IUIInfoBar element)
    {
        ((UIInfoBar)element).Severity = UIInfoBarSeverity.Success;
        return element;
    }

    /// <summary>
    /// Shows the icon of the bar.
    /// </summary>
    public static IUIInfoBar ShowIcon(this IUIInfoBar element)
    {
        ((UIInfoBar)element).IsIconVisible = true;
        return element;
    }

    /// <summary>
    /// Hides the icon of the bar.
    /// </summary>
    public static IUIInfoBar HideIcon(this IUIInfoBar element)
    {
        ((UIInfoBar)element).IsIconVisible = false;
        return element;
    }

    /// <summary>
    /// Opens the bar.
    /// </summary>
    public static IUIInfoBar Open(this IUIInfoBar element)
    {
        ((UIInfoBar)element).IsOpened = true;
        return element;
    }

    /// <summary>
    /// Closes the bar.
    /// </summary>
    public static IUIInfoBar Close(this IUIInfoBar element)
    {
        ((UIInfoBar)element).IsOpened = false;
        return element;
    }

    /// <summary>
    /// Makes the bar closable explicitly by the user.
    /// </summary>
    public static IUIInfoBar Closable(this IUIInfoBar element)
    {
        ((UIInfoBar)element).IsClosable = true;
        return element;
    }

    /// <summary>
    /// Makes the bar not closable explicitly by the user.
    /// </summary>
    public static IUIInfoBar NonClosable(this IUIInfoBar element)
    {
        ((UIInfoBar)element).IsClosable = false;
        return element;
    }
}
