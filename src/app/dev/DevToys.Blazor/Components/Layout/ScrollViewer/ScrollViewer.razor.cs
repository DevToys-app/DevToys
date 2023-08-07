namespace DevToys.Blazor.Components;

public partial class ScrollViewer : JSStyledComponentBase
{
    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/Layout/ScrollViewer/ScrollViewer.razor.js";

    private string? _scrollViewerCssClasses;

    /// <summary>
    /// Gets or set the orientation in which the content can be scrolled.
    /// </summary>
    [Parameter]
    public UIOrientation Orientation { get; set; } = UIOrientation.Vertical;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the native scroll bars of the web browser should be used instead of the scroll bars
    /// provided by SimpleBar library.
    /// </summary>
    /// <remarks>
    /// This is often useful when the scroll viewer is expect to get the width (or height) or its content instead of fitting within its parent.
    /// This is commonly happening in context menu / drop down menu list scenarios, where the width of the list of item displayed in the menu
    /// is dynamic and the parent of the scroll view has no width.
    /// </remarks>
    [Parameter]
    public bool UseNativeScrollBar { get; set; }

    /// <summary>
    /// Gets or sets whether the scroll viewer is allowed to have its content scrolled.
    /// </summary>
    [Parameter]
    public bool IsScrollable { get; set; } = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();
        if (IsActuallyEnabled && !UseNativeScrollBar && IsScrollable)
        {
            // This will trigger simplebar to start.
            AdditionalAttributes.TryAdd("data-simplebar", true);
        }
        else
        {
            AdditionalAttributes.Remove("data-simplebar");
        }

        if (IsScrollable)
        {
            AdditionalAttributes.Remove("data-simplebar-not-scrollable");
        }
        else
        {
            AdditionalAttributes.TryAdd("data-simplebar-not-scrollable", true);
        }

        var scrollViewerCssClasses = new CssBuilder();
        if (((Orientation & UIOrientation.Vertical) != 0 && (Orientation & UIOrientation.Horizontal) != 0))
        {
            // Nothing. Scrollable in both axis.
        }
        else if (!IsScrollable)
        {
            scrollViewerCssClasses.AddClass("not-scrollable");
        }
        else
        {
            scrollViewerCssClasses.AddClass("vertical", (Orientation & UIOrientation.Vertical) != 0);
            scrollViewerCssClasses.AddClass("horizontal", (Orientation & UIOrientation.Horizontal) != 0);
        }

        scrollViewerCssClasses.AddClass("use-native-scroll", UseNativeScrollBar && IsScrollable);
        _scrollViewerCssClasses = scrollViewerCssClasses.Build();

        base.OnParametersSet();
    }

    public async Task ScrollToAsync(string elementId)
    {
        Guard.IsNotNullOrWhiteSpace(elementId);

        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            await (await JSModule).InvokeVoidWithErrorHandlingAsync("scrollToElement", Element, elementId);
        }
    }
}
