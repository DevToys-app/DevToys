namespace DevToys.MauiBlazor.Components;

public partial class GridView<TKey, TElement> : StyledComponentBase, IAsyncDisposable
{
    private const string JAVASCRIPT_FILE = "./Components/GridView/GridView.razor.js";

    private ScrollViewer? _scrollViewer = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference JSModule { get; set; } = default!;

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public RenderFragment<TKey>? GroupHeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<TElement> ItemTemplate { get; set; } = default!;

    [Parameter]
    public ILookup<TKey, TElement>? ItemsSource { get; set; }

    [Parameter]
    public EventCallback<object> OnItemClick { get; set; }

    [Parameter]
    public int ItemMinWidth { get; set; }

    public GridView()
    {
        CSS.Add("grid-view");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Guard.IsNotNull(_scrollViewer);
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await JSModule.InvokeVoidAsync("initializeStickyHeaders", _scrollViewer.Id);

            if (ItemMinWidth > 0)
            {
                await JSModule.InvokeVoidAsync("initializeDynamicItemSize", _scrollViewer.Id, ItemMinWidth);
            }
        }
    }

    /// <summary />
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (JSModule is not null)
        {
            await JSModule.DisposeAsync();
        }
    }

    internal Task OnItemClickAsync(TElement item)
    {
        return OnItemClick.InvokeAsync(item);
    }
}
