using DevToys.Blazor.Components.Monaco.Editor;
using DevToys.Core.Settings;

namespace DevToys.Blazor.Components;

public partial class MonacoEditorDiff : RicherMonacoEditorDiffBase
{
    private readonly object _lock = new();
    private bool _isLoaded;
    private bool _oldIsActuallyEnabled;
    private bool _oldReadOnlyState;

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IThemeListener _themeListener = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public Func<MonacoEditorDiff, StandaloneDiffEditorConstructionOptions>? ConstructionOptions { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _themeListener.ThemeChanged += ThemeListener_ThemeChanged;
        SettingsProvider.SettingChanged += SettingsProvider_SettingChanged;
        _oldIsActuallyEnabled = IsActuallyEnabled;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Guard.IsNull(OriginalEditor);
            Guard.IsNull(ModifiedEditor);

            // Get desired options
            StandaloneDiffEditorConstructionOptions options = ConstructionOptions?.Invoke(this) ?? new();

            // Overwrite some important options.
            options.Theme = GetTheme();
            options.AutomaticLayout = true;
            options.GlyphMargin = false;
            options.MouseWheelZoom = false;
            options.OverviewRulerBorder = false;
            options.ScrollBeyondLastLine = false;
            options.FontLigatures = true;
            options.SnippetSuggestions = "none";
            options.CodeLens = false;
            options.QuickSuggestions = new QuickSuggestionsOptions { Comments = false, Other = false, Strings = false };
            options.Minimap = new EditorMinimapOptions { Enabled = false };
            options.Hover = new EditorHoverOptions { Enabled = true, Above = false };
            options.MatchBrackets = "always";
            options.BracketPairColorization = new BracketPairColorizationOptions { Enabled = true };
            options.RenderLineHighlightOnlyWhenFocus = true;

            options.DiffCodeLens = false;
            options.RenderOverviewRuler = true;

            // Apply global user settings
            options.FontFamily = SettingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
            options.WordWrap = SettingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? "on" : "off";
            options.LineNumbers = SettingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? "on" : "off";
            options.RenderLineHighlight = SettingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? "all" : "none";
            options.RenderWhitespace = SettingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? "all" : "none";

            // Create the bridges for the inner editors
            Guard.IsNotNull(SettingsProvider);
            _originalEditor = MonacoEditorHelper.CreateVirtualEditor(JSRuntime, Id + "_original", settingsProvider: SettingsProvider);
            _modifiedEditor = MonacoEditorHelper.CreateVirtualEditor(JSRuntime, Id + "_modified", settingsProvider: SettingsProvider);

            Guard.IsNotNull(OriginalEditor);
            Guard.IsNotNull(ModifiedEditor);

            // Create the editor
            await MonacoEditorHelper.CreateMonacoEditorDiffInstanceAsync(
                JSRuntime,
                Id,
                options,
                null,
                Reference,
                OriginalEditor.Reference,
                ModifiedEditor.Reference);
        }

        await base.OnAfterRenderAsync(firstRender);

        if (IsActuallyEnabled != _oldIsActuallyEnabled)
        {
            _oldIsActuallyEnabled = IsActuallyEnabled;

            var options = new DiffEditorOptions();
            if (IsActuallyEnabled)
            {
                options.ReadOnly = _oldReadOnlyState;
            }
            else
            {
                Guard.IsNotNull(ModifiedEditor);
                EditorOptions currentOptions = await this.ModifiedEditor.GetRawOptionsAsync();
                _oldReadOnlyState = currentOptions.ReadOnly.GetValueOrDefault(false);
                options.ReadOnly = true;
            }

            await UpdateOptionsAsync(options);
        }
    }

    public override ValueTask DisposeAsync()
    {
        if (_themeListener is not null)
        {
            _themeListener.ThemeChanged -= ThemeListener_ThemeChanged;
        }

        if (SettingsProvider is not null)
        {
            SettingsProvider.SettingChanged -= SettingsProvider_SettingChanged;
        }

        return base.DisposeAsync();
    }

    protected override void OnEditorLoaded()
    {
        base.OnEditorLoaded();
        lock (_lock)
        {
            _isLoaded = true;
            StateHasChanged();
        }
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        var options = new DiffEditorOptions()
        {
            FontFamily = SettingsProvider.GetSetting(PredefinedSettings.TextEditorFont),
            WordWrap = SettingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? "on" : "off",
            LineNumbers = SettingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? "on" : "off",
            RenderLineHighlight = SettingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? "all" : "none",
            RenderWhitespace = SettingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? "all" : "none"
        };

        UpdateOptionsAsync(options).Forget();
    }

    private void ThemeListener_ThemeChanged(object? sender, EventArgs e)
    {
        MonacoEditorHelper.SetThemeAsync(JSRuntime, GetTheme());
    }

    private string GetTheme()
    {
        Guard.IsNotNull(_themeListener);
        return _themeListener.ActualAppTheme == ApplicationTheme.Dark ? BuiltinTheme.VsDark : BuiltinTheme.Vs;
    }
}
