namespace DevToys.Blazor.Components.UIElements;

public partial class UIProgressRingPresenter : ComponentBase, IDisposable
{
    [Parameter]
    public IUIProgressRing UIProgressRing { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (UIProgressRing is not null)
        {
            UIProgressRing.ValueChangingAsynchronously += UIProgressRing_ValueChangingAsynchronously;
        }
    }

    public void Dispose()
    {
        if (UIProgressRing is not null)
        {
            UIProgressRing.ValueChangingAsynchronously -= UIProgressRing_ValueChangingAsynchronously;
        }
        GC.SuppressFinalize(this);
    }

    private void UIProgressRing_ValueChangingAsynchronously(object? sender, double percentage)
    {
        InvokeAsync(() =>
        {
            UIProgressRing?.Progress(percentage);
        });
    }
}
