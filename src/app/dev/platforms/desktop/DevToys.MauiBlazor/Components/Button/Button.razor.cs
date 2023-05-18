namespace DevToys.MauiBlazor.Components;

public partial class Button : StyledComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public bool IsEnabled { get; set; } = true;

    [Parameter]
    public ButtonAppearance Appearance { get; set; } = ButtonAppearance.Neutral;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnParametersSet()
    {
        CSS.Clear();
        CSS.Add($"type-{Appearance.Class}");

        if (IsEnabled && AdditionalAttributes is not null)
        {
            AdditionalAttributes.Remove("disabled");
        }

        if (!IsEnabled)
        {
            CSS.Add("disabled");

            AdditionalAttributes ??= new Dictionary<string, object>();

            AdditionalAttributes.TryAdd("disabled", true);
        }

        base.OnParametersSet();
    }

    internal ValueTask FocusAsync()
    {
        return JSRuntime.InvokeVoidAsync("devtoys.DOM.setFocus", Element);
    }
}
