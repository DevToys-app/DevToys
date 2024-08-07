using DevToys.Blazor.Core.Services;
using DevToys.Core.Settings;

namespace DevToys.Blazor.Pages;

public partial class MainLayout : LayoutComponentBase
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IThemeListener _themeListener = default!;

    [Import]
    private ISettingsProvider _settingsProvider = default!;

    [Import]
    private IFontProvider _fontProvider = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    [Inject]
    internal FontService FontService { get; set; } = default!;

    [Parameter]
    public string ThemeName { get; set; } = default!;

    [Parameter]
    public bool IsCompactMode { get; set; }

    [Parameter]
    public bool UserIsCompactModePreference { get; set; }

    [Parameter]
    public bool UseLessAnimations { get; set; }

    [Parameter]
    public string Class { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();

        SetDefaultTextEditorFont();
    }

    protected override Task OnInitializedAsync()
    {
        FontService.ImportThirdPartyFontsAsync().ForgetSafely();
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _themeListener.ThemeChanged += ThemeListener_ThemeChanged;

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
        if (_themeListener.ActualAppTheme == ApplicationTheme.Dark)
        {
            if (OperatingSystem.IsWindows())
            {
                themeName = "windows-dark-theme";
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsIOS())
            {
                themeName = "macos-dark-theme";
            }
            else
            {
                themeName = "linux-dark-theme";
            }
        }
        else
        {
            if (OperatingSystem.IsWindows())
            {
                themeName = "windows-light-theme";
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsIOS())
            {
                themeName = "macos-light-theme";
            }
            else
            {
                themeName = "linux-light-theme";
            }
        }

        ThemeName = themeName;
        IsCompactMode = _themeListener.IsCompactMode;
        UserIsCompactModePreference = _themeListener.UserIsCompactModePreference;
        UseLessAnimations = _themeListener.UseLessAnimations;

        InvokeAsync(() =>
        {
            StateHasChanged();
        });

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
        string? currentFontName = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont); // By default, the value is null.
        string[] systemFontFamilies = _fontProvider.GetFontFamilies();
        if (!systemFontFamilies.Contains(currentFontName))
        {
            for (int i = 0; i < PredefinedSettings.DefaultFonts.Length; i++)
            {
                if (systemFontFamilies.Contains(PredefinedSettings.DefaultFonts[i]))
                {
                    _settingsProvider.SetSetting(PredefinedSettings.TextEditorFont, PredefinedSettings.DefaultFonts[i]);
                    return;
                }
            }
        }
    }
}
