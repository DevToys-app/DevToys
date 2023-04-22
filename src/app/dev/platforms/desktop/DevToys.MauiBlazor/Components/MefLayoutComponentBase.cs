using DevToys.Api;

namespace DevToys.MauiBlazor.Components;

public abstract class MefLayoutComponentBase : StyledLayoutComponentBase
{
    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
    }
}
