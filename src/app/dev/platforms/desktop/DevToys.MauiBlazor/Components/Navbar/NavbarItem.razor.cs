using DevToys.Core.Tools.ViewItems;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.MauiBlazor.Components;

public partial class NavbarItem : MefLayoutComponentBase
{
    private const string DefaultActiveClass = "active";
    private bool _selected;

    internal string? DisplayName
    {
        get
        {
            if (Item is GuiToolViewItem guiToolViewItem)
            {
                return guiToolViewItem.MenuDisplayTitle;
            }
            if (Item is GroupViewItem groupViewItem)
            {
                return groupViewItem.DisplayTitle;
            }
            return string.Empty;
        }
    }

    [Parameter]
    public INotifyPropertyChanged Item { get; set; } = default!;

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

    protected override void OnParametersSet()
    {
        bool isSelected = false;
        if (Item is GuiToolViewItem guiToolViewItem)
        {
            // handle selected item selected
        }
        if (Item is GroupViewItem groupViewItem)
        {
            // handle selected item selected
        }

        if (isSelected)
        {
            ClassesHasChanged();
            StateHasChanged();
        }
        base.OnParametersSet();
    }

    internal async Task OnClickHandler(MouseEventArgs eventArgs)
    {
        //await Clicked.InvokeAsync();
        await Task.CompletedTask;
    }

    internal async void HandleClickEventAsync(bool isSelected)
    {
        Selected = isSelected;
        await Task.CompletedTask;
    }
}
