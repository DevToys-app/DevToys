namespace DevToys.Api;

public sealed class UIDialog : IDisposable
{
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
    }

    public bool IsOpened { get; private set; }

    public bool IsDismissible { get; }

    public IUIElement? DialogContent { get; }

    public IUIElement? FooterContent { get; }

    public event EventHandler? IsOpenedChanged;

    public void Close()
    {
        IsOpened = false;
        IsOpenedChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Close();
    }
}
