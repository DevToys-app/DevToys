namespace DevToys.MauiBlazor.Components;

public partial class ScrollViewer : StyledComponentBase, IAsyncDisposable
{
    private const string JAVASCRIPT_FILE = "./Components/ScrollViewer/ScrollViewer.razor.js";

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference JSModule { get; set; } = default!;

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
        if (firstRender)
        {
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await JSModule.InvokeVoidAsync("initializeScrollViewer", Id);
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

    /// <summary />
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (JSModule is not null)
        {
            await JSModule.DisposeAsync();
        }
    }
}
