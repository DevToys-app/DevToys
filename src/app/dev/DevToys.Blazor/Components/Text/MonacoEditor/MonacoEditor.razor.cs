using System.Text.Json;
using System.Text.Json.Serialization;
using DevToys.Blazor.Components.Monaco.Editor;

namespace DevToys.Blazor.Components;

public partial class MonacoEditor : RicherMonacoEditorBase
{
    private readonly object _lock = new object();
    private bool _isLoaded;
    private bool _oldIsActuallyEnabled;
    private bool _oldReadOnlyState;

    [Import]
    internal IThemeListener ThemeListener { get; set; } = default!;

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public Func<MonacoEditor, StandaloneEditorConstructionOptions>? ConstructionOptions { get; set; }

    private bool ShowLoading { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ThemeListener.ThemeChanged += ThemeListener_ThemeChanged;
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
        if (ThemeListener is not null)
        {
            ThemeListener.ThemeChanged -= ThemeListener_ThemeChanged;
        }

        return base.DisposeAsync();
    }

    internal ValueTask<bool> UpdateOptionsAsync(EditorUpdateOptions newOptions)
    {
        // Convert the options object into a JsonElement to get rid of the properties with null values
        string optionsJson = JsonSerializer.Serialize(newOptions, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        JsonElement optionsDict = JsonSerializer.Deserialize<JsonElement>(optionsJson);
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.updateOptions", Id, optionsDict);
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
        Guard.IsNotNull(ThemeListener);
        return ThemeListener.ActualAppTheme == ApplicationTheme.Dark ? BuiltinTheme.VsDark : BuiltinTheme.Vs;
    }

    private async Task DisplayLoadingIfSlowLoadingAsync()
    {
        // Let's not show the progress ring during the first 100ms. We know customers tends to perceive anything
        // faster than 100ms as "instant", so no need to bother the user with a very shortly displayed progress ring
        // if we succeed to load within 100ms.
        // https://psychology.stackexchange.com/questions/1664/what-is-the-threshold-where-actions-are-perceived-as-instant

        const int delay = 100;

        await Task.Delay(delay);

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
