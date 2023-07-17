namespace DevToys.Blazor.Components.UIElements;

public partial class UISettingGroupPresenter : ComponentBase, IDisposable
{
    private readonly List<IUISetting> _settings = new();

    [Parameter]
    public IUISettingGroup UISettingGroup { get; set; } = default!;

    [Parameter]
    public string? Description { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UpdateDescription();
        UpdateSettingStateDescriptionSubscription();
        UISettingGroup.ChildrenChanged += UISettingGroup_ChildrenChanged;
        UISettingGroup.DescriptionChanged += UISettingGroup_DescriptionChanged;
    }

    public void Dispose()
    {
        UISettingGroup.ChildrenChanged -= UISettingGroup_ChildrenChanged;
        UISettingGroup.DescriptionChanged -= UISettingGroup_DescriptionChanged;

        for (int i = 0; i < _settings.Count; i++)
        {
            _settings[i].StateDescriptionChanged -= Setting_StateDescriptionChanged;
        }
    }

    private void UpdateDescription()
    {
        if (!string.IsNullOrEmpty(UISettingGroup.Description))
        {
            Description = UISettingGroup.Description;
        }
        else if (UISettingGroup.Children is not null)
        {
            var stateDescriptions = new List<string>();
            for (int i = 0; i < UISettingGroup.Children.Length; i++)
            {
                IUIElement element = UISettingGroup.Children[i];
                if (element is IUISetting setting && !string.IsNullOrWhiteSpace(setting.StateDescription))
                {
                    stateDescriptions.Add(setting.StateDescription);
                }
            }

            string stateDescription = string.Join(", ", stateDescriptions);
            Description = stateDescription;
        }

        StateHasChanged();
    }

    private void UpdateSettingStateDescriptionSubscription()
    {
        for (int i = 0; i < _settings.Count; i++)
        {
            _settings[i].StateDescriptionChanged -= Setting_StateDescriptionChanged;
        }

        _settings.Clear();

        if (UISettingGroup.Children is not null)
        {
            for (int i = 0; i < UISettingGroup.Children.Length; i++)
            {
                IUIElement element = UISettingGroup.Children[i];
                if (element is IUISetting setting)
                {
                    _settings.Add(setting);
                    setting.StateDescriptionChanged -= Setting_StateDescriptionChanged;
                    setting.StateDescriptionChanged += Setting_StateDescriptionChanged;
                }
            }
        }
    }

    private void UISettingGroup_ChildrenChanged(object? sender, EventArgs e)
    {
        UpdateSettingStateDescriptionSubscription();
    }

    private void UISettingGroup_DescriptionChanged(object? sender, EventArgs e)
    {
        UpdateDescription();
    }

    private void Setting_StateDescriptionChanged(object? sender, EventArgs e)
    {
        UpdateDescription();
    }
}
