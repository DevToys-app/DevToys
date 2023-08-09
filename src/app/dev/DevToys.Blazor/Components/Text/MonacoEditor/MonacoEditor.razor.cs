using DevToys.Blazor.Components.Monaco.Editor;
using DevToys.Core.Settings;

namespace DevToys.Blazor.Components;

public partial class MonacoEditor : RicherMonacoEditorBase
{
    private readonly object _lock = new();
    private bool _isLoaded;
    private bool _oldIsActuallyEnabled;
    private bool _oldReadOnlyState;

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IThemeListener _themeListener = default!;

    [Import]
    private ISettingsProvider _settingsProvider = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    public MonacoEditor()
    {
    }

    public MonacoEditor(IJSRuntime? jsRuntime = null, string? id = null, string? @class = null)
        : base(jsRuntime)
    {
        if (!string.IsNullOrEmpty(id))
        {
            Id = id;
        }

        Class = @class ?? string.Empty;
    }

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public Func<MonacoEditor, StandaloneEditorConstructionOptions>? ConstructionOptions { get; set; }

    private bool ShowLoading { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _themeListener.ThemeChanged += ThemeListener_ThemeChanged;
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;
        _oldIsActuallyEnabled = IsActuallyEnabled;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            InvokeAsync(DisplayLoadingIfSlowLoadingAsync).Forget();

            // Get desired options
            StandaloneEditorConstructionOptions options = ConstructionOptions?.Invoke(this) ?? new();

            // Overwrite some important options.
            options.Theme = GetTheme();
            options.ReadOnly = options.ReadOnly.GetValueOrDefault(false) || !IsActuallyEnabled;
            options.AutomaticLayout = true;
            options.GlyphMargin = false;
            options.MouseWheelZoom = false;
            options.OverviewRulerBorder = false;
            options.ScrollBeyondLastLine = false;
            options.FontLigatures = false;
            options.SnippetSuggestions = "none";
            options.CodeLens = false;
            options.QuickSuggestions = new QuickSuggestionsOptions { Comments = false, Other = false, Strings = false };
            options.WordBasedSuggestions = false;
            options.Minimap = new EditorMinimapOptions { Enabled = false };
            options.Hover = new EditorHoverOptions { Enabled = false };
            options.MatchBrackets = "always";
            options.BracketPairColorization = new BracketPairColorizationOptions { Enabled = true };
            options.RenderLineHighlightOnlyWhenFocus = true;

            // Apply global user settings
            options.FontFamily = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
            options.WordWrap = _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? "on" : "off";
            options.LineNumbers = _settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? "on" : "off";
            options.RenderLineHighlight = _settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? "all" : "none";
            options.RenderWhitespace = _settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? "all" : "none";

            // Create the editor
            await MonacoEditorHelper.CreateMonacoEditorInstanceAsync(JSRuntime, Id, options, null, Reference);
        }

        if (IsActuallyEnabled != _oldIsActuallyEnabled)
        {
            _oldIsActuallyEnabled = IsActuallyEnabled;

            var options = new EditorUpdateOptions();
            if (IsActuallyEnabled)
            {
                options.ReadOnly = _oldReadOnlyState;
            }
            else
            {
                EditorOptions currentOptions = await this.GetRawOptionsAsync();
                _oldReadOnlyState = currentOptions.ReadOnly.GetValueOrDefault(false);
                options.ReadOnly = true;
            }

            await UpdateOptionsAsync(options);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public override ValueTask DisposeAsync()
    {
        if (_themeListener is not null)
        {
            _themeListener.ThemeChanged -= ThemeListener_ThemeChanged;
        }

        if (_settingsProvider is not null)
        {
            _settingsProvider.SettingChanged -= SettingsProvider_SettingChanged;
        }

        return base.DisposeAsync();
    }

    protected override void OnEditorLoaded()
    {
        base.OnEditorLoaded();
        lock (_lock)
        {
            _isLoaded = true;
            ShowLoading = false;
            StateHasChanged();
        }
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        var options = new EditorUpdateOptions()
        {
            FontFamily = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont),
            WordWrap = _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? "on" : "off",
            LineNumbers = _settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? "on" : "off",
            RenderLineHighlight = _settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? "all" : "none",
            RenderWhitespace = _settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? "all" : "none"
        };

        UpdateOptionsAsync(options);
    }

    private void ThemeListener_ThemeChanged(object? sender, EventArgs e)
    {
        var options = new EditorUpdateOptions()
        {
            Theme = GetTheme()
        };

        UpdateOptionsAsync(options);
    }

    private string GetTheme()
    {
        Guard.IsNotNull(_themeListener);
        return _themeListener.ActualAppTheme == ApplicationTheme.Dark ? BuiltinTheme.VsDark : BuiltinTheme.Vs;
    }

    private async Task DisplayLoadingIfSlowLoadingAsync()
    {
        // Let's not show the progress ring during the first 100ms. We know customers tends to perceive anything
        // faster than 100ms as "instant", so no need to bother the user with a very shortly displayed progress ring
        // if we succeed to load within 100ms.
        // https://psychology.stackexchange.com/questions/1664/what-is-the-threshold-where-actions-are-perceived-as-instant

        const int Delay = 100;

        await Task.Delay(Delay);

        lock (_lock)
        {
            if (!_isLoaded)
            {
                ShowLoading = true;
                StateHasChanged();
            }
        }
    }
}
