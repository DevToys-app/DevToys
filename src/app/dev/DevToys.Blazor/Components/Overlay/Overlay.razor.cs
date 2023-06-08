using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class Overlay : StyledComponentBase
{
    private string OverlayClasses =>
        new CssBuilder("overlay")
            .AddClass("overlay-absolute", FitChildContent)
            .AddClass(FinalCssClasses)
            .Build();

    private string OverlayStyles =>
        new StyleBuilder()
            .AddStyle("z-index", $"{ZIndex}", ZIndex != 5)
            .AddStyle(Style)
            .Build();

    /// <summary>
    /// If true overlay will be visible. Two-way bindable.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; }

    /// <summary>
    /// If true, the overlay will only cover the <see cref="ChildContent"/>.
    /// </summary>
    [Parameter]
    public bool FitChildContent { get; set; }

    /// <summary>
    /// Sets the z-index of the overlay.
    /// </summary>
    [Parameter]
    public int ZIndex { get; set; } = 5;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Fired when the overlay is clicked
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    protected internal async Task OnClickAsync(MouseEventArgs ev)
    {
        await OnClick.InvokeAsync(ev);
    }
}
