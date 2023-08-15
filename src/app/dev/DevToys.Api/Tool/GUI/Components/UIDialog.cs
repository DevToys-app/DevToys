namespace DevToys.Api;

public sealed class UIDialog : IDisposable
{
    private readonly TaskCompletionSource _dialogCloseCompletionSource = new();

    internal UIDialog(IUIElement dialogContent, bool isDismissible)
        : this(dialogContent, null, isDismissible)
    {
    }

    internal UIDialog(IUIElement dialogContent, IUIElement? footerContent, bool isDismissible)
    {
        DialogContent = dialogContent;
        FooterContent = footerContent;
        IsDismissible = isDismissible;
        IsOpened = true;
        DialogCloseAwaiter = _dialogCloseCompletionSource.Task;
    }

    public bool IsOpened { get; private set; }

    public bool IsDismissible { get; }

    public IUIElement? DialogContent { get; }

    public IUIElement? FooterContent { get; }

    public Task DialogCloseAwaiter { get; }

    public event EventHandler? IsOpenedChanged;

    public void Close()
    {
        IsOpened = false;
        IsOpenedChanged?.Invoke(this, EventArgs.Empty);
        _dialogCloseCompletionSource.TrySetResult();
    }

    public void Dispose()
    {
        Close();
    }
}
