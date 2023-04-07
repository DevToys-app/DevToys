using DevToys.MauiBlazor.Core.Helpers;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.MauiBlazor.Components;

public partial class NavbarItem : MefLayoutComponentBase
{
    private const string DefaultActiveClass = "active";
    private bool _selected;

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Page { get; set; }

    [Parameter]
    public string? Icon { get; set; }

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
        if (_selected)
        {
            helper.Append("active");
        }
        base.AppendClasses(helper);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (_selected)
            {
                ClassesHasChanged();
            }
            await InvokeAsync(StateHasChanged);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    internal async void HandleClickEventAsync(bool isSelected)
    {
        Selected = isSelected;
        await Task.CompletedTask;
    }
}
