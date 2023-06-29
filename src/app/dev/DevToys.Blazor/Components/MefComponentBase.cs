namespace DevToys.Blazor.Components;

public abstract class MefComponentBase : JSStyledComponentBase
{
    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
    }
}
