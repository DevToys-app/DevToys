namespace DevToys.Blazor.Components;

public partial class ScrollViewer : JSStyledComponentBase
{
    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/ScrollViewer/ScrollViewer.razor.js";

    /// <summary>
    /// Gets or set the orientation in which the content can be scrolled.
    /// </summary>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Vertical;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await (await JSModule).InvokeVoidAsync("initializeScrollViewer", Id);
        }
    }

    protected override void OnParametersSet()
    {
        if ((Orientation & Orientation.Vertical) != 0
            && (Orientation & Orientation.Horizontal) != 0)
        {
            CSS.Clear();
        }
        else
        {
            if ((Orientation & Orientation.Vertical) != 0)
            {
                CSS.Add("vertical");
            }
            else
            {
                CSS.Remove("vertical");
            }

            if ((Orientation & Orientation.Horizontal) != 0)
            {
                CSS.Add("horizontal");
            }
            else
            {
                CSS.Remove("horizontal");
            }
        }

        base.OnParametersSet();
    }
}
