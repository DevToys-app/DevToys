namespace DevToys.MauiBlazor.Components;

public partial class NavbarGroup : NavbarItem
{
    private const string DefaultActiveClass = "active";
    private bool _selected;

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
        }
    }

    [Parameter]
    public bool IsExpended { get; set; } = false;

    internal string ExpendedName => IsExpended.ToString().ToLowerInvariant();

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
    internal async Task OnClickHandler()
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
