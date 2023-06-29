namespace DevToys.Blazor.Pages;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    [Import]
    private IThemeListener ThemeListener { get; set; } = default!;

    [Import]
    private ISettingsProvider SettingsProvider { get; set; } = default!;

    [Import]
    private IFontProvider FontProvider { get; set; } = default!;

    [Parameter]
    public string ThemeName { get; set; } = default!;

    [Parameter]
    public bool IsCompactMode { get; set; }

    [Parameter]
    public bool UserIsCompactModePreference { get; set; }

    [Parameter]
    public string Class { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();

        SetDefaultTextEditorFont();
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
            if (OperatingSystem.IsWindows())
            {
                themeName = "windows-dark-theme";
            }
            else
            {
                themeName = "macos-dark-theme";
            }
        }
        else
        {
            if (OperatingSystem.IsWindows())
            {
                themeName = "windows-light-theme";
            }
            else
            {
                themeName = "macos-light-theme";
            }
        }

        ThemeName = themeName;
        IsCompactMode = ThemeListener.IsCompactMode;
        UserIsCompactModePreference = ThemeListener.UserIsCompactModePreference;
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

    private void SetDefaultTextEditorFont()
    {
        string? currentFontName = SettingsProvider.GetSetting(PredefinedSettings.TextEditorFont); // By default, the value is null.
        string[] systemFontFamilies = FontProvider.GetFontFamilies();
        if (!systemFontFamilies.Contains(currentFontName))
        {
            for (int i = 0; i < PredefinedSettings.DefaultFonts.Length; i++)
            {
                if (systemFontFamilies.Contains(PredefinedSettings.DefaultFonts[i]))
                {
                    SettingsProvider.SetSetting(PredefinedSettings.TextEditorFont, PredefinedSettings.DefaultFonts[i]);
                    return;
                }
            }
        }
    }
}
