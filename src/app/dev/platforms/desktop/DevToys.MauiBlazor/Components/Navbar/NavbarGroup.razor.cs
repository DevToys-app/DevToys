using Microsoft.AspNetCore.Components.Web;

namespace DevToys.MauiBlazor.Components;

public partial class NavbarGroup : NavbarItem
{
    private const string DefaultActiveClass = "active";

    private string _groupClass = string.Empty;

    private readonly HashSet<string> _expendedClass = new();

    private readonly List<NavbarItem> _childs = new();

    internal string? GroupClass
    {
        get => string.Join(' ', _expendedClass);
        set
        {
            if (value is null)
            {
                return;
            }
            string[] classValues = value.Split(' ');
            foreach (string classValue in classValues)
            {
                _expendedClass.Add(classValue);
            }

            _groupClass = value;
        }
    }

    [Parameter]
    public bool IsExpended { get; set; } = false;

    internal string ExpendedName
    {
        get
        {
            if (IsExpended)
            {
                return Utils.True.Code;
            }
            return Utils.False.Code;
        }
    }

    public void RegisterChild(NavbarItem child)
    {
        _childs.Add(child);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _expendedClass.Add("nav-items");
            _expendedClass.Add("list");
            _expendedClass.Add("collapse");
        }
        if (_childs.Any(child => child.Selected) && !Expanded)
        {
            Expanded = true;
            _expendedClass.Add("show");
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("nav-group");
        base.AppendClasses(helper);
    }

    // Todo Handle collapse animation
    internal new async Task OnClickHandler(MouseEventArgs eventArgs)
    {
        Expanded = !Expanded;
        ToggleExpended();
        await InvokeAsync(StateHasChanged);
    }

    private void ToggleExpended()
    {
        if (Expanded)
        {
            _expendedClass.Add("show");
        }
        else
        {
            _expendedClass.Remove("show");
        }
        StateHasChanged();
    }
}
