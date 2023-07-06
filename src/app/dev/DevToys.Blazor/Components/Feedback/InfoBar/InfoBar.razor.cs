namespace DevToys.Blazor.Components;

public partial class InfoBar : StyledComponentBase
{
    public InfoBar()
    {
        VerticalAlignment = UIVerticalAlignment.Center;
    }

    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public bool IsClosable { get; set; } = true;

    [Parameter]
    public bool IsIconVisible { get; set; } = true;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public string ActionButtonText { get; set; } = string.Empty;

    [Parameter]
    public bool IsActionButtonAccent { get; set; }

    [Parameter]
    public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;

    [Parameter]
    public EventCallback Closed { get; set; }

    [Parameter]
    public EventCallback ActionButtonClicked { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
    }

    private void OnCloseButtonClicked()
    {
        IsOpened = false;
        if (Closed.HasDelegate)
        {
            Closed.InvokeAsync();
        }
    }

    private void OnActionButtonClicked()
    {
        if (ActionButtonClicked.HasDelegate)
        {
            ActionButtonClicked.InvokeAsync();
        }
        OnCloseButtonClicked();
    }
}
