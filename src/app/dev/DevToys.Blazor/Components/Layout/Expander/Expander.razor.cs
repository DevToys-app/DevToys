namespace DevToys.Blazor.Components;

public partial class Expander : StyledComponentBase
{
    public bool IsExpanded { get; set; } = false;

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? Title { get; set; }

    [Parameter]
    public RenderFragment? Description { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private Task OnClickAsync()
    {
        return Task.Run(() => IsExpanded = !IsExpanded);
    }
}

