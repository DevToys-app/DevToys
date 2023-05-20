using DevToys.Api;
using DevToys.Api.Core.Theme;

namespace DevToys.MauiBlazor.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    [Import]
    private IThemeListener ThemeListener { get; set; } = default!;

    [Parameter]
    public string ThemeName { get; set; } = default!;

    [Parameter]
    public string Class { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
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
        Class = "theme-transition";

        string themeName;
        if (ThemeListener.ActualAppTheme == ApplicationTheme.Dark)
        {
            themeName = "windows-dark-theme";
        }
        else
        {
            themeName = "windows-light-theme";
        }

        ThemeName = themeName;
        StateHasChanged();

        Task.Delay(1000).ContinueWith(t =>
        {
            InvokeAsync(() =>
            {
                Class = "";
                StateHasChanged();
            });
        });
    }
}
