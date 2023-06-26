using DevToys.Api;
using DevToys.Blazor.Components;
using DevToys.Business.ViewModels;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.Blazor.Pages.SubPages;

public partial class ToolPage : MefComponentBase
{
    [Import]
    internal ToolPageViewModel ViewModel { get; set; } = default!;

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; } = default!;

    internal bool IsCompactModeEnabled;

    [Parameter]
    public GuiToolViewItem? GuiToolViewItem { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        IsCompactModeEnabled = SettingsProvider.GetSetting(PredefinedSettings.CompactMode);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (GuiToolViewItem is not null)
        {
            ViewModel.Load(GuiToolViewItem);
        }
    }

    private void OnCompactModeChanged()
    {
        bool newValue = !SettingsProvider.GetSetting(PredefinedSettings.CompactMode);
        SettingsProvider.SetSetting(PredefinedSettings.CompactMode, newValue);
        IsCompactModeEnabled = newValue;
    }
}
