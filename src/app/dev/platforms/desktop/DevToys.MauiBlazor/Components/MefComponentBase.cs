using DevToys.Api;

namespace DevToys.MauiBlazor.Components;

public abstract class MefComponentBase : StyledComponentBase
{
    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
    }
}
