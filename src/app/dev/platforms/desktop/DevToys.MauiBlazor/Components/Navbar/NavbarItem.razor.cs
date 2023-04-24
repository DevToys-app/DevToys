using Microsoft.AspNetCore.Components.Web;

namespace DevToys.MauiBlazor.Components;

public partial class NavbarItem : StyledLayoutComponentBase
{
    private const string DefaultActiveClass = "active";
    private bool _selected;

    [Parameter]
    public INotifyPropertyChanged Item { get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string IconFontFamily { get; set; } = string.Empty;

    [Parameter]
    public char IconGlyph { get; set; }

    [Parameter]
    public EventCallback<INotifyPropertyChanged> OnSelected { get; set; }

    [Parameter]
    public bool Expanded { get; set; }

    [Parameter]
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value)
            {
                return;
            }

            _selected = value;
            ClassesHasChanged();
        }
    }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("nav-item");
        if (Selected)
        {
            helper.Append("active");
        }
        base.AppendClasses(helper);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Selected)
            {
                ClassesHasChanged();
            }
            await InvokeAsync(StateHasChanged);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    internal Task OnClickAsync(MouseEventArgs eventArgs)
    {
        return OnSelected.InvokeAsync(Item);
    }
}
