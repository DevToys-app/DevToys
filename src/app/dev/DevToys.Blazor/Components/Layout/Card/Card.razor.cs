namespace DevToys.Blazor.Components;

public partial class Card : StyledComponentBase
{
    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? Title { get; set; }

    [Parameter]
    public RenderFragment? Description { get; set; }

    [Parameter]
    public RenderFragment? Control { get; set; }
}

