using DevToys.Blazor.Core.Services;

namespace DevToys.Blazor.Components;

public partial class Popover : StyledComponentBase, IAsyncDisposable
{
    private bool _afterFirstRender;
    private PopoverHandler? _handler;

    [Inject]
    internal PopoverService PopoverService { get; set; } = default!;

    private string PopoverClasses =>
        new CssBuilder("popover")
        .AddClass($"popover-fixed", Fixed)
        .AddClass($"popover-open", Open)
        .AddClass($"popover-{OriginToPartialClassName(TransformOrigin)}")
        .AddClass($"popover-anchor-{OriginToPartialClassName(AnchorOrigin)}")
        .AddClass($"popover-overflow-{OverflowBehaviorToPartialClassName(OverflowBehavior)}")
        .AddClass($"popover-relative-width", RelativeWidth)
        .AddClass($"overflow-y-auto", MaxHeight != null)
        .AddClass(FinalCssClasses)
        .Build();

    private string PopoverStyles =>
        new StyleBuilder()
        .AddStyle("transition-duration", $"250ms")
        .AddStyle("transition-delay", $"0ms")
        .AddStyle("max-height", MaxHeight.ToPx(), MaxHeight != null)
        .AddStyle(Style)
        .Build();

    /// <summary>
    /// Sets the maximum height the popover can have when open.
    /// </summary>
    [Parameter]
    public int? MaxHeight { get; set; } = null;

    /// <summary>
    /// If true, the popover is visible.
    /// </summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>
    /// If true the popover will be fixed position instead of absolute.
    /// </summary>
    [Parameter]
    public bool Fixed { get; set; }

    /// <summary>
    /// Set the anchor point on the element of the popover.
    /// The anchor point will determinate where the popover will be placed.
    /// </summary>
    [Parameter]
    public Origin AnchorOrigin { get; set; } = Origin.TopLeft;

    /// <summary>
    /// Sets the intersection point if the anchor element. At this point the popover will lay above the popover. 
    /// This property in conjunction with <see cref="AnchorOrigin"/> determinate where the popover will be placed.
    /// </summary>
    [Parameter]
    public Origin TransformOrigin { get; set; } = Origin.TopLeft;

    /// <summary>
    /// Set the overflow behavior of a popover and controls how the element should react if there is not enough space for the element to be visible
    /// Defaults to none, which doesn't apply any overflow logic
    /// </summary>
    [Parameter]
    public OverflowBehavior OverflowBehavior { get; set; } = OverflowBehavior.FlipOnOpen;

    /// <summary>
    /// If true, the popover will have the same width at its parent element, default to false
    /// </summary>
    [Parameter]
    public bool RelativeWidth { get; set; } = false;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        ChildContent ??= new RenderFragment((x) => { });

        _handler = PopoverService.Register(ChildContent);
        _handler.SetComponentBaseParameters(this, PopoverClasses, PopoverStyles, Open);
        base.OnInitialized();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (_afterFirstRender)
        {
            Guard.IsNotNull(_handler);
            Guard.IsNotNull(ChildContent);
            await _handler.UpdateFragmentAsync(ChildContent, this, PopoverClasses, PopoverStyles, Open);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Guard.IsNotNull(_handler);
            Guard.IsNotNull(ChildContent);
            await _handler.Initialize();
            await PopoverService.InitializeIfNeeded();
            await _handler.UpdateFragmentAsync(ChildContent, this, PopoverClasses, PopoverStyles, Open);
            _afterFirstRender = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_handler is not null)
            {
                await PopoverService.Unregister(_handler);
            }
        }
        catch (JSDisconnectedException) { }
        catch (TaskCanceledException) { }

        GC.SuppressFinalize(this);
    }

    private static string OverflowBehaviorToPartialClassName(OverflowBehavior overflowBehavior)
    {
        return overflowBehavior switch
        {
            OverflowBehavior.FlipNever => "flip-never",
            OverflowBehavior.FlipOnOpen => "flip-onopen",
            OverflowBehavior.FlipAlways => "flip-always",
            _ => throw new NotSupportedException()
        };
    }

    private static string OriginToPartialClassName(Origin origin)
    {
        return origin switch
        {
            Origin.TopLeft => "top-left",
            Origin.TopCenter => "top-center",
            Origin.TopRight => "top-right",
            Origin.CenterLeft => "center-left",
            Origin.CenterCenter => "center-center",
            Origin.CenterRight => "center-right",
            Origin.BottomLeft => "bottom-left",
            Origin.BottomCenter => "bottom-center",
            Origin.BottomRight => "bottom-right",
            _ => throw new NotSupportedException()
        };
    }
}
