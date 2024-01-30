namespace DevToys.Api;

/// <summary>
/// Represents a modal dialog.
/// </summary>
public sealed class UIDialog : IDisposable
{
    private readonly TaskCompletionSource _dialogCloseCompletionSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UIDialog"/> class with the specified dialog content and dismissible flag.
    /// </summary>
    /// <param name="dialogContent">The dialog content.</param>
    /// <param name="isDismissible">A flag indicating whether the dialog can be closed by clicking outside of it.</param>
    internal UIDialog(IUIElement dialogContent, bool isDismissible)
        : this(dialogContent, null, isDismissible)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UIDialog"/> class with the specified dialog content, footer content, and dismissible flag.
    /// </summary>
    /// <param name="dialogContent">The dialog content.</param>
    /// <param name="footerContent">The footer content.</param>
    /// <param name="isDismissible">A flag indicating whether the dialog can be closed by clicking outside of it.</param>
    internal UIDialog(IUIElement dialogContent, IUIElement? footerContent, bool isDismissible)
    {
        DialogContent = dialogContent;
        FooterContent = footerContent;
        IsDismissible = isDismissible;
        IsOpened = true;
        DialogCloseAwaiter = _dialogCloseCompletionSource.Task;
    }

    /// <summary>
    /// Gets a value indicating whether the dialog is opened.
    /// </summary>
    public bool IsOpened { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the dialog can be closed by clicking outside of it, pressing Escape key, or losing focus.
    /// </summary>
    public bool IsDismissible { get; }

    /// <summary>
    /// Gets the dialog content.
    /// </summary>
    public IUIElement? DialogContent { get; }

    /// <summary>
    /// Gets the footer content.
    /// </summary>
    public IUIElement? FooterContent { get; }

    /// <summary>
    /// Gets the task that completes when the dialog closes.
    /// </summary>
    public Task DialogCloseAwaiter { get; }

    /// <summary>
    /// Occurs when the <see cref="IsOpened"/> property changes.
    /// </summary>
    public event EventHandler? IsOpenedChanged;

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public void Close()
    {
        IsOpened = false;
        IsOpenedChanged?.Invoke(this, EventArgs.Empty);
        _dialogCloseCompletionSource.TrySetResult();
    }

    /// <summary>
    /// Disposes the dialog.
    /// </summary>
    public void Dispose()
    {
        Close();
    }
}
