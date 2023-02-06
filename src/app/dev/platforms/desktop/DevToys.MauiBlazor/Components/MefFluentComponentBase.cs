using DevToys.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;

namespace DevToys.MauiBlazor.Components;

public abstract class MefFluentComponentBase : FluentComponentBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Inject]
    protected IMefProvider MefProvider { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
    }
}
