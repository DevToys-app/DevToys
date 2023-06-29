namespace DevToys.Blazor.Components;

public partial class ScrollViewer : StyledComponentBase
{
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();
        if (IsActuallyEnabled && !UseNativeScrollBar)
        {
            // This will trigger simplebar to start.
            AdditionalAttributes.TryAdd("data-simplebar", true);
        }
        else
        {
            AdditionalAttributes.Remove("data-simplebar");
        }

        if ((Orientation & UIOrientation.Vertical) != 0
            && (Orientation & UIOrientation.Horizontal) != 0)
        {
            CSS.Clear();
        }
        else
        {
            if ((Orientation & UIOrientation.Vertical) != 0)
            {
                CSS.Add("vertical");
            }
            else
            {
                CSS.Remove("vertical");
            }

            if ((Orientation & UIOrientation.Horizontal) != 0)
            {
                CSS.Add("horizontal");
            }
            else
            {
                CSS.Remove("horizontal");
            }
        }

        if (UseNativeScrollBar)
        {
            CSS.Add("use-native-scroll");
        }
        else
        {
            CSS.Remove("use-native-scroll");
        }

        base.OnParametersSet();
    }
}
