using DevToys.Blazor.Components;
using DevToys.Core.Settings;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Pages.Dialogs;

public partial class FirstStartDialog : MefComponentBase
{
    private CheckBox _checkUpdateCheckBox = default!;
    private Button _continueButton = default!;
    private Dialog _dialog = default!;

    [Import]
    private ISettingsProvider SettingsProvider { get; set; } = default!;

    internal Task OpenAsync()
    {
        _dialog.TryOpen();
        return _dialog.WaitUntilClosedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _continueButton.IsEnabled = false;
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }

    private void IsAgreeCheckBoxCheckedChanged(bool isChecked)
    {
        _continueButton.IsEnabled = isChecked;
    }

    private void OnContinueButtonClick(MouseEventArgs ev)
    {
        SettingsProvider.SetSetting(PredefinedSettings.CheckForUpdate, _checkUpdateCheckBox.IsChecked);
        SettingsProvider.SetSetting(PredefinedSettings.IsFirstStart, false);
        _dialog.Close();
    }
}
