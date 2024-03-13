using DevToys.Blazor.Components;
using DevToys.Core.Settings;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Pages.Dialogs;

public partial class FirstStartDialog : MefComponentBase
{
    private Button _continueButton = default!;
    private Dialog _dialog = default!;

    [Import]
    private ISettingsProvider SettingsProvider { get; set; } = default!;

    internal void Open()
    {
        _dialog.TryOpen();
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
        SettingsProvider.SetSetting(PredefinedSettings.IsFirstStart, false);
        _dialog.Close();
    }
}
