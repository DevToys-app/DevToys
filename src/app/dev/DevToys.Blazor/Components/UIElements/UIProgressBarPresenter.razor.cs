namespace DevToys.Blazor.Components.UIElements;

public partial class UIProgressBarPresenter : ComponentBase, IDisposable
{
    [Parameter]
    public IUIProgressBar UIProgressBar { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (UIProgressBar is not null)
        {
            UIProgressBar.ValueChangingAsynchronously += UIProgressBar_ValueChangingAsynchronously;
        }
    }

    public void Dispose()
    {
        if (UIProgressBar is not null)
        {
            UIProgressBar.ValueChangingAsynchronously -= UIProgressBar_ValueChangingAsynchronously;
        }
        GC.SuppressFinalize(this);
    }

    private void UIProgressBar_ValueChangingAsynchronously(object? sender, double percentage)
    {
        InvokeAsync(() =>
        {
            UIProgressBar?.Progress(percentage);
        });
    }
}
