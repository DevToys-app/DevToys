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
#pragma warning restore IDE0044 // Add readonly modifier

    public MonacoEditor()
    {
    }

    public MonacoEditor(IJSRuntime? jsRuntime = null, string? id = null, string? @class = null, ISettingsProvider? settingsProvider = null)
        : base(jsRuntime, settingsProvider)
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

    [Parameter]
    public UITextWrapMode? WrapMode { get; set; }

    [Parameter]
    public UITextLineNumber? LineNumberMode { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _themeListener.ThemeChanged += ThemeListener_ThemeChanged;
        SettingsProvider.SettingChanged += SettingsProvider_SettingChanged;
        _oldIsActuallyEnabled = IsActuallyEnabled;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SettingsProvider is not null)
        {
            SettingsProvider_SettingChanged(SettingsProvider, null!);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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
            options.FontLigatures = true;
            options.SnippetSuggestions = "none";
            options.CodeLens = false;
            options.QuickSuggestions = new QuickSuggestionsOptions { Comments = false, Other = false, Strings = false };
            options.WordBasedSuggestions = false;
            options.Minimap = new EditorMinimapOptions { Enabled = false };
            options.Hover = new EditorHoverOptions { Enabled = true, Above = false };
            options.MatchBrackets = "always";
            options.BracketPairColorization = new BracketPairColorizationOptions { Enabled = true };
            options.RenderLineHighlightOnlyWhenFocus = true;

            // Apply global user settings
            options.FontFamily = SettingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
            options.RenderLineHighlight = SettingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? "all" : "none";
            options.RenderWhitespace = SettingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? "all" : "none";
            ApplyWordWrapOption(options);
            ApplyLineNumberOption(options);

            // Create the editor
            await MonacoEditorHelper.CreateMonacoEditorInstanceAsync(JSRuntime, Id, options, null, Reference);
        }

        if (IsActuallyEnabled != _oldIsActuallyEnabled && !_isDisposed)
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

            if (SettingsProvider is not null)
            {
                SettingsProvider_SettingChanged(SettingsProvider, null!);
            }

            StateHasChanged();
        }
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (_isLoaded)
        {
            var options = new EditorUpdateOptions()
            {
                FontFamily = SettingsProvider.GetSetting(PredefinedSettings.TextEditorFont),
                RenderLineHighlight = SettingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? "all" : "none",
                RenderWhitespace = SettingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? "all" : "none",
            };

            ApplyWordWrapOption(options);
            ApplyLineNumberOption(options);

            UpdateOptionsAsync(options).Forget();
        }
    }

    private void ThemeListener_ThemeChanged(object? sender, EventArgs e)
    {
        var options = new EditorUpdateOptions()
        {
            Theme = GetTheme()
        };

        UpdateOptionsAsync(options).Forget();
    }

    private string GetTheme()
    {
        Guard.IsNotNull(_themeListener);
        return _themeListener.ActualAppTheme == ApplicationTheme.Dark ? BuiltinTheme.VsDark : BuiltinTheme.Vs;
    }

    private void ApplyWordWrapOption(EditorOptions editorOptions)
    {
        switch (WrapMode)
        {
            case UITextWrapMode.Auto:
                editorOptions.WordWrap = SettingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? "on" : "off";
                break;
            case UITextWrapMode.Wrap:
                editorOptions.WordWrap = "on";
                break;
            case UITextWrapMode.NoWrap:
                editorOptions.WordWrap = "off";
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void ApplyLineNumberOption(EditorOptions editorOptions)
    {
        switch (LineNumberMode)
        {
            case UITextLineNumber.Auto:
                editorOptions.LineNumbers = SettingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? "on" : "off";
                break;
            case UITextLineNumber.Show:
                editorOptions.LineNumbers = "on";
                break;
            case UITextLineNumber.Hide:
                editorOptions.LineNumbers = "off";
                break;
            default:
                throw new NotSupportedException();
        }
    }
}
