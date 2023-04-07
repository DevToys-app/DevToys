using DevToys.Api.Core.Theme;
using DevToys.Business.ViewModels;
using DevToys.MauiBlazor.Components;
using Microsoft.Fast.Components.FluentUI;

namespace DevToys.MauiBlazor.Shared;

public partial class MainLayout : MefLayoutComponentBase
{
    [Import]
    private IThemeListener ThemeListener { get; set; } = default!;

    [Import]
    internal MainWindowViewModel ViewModel { get; set; } = default!;

    [Inject]
    private GlobalState GlobalState { get; set; } = default!;

    private float _baseLayerLuminanceValue;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ThemeListener.ThemeChanged += ThemeListener_ThemeChanged;

            ApplyTheme();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void ThemeListener_ThemeChanged(object? sender, EventArgs e)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        StandardLuminance theme;
        if (ThemeListener.ActualAppTheme == ApplicationTheme.Dark)
        {
            theme = StandardLuminance.DarkMode;
        }
        else
        {
            theme = StandardLuminance.LightMode;
        }

        _baseLayerLuminanceValue = theme.GetLuminanceValue();
        GlobalState.SetLuminance(theme);

        StateHasChanged();
    }
}
