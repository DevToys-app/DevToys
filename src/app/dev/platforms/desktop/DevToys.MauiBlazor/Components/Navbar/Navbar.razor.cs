namespace DevToys.MauiBlazor.Components;

public partial class NavBar : StyledComponentBase
{

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }
}
