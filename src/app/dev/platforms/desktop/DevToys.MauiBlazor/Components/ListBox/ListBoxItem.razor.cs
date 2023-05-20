namespace DevToys.MauiBlazor.Components;

public partial class ListBoxItem : StyledComponentBase
{
    [Parameter]
    public object Item { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public bool IsEnabled { get; set; } = true;

    [Parameter]
    public EventCallback<object> OnSelected { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnParametersSet()
    {
        if (IsSelected)
        {
            CSS.Add("selected");
        }
        else
        {
            CSS.Remove("selected");
        }

        if (IsEnabled)
        {
            CSS.Remove("disabled");
        }
        else
        {
            CSS.Add("disabled");
        }

        base.OnParametersSet();
    }

    private Task OnClickAsync()
    {
        return OnSelected.InvokeAsync(Item);
    }
}
