using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class GridView<TKey, TElement> : JSStyledComponentBase, IFocusable
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

    internal string ScrollViewerId
    {
        get
        {
            Guard.IsNotNull(_scrollViewer);
            return _scrollViewer.Id;
        }
    }

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

    public ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_scrollViewer);
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _scrollViewer.Element);
    }

    private Task OnItemClickAsync(TElement item)
    {
        return OnItemClick.InvokeAsync(item);
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs ev, TElement item)
    {
        if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ev.Code, "Space", StringComparison.OrdinalIgnoreCase))
        {
            await OnItemClickAsync(item);
        }
    }
}
