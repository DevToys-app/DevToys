namespace DevToys.Blazor.Components;

public partial class GridView<TKey, TElement> : JSStyledComponentBase
{
    private ScrollViewer? _scrollViewer = default!;

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/Collections/GridView/GridView.razor.js";

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
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                Guard.IsNotNull(_scrollViewer);
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("initializeStickyHeaders", _scrollViewer.Id);

                if (ItemMinWidth > 0)
                {
                    await (await JSModule).InvokeVoidWithErrorHandlingAsync("initializeDynamicItemSize", _scrollViewer.Id, ItemMinWidth);
                }
            }
        }
    }

    internal Task OnItemClickAsync(TElement item)
    {
        return OnItemClick.InvokeAsync(item);
    }
}
