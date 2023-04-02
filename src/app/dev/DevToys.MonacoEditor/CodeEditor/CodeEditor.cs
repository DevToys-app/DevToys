using System.Reflection;
using System.Runtime.CompilerServices;
using DevToys.Api;
using DevToys.MonacoEditor.Extensions;
using DevToys.MonacoEditor.Monaco;
using DevToys.MonacoEditor.Monaco.Editor;
using DevToys.MonacoEditor.Monaco.Helpers;
using DevToys.MonacoEditor.WebInterop;
using DevToys.UI.Framework.Controls;
using DevToys.UI.Framework.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Uno.Extensions;
using Windows.Foundation;
using Windows.UI;
using static System.Net.Mime.MediaTypeNames;
using Range = DevToys.MonacoEditor.Monaco.Range;

namespace DevToys.MonacoEditor;

/// <summary>
/// .NET wrapper for the Monaco CodeEditor
/// https://microsoft.github.io/monaco-editor/
/// </summary>
[TemplatePart(Name = "View", Type = typeof(ICodeEditorPresenter))]
[TemplateVisualState(Name = NormalState, GroupName = CommonStates)]
[TemplateVisualState(Name = PointerOverState, GroupName = CommonStates)]
[TemplateVisualState(Name = FocusedState, GroupName = CommonStates)]
[TemplateVisualState(Name = DisabledState, GroupName = CommonStates)]
public sealed partial class CodeEditor : Control, IParentAccessorAcceptor, IDisposable, IMonacoEditor
{
    internal const string CommonStates = "CommonStates";
    internal const string NormalState = "Normal";
    internal const string PointerOverState = "PointerOver";
    internal const string FocusedState = "Focused";
    internal const string DisabledState = "Disabled";

    private static readonly IModelDecorationOptions HighlightedSpanStyle
        = new IModelDecorationOptions()
        {
            ClassName = new CssLineStyle()
            {
                BackgroundColor = Color.FromArgb(85, 234, 92, 0) // #55EA5C00
            }
        };

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider = Parts.SettingsProvider;
    private readonly DebugLogger _debugLogger = new();
    private readonly ThemeListener _themeListener = new();
    private readonly CssStyleBroker _cssBroker;

    private IReadOnlyList<TextSpan>? _spansToHighlight;
    private ICodeEditorPresenter? _view;
    private int _focusCount;
    private bool _initialized;
    private bool _refrainFromUpdatingOptionsInternally;
    private ModelHelper? _model;

    public CodeEditor()
    {
        _logger = this.Log();
        DefaultStyleKey = typeof(CodeEditor);

        ParentAccessor = new ParentAccessor(this);
        ParentAccessor.AddAssemblyForTypeLookup(typeof(Range).GetTypeInfo().Assembly);
        ParentAccessor.AddAssemblyForTypeLookup(typeof(TextSpan).GetTypeInfo().Assembly);
        ParentAccessor.RegisterAction("Loaded", OnMonacoEditorLoaded);
        ParentAccessor.RegisterAction("GotFocus", OnMonacoEditorGotFocus);
        ParentAccessor.RegisterAction("LostFocus", OnMonacoEditorLostFocus);

        _themeListener.ThemeChanged += ThemeListener_ThemeChanged;

        _cssBroker = new CssStyleBroker(this);

        Options = new StandaloneEditorConstructionOptions();
        DiffOptions = new DiffEditorConstructionOptions();

        Options.GlyphMargin = false;
        DiffOptions.GlyphMargin = false;

        Options.PropertyChanged += Options_PropertyChanged;
        DiffOptions.PropertyChanged += DiffOptions_PropertyChanged;
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;
    }

    public static DependencyProperty IsEditorLoadedProperty { get; }
        = DependencyProperty.Register(
            nameof(IsEditorLoaded),
            typeof(string),
            typeof(CodeEditor),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets whether the editor has been loaded or not.
    /// </summary>
    public bool IsEditorLoaded
    {
        get => (bool)GetValue(IsEditorLoadedProperty);
        private set => SetValue(IsEditorLoadedProperty, value);
    }

    public static DependencyProperty IsDiffViewModeProperty { get; }
        = DependencyProperty.Register(
            nameof(IsDiffViewMode),
            typeof(bool),
            typeof(CodeEditor),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the CodeEditor is in Diff View mode.
    /// </summary>
    public bool IsDiffViewMode
    {
        get => (bool)GetValue(IsDiffViewModeProperty);
        set => SetValue(IsDiffViewModeProperty, value);
    }

    public static DependencyProperty TextProperty { get; }
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CodeEditor),
            new PropertyMetadata(
                string.Empty,
                async (d, e) =>
                {
                    if (d is CodeEditor codeEditor && codeEditor._initialized)
                    {
                        if (!codeEditor.IsSettingValue)
                        {
                            // link:otherScriptsToBeOrganized.ts:updateContent
                            await codeEditor.InvokeScriptAsync("updateContent", e.NewValue.ToString() ?? string.Empty);
                        }
                        else
                        {
                            codeEditor.TextChanged?.Invoke(d, EventArgs.Empty);
                        }
                    }
                }));

    /// <summary>
    /// Gets or sets the CodeEditor Text.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static DependencyProperty ReadOnlyProperty { get; }
        = DependencyProperty.Register(
            nameof(ReadOnly),
            typeof(bool),
            typeof(CodeEditor),
            new PropertyMetadata(
                false,
                (d, e) =>
                {
                    if (d is not CodeEditor editor)
                    {
                        return;
                    }

                    if (editor.Options != null && editor._initialized)
                    {
                        editor.Options.ReadOnly = bool.Parse(e.NewValue?.ToString() ?? "false");
                    }

                    if (editor.DiffOptions != null && editor._initialized)
                    {
                        editor.DiffOptions.OriginalEditable = !bool.Parse(e.NewValue?.ToString() ?? "false");
                        editor.DiffOptions.ReadOnly = bool.Parse(e.NewValue?.ToString() ?? "false");
                    }
                }));

    public static DependencyProperty SelectedSpanProperty { get; }
        = DependencyProperty.Register(
            nameof(SelectedSpan),
            typeof(TextSpan),
            typeof(CodeEditor),
            new PropertyMetadata(
                new TextSpan(0, 0),
                async (d, e) =>
                {
                    if (d is CodeEditor codeEditor && codeEditor._initialized)
                    {
                        if (!codeEditor.IsSettingValue)
                        {
                            // link:updateSelection.ts:updateSelectedSpan
                            await codeEditor.InvokeScriptAsync("updateSelectedSpan", e.NewValue ?? new TextSpan(0, 0));
                        }
                        else
                        {
                            codeEditor.SelectedSpanChanged?.Invoke(d, EventArgs.Empty);
                        }
                    }
                }));

    /// <summary>
    /// Gets the current selection in the editor, or replace the current selection by the given span.
    /// </summary>
    public TextSpan SelectedSpan
    {
        get => (TextSpan)GetValue(SelectedSpanProperty);
        set => SetValue(SelectedSpanProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the editor is read-only.
    /// </summary>
    public bool ReadOnly
    {
        get => (bool)GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    public static DependencyProperty CodeLanguageProperty { get; }
        = DependencyProperty.Register(
            nameof(CodeLanguage),
            typeof(string),
            typeof(CodeEditor),
            new PropertyMetadata(
                string.Empty,
                (d, e) =>
                {
                    if (d is not CodeEditor editor)
                    {
                        return;
                    }

                    if (editor.Options != null && editor._initialized)
                    {
                        editor.Options.Language = e.NewValue.ToString();
                        editor.Options.Folding = !string.IsNullOrWhiteSpace(editor.Options.Language) && !string.Equals("text", editor.Options.Language, StringComparison.OrdinalIgnoreCase);
                    }
                }));

    /// <summary>
    /// Set the Syntax Language for the editor.
    /// Note: Most likely to change or move location.
    /// </summary>
    public string? CodeLanguage
    {
        get => (string?)GetValue(CodeLanguageProperty);
        set => SetValue(CodeLanguageProperty, value);
    }

    public static DependencyProperty OptionsProperty { get; } = DependencyProperty.Register(
        nameof(Options),
        typeof(StandaloneEditorConstructionOptions),
        typeof(CodeEditor),
        new PropertyMetadata(
            null,
            (d, e) =>
            {
                if (d is CodeEditor editor)
                {
                    if (e.OldValue is StandaloneEditorConstructionOptions oldValue)
                    {
                        oldValue.PropertyChanged -= editor.Options_PropertyChanged;
                    }

                    if (e.NewValue is StandaloneEditorConstructionOptions value)
                    {
                        value.PropertyChanged += editor.Options_PropertyChanged;
                    }
                }
            }));

    /// <summary>
    /// Gets the CodeEditor Options.
    /// </summary>
    public StandaloneEditorConstructionOptions Options
    {
        get => (StandaloneEditorConstructionOptions)GetValue(OptionsProperty);
        private set => SetValue(OptionsProperty, value);
    }

    public static DependencyProperty DiffLeftTextProperty { get; }
        = DependencyProperty.Register(
            nameof(DiffLeftText),
            typeof(string),
            typeof(CodeEditor),
            new PropertyMetadata(
                string.Empty,
                (d, e) =>
                {
                    var codeEditor = (CodeEditor)d;
                    if (!codeEditor.IsSettingValue && codeEditor.IsDiffViewMode)
                    {
                        _ = codeEditor.InvokeScriptAsync("updateDiffContent", new object[] { e.NewValue.ToString() ?? string.Empty, codeEditor.DiffRightText });
                    }
                }));

    /// <summary>
    /// Gets or sets the text to display in the left pane of the side-by-side diff view.
    /// </summary>
    public string DiffLeftText
    {
        get => (string)GetValue(DiffLeftTextProperty);
        set => SetValue(DiffLeftTextProperty, value);
    }

    public static DependencyProperty DiffRightTextProperty { get; }
        = DependencyProperty.Register(
            nameof(DiffRightText),
            typeof(string),
            typeof(CodeEditor),
            new PropertyMetadata(
                string.Empty,
                (d, e) =>
                {
                    var codeEditor = (CodeEditor)d;
                    if (!codeEditor.IsSettingValue && codeEditor.IsDiffViewMode)
                    {
                        _ = codeEditor.InvokeScriptAsync("updateDiffContent", new object[] { codeEditor.DiffLeftText, e.NewValue.ToString() ?? string.Empty });
                    }
                }));

    /// <summary>
    /// Gets or sets the text to display in the right pane of the side-by-side diff view.
    /// </summary>
    public string DiffRightText
    {
        get => (string)GetValue(DiffRightTextProperty);
        set => SetValue(DiffRightTextProperty, value);
    }

    public static DependencyProperty DiffOptionsProperty { get; }
        = DependencyProperty.Register(
            nameof(DiffOptions),
            typeof(DiffEditorConstructionOptions),
            typeof(CodeEditor),
            new PropertyMetadata(
                null,
                (d, e) =>
                {
                    if (d is CodeEditor editor)
                    {
                        if (e.OldValue is DiffEditorConstructionOptions oldValue)
                        {
                            oldValue.PropertyChanged -= editor.DiffOptions_PropertyChanged;
                        }

                        if (e.NewValue is DiffEditorConstructionOptions value)
                        {
                            value.PropertyChanged += editor.DiffOptions_PropertyChanged;
                        }
                    }
                }));

    /// <summary>
    /// Get or set the CodeEditorCore Options.
    /// </summary>
    public DiffEditorConstructionOptions DiffOptions
    {
        get => (DiffEditorConstructionOptions)GetValue(DiffOptionsProperty);
        set => SetValue(DiffOptionsProperty, value);
    }

    public bool IsSettingValue { get; set; }

    public Control UIHost => this;

    internal ParentAccessor ParentAccessor { get; }

    /// <summary>
    /// When Editor is Loaded, it has been rendered and is ready to be displayed.
    /// </summary>
    public event RoutedEventHandler? EditorLoaded;

    /// <summary>
    /// Called when an internal exception is encountered while executing a command. (for testing/reporting issues)
    /// </summary>
    public event TypedEventHandler<CodeEditor, Exception>? InternalException;

    /// <summary>
    /// Called when a link is Ctrl+Clicked on in the editor, set Handled to true to prevent opening.
    /// </summary>
    public event TypedEventHandler<CodeEditor, CoreWebView2NewWindowRequestedEventArgs>? OpenLinkRequested;

    public event EventHandler? TextChanged;

    public event EventHandler? SelectedSpanChanged;

#if HAS_UNO
    public new void Dispose()
#else
    public void Dispose()
#endif
    {
        ParentAccessor.Dispose();
        _cssBroker.Dispose();
    }

    public async Task HighlightSpansAsync(IReadOnlyList<TextSpan>? spans)
    {
        if (!_initialized)
        {
            _spansToHighlight = spans;
            return;
        }

        var newDecorationsAdjust = new List<IModelDeltaDecoration>();

        if (spans is not null)
        {
            for (int i = 0; i < spans.Count; i++)
            {
                TextSpan span = spans[i];
                if (span.StartPosition + span.Length < Text.Length)
                {
                    Position? startPosition = await GetModel().GetPositionAtAsync((uint)span.StartPosition);
                    Position? endPosition = await GetModel().GetPositionAtAsync((uint)(span.StartPosition + span.Length));
                    if (startPosition is not null && endPosition is not null)
                    {
                        newDecorationsAdjust.Add(
                            new IModelDeltaDecoration(
                                new Range(
                                    startPosition.LineNumber,
                                    startPosition.Column,
                                    endPosition.LineNumber,
                                    endPosition.Column),
                                HighlightedSpanStyle));
                    }
                }
            }
        }

        if (_cssBroker.AssociateStyles(newDecorationsAdjust))
        {
            // Update Styles First
            await InvokeScriptAsync("updateStyle", _cssBroker.GetStyles());
        }

        // Send Command to Modify Decorations
        // IMPORTANT: Need to cast to object here as we want this to be a single array object passed as a parameter, not a list of parameters to expand.
        await InvokeScriptAsync("updateDecorations", (object)newDecorationsAdjust);
    }

    internal IModel GetModel()
    {
        return _model ?? throw new NotSupportedException($"Model is not available");
    }

    internal async Task SendScriptAsync(
        string script,
        [CallerMemberName] string? member = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        await SendScriptAsync<object>(script, member, file, line);
    }

    internal async Task<T?> SendScriptAsync<T>(
        string script,
        [CallerMemberName] string? member = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        if (_initialized)
        {
            Guard.IsNotNull(_view);
            try
            {
                return await _view.RunScriptAsync<T>(script, serializeResult: false, member, file, line);
            }
            catch (Exception e)
            {
                LogInternalError(e);
                InternalException?.Invoke(this, e);
            }
        }
        else
        {
            if (Debugger.IsAttached)
            {
                Debug.WriteLine("WARNING: Tried to call '" + script + "' before initialized.");
            }
        }

        return default;
    }

    internal async Task InvokeScriptAsync(
        string method,
        object arg,
        bool serialize = true,
        [CallerMemberName] string? member = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        await InvokeScriptAsync<object>(method, new object[] { arg }, serialize, member, file, line);
    }

    internal async Task InvokeScriptAsync(
        string method,
        object[] args,
        bool serialize = true,
        [CallerMemberName] string? member = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        await InvokeScriptAsync<object>(method, args, serialize, member, file, line);
    }

    internal async Task<T?> InvokeScriptAsync<T>(
        string method,
        object arg,
        bool serialize = true,
        [CallerMemberName] string? member = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        return await InvokeScriptAsync<T?>(method, new object[] { arg }, serialize, member, file, line);
    }

    internal async Task<T?> InvokeScriptAsync<T>(
        string method,
        object[] args,
        bool serialize = true,
        [CallerMemberName] string? member = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        if (_initialized)
        {
            Guard.IsNotNull(_view);
            try
            {
                return await _view.InvokeScriptAsync<T>(method, args, serialize, member, file, line);
            }
            catch (Exception e)
            {
                LogInternalError(e);
                InternalException?.Invoke(this, e);
            }
        }
        else
        {
            if (Debugger.IsAttached)
            {
                Debug.WriteLine("WARNING: Tried to call '" + method + "' before initialized.");
            }
        }

        return default;
    }

    protected override void OnApplyTemplate()
    {
        if (_view is not null)
        {
            _view.NavigationStarting -= WebView_NavigationStarting;
            _view.DOMContentLoaded -= WebView_DOMContentLoaded;
            _view.NavigationCompleted -= WebView_NavigationCompleted;
            _view.NewWindowRequested -= WebView_NewWindowRequested;
            _view.DotNetObjectInjectionRequested -= WebView_DotNetObjectInjectionRequested;
            Debug.WriteLine("Setting initialized - false");
            _initialized = false;
        }

        Guard.IsFalse(_initialized);
        _view = (ICodeEditorPresenter)GetTemplateChild("View");

        if (_view is not null)
        {
            _view.NavigationStarting += WebView_NavigationStarting;
            _view.DOMContentLoaded += WebView_DOMContentLoaded;
            _view.NavigationCompleted += WebView_NavigationCompleted;
            _view.NewWindowRequested += WebView_NewWindowRequested;
            _view.DotNetObjectInjectionRequested += WebView_DotNetObjectInjectionRequested;

            base.OnApplyTemplate();

            _view.LaunchAsync().Forget();
        }
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        GiveFocusToInnerEditor();
    }

    private void Options_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not StandaloneEditorConstructionOptions options || IsDiffViewMode || !_initialized)
        {
            return;
        }

        if (e.PropertyName == nameof(StandaloneEditorConstructionOptions.Language))
        {
            InvokeScriptAsync("updateLanguage", options.Language ?? string.Empty).Forget();
            if (CodeLanguage != options.Language)
            {
                CodeLanguage = options.Language;
            }
        }

        if (e.PropertyName == nameof(StandaloneEditorConstructionOptions.ReadOnly))
        {
            if (ReadOnly != options.ReadOnly)
            {
                options.ReadOnly = ReadOnly;
            }
        }

        if (!_refrainFromUpdatingOptionsInternally)
        {
            InvokeScriptAsync("updateOptions", options).Forget();
        }
    }

    private void DiffOptions_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not DiffEditorConstructionOptions options || !IsDiffViewMode || !_initialized)
        {
            return;
        }

        if (e.PropertyName == nameof(DiffEditorConstructionOptions.ReadOnly))
        {
            if (ReadOnly != options.ReadOnly)
            {
                options.ReadOnly = ReadOnly;
            }
        }

        if (!_refrainFromUpdatingOptionsInternally)
        {
            InvokeScriptAsync("updateDiffOptions", options).Forget();
        }
    }

    private void ThemeListener_ThemeChanged(ThemeListener sender)
    {
        InvokeScriptAsync(
            "setTheme",
            args: new string[] { sender.AccentColorHtmlHex })
            .Forget();
        InvokeScriptAsync(
            "changeTheme",
            args: new object[] { sender.CurrentTheme.ToString(), sender.IsHighContrast, IsFocusEngaged })
            .Forget();
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName.Contains("TextEditor"))
        {
            ApplySettings();
        }
    }

    private async Task WebView_DotNetObjectInjectionRequested(ICodeEditorPresenter sender, EventArgs args)
    {
        Guard.IsNotNull(_view);
        Guard.IsFalse(_initialized);

        var tasks = new List<Task>
        {
            _view.InjectDotNetObjectToWebPageAsync("Debug", _debugLogger),
            _view.InjectDotNetObjectToWebPageAsync("Accessor", ParentAccessor),
            _view.InjectDotNetObjectToWebPageAsync("Theme", _themeListener)
        };

        await Task.WhenAll(tasks);
    }

    private void WebView_NavigationStarting(ICodeEditorPresenter sender, CoreWebView2NavigationStartingEventArgs args)
    {
    }

    private void WebView_NavigationCompleted(ICodeEditorPresenter sender, CoreWebView2NavigationCompletedEventArgs args)
    {
    }

    private void WebView_DOMContentLoaded(ICodeEditorPresenter sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
    }

    private void WebView_NewWindowRequested(ICodeEditorPresenter sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        // TODO: Should probably create own event args here as we don't want to expose the referrer to our internal page?
        OpenLinkRequested?.Invoke(this, args);
    }

    private void OnMonacoEditorGotFocus()
    {
        _focusCount++;
        if (_focusCount > 0)
        {
            InvokeScriptAsync(
                "changeTheme",
                args: new object[] { _themeListener.CurrentTheme.ToString(), _themeListener.IsHighContrast, true })
                .Forget();
            VisualStateManager.GoToState(this, FocusedState, false);
        }
    }

    private void OnMonacoEditorLostFocus()
    {
        _focusCount = Math.Max(0, _focusCount - 1);
        if (_focusCount == 0)
        {
            InvokeScriptAsync(
                "changeTheme",
                args: new object[] { _themeListener.CurrentTheme.ToString(), _themeListener.IsHighContrast, false })
                .Forget();
            VisualStateManager.GoToState(this, NormalState, false);
        }
    }

    private void OnMonacoEditorLoaded()
    {
        _model = new ModelHelper(this);

        _initialized = true;

        // Update theme
        InvokeScriptAsync(
            "setTheme",
            args: new string[] { _themeListener!.AccentColorHtmlHex })
            .Forget();
        InvokeScriptAsync(
            "changeTheme",
            new object[] { _themeListener.CurrentTheme.ToString(), _themeListener.IsHighContrast, IsFocusEngaged })
            .Forget();

        // Update options.
        _refrainFromUpdatingOptionsInternally = true;

        Options.SmoothScrolling = true;
        Options.GlyphMargin = false;
        Options.MouseWheelZoom = false;
        Options.OverviewRulerBorder = false;
        Options.ScrollBeyondLastLine = false;
        Options.FontLigatures = false;
        Options.SnippetSuggestions = SnippetSuggestions.None;
        Options.CodeLens = true;
        Options.QuickSuggestions = false;
        Options.WordBasedSuggestions = false;
        Options.Minimap = new EditorMinimapOptions() { Enabled = false };
        Options.ShowFoldingControls = Show.Always;
        Options.ReadOnly = ReadOnly;
        Options.Language = CodeLanguage;
        Options.Folding = !string.IsNullOrWhiteSpace(CodeLanguage) && !string.Equals("text", CodeLanguage, StringComparison.OrdinalIgnoreCase);
        DiffOptions.SmoothScrolling = true;
        DiffOptions.GlyphMargin = false;
        DiffOptions.MouseWheelZoom = false;
        DiffOptions.OverviewRulerBorder = false;
        DiffOptions.ScrollBeyondLastLine = false;
        DiffOptions.FontLigatures = false;
        DiffOptions.SnippetSuggestions = SnippetSuggestions.None;
        DiffOptions.CodeLens = true;
        DiffOptions.QuickSuggestions = false;
        DiffOptions.ShowFoldingControls = Show.Always;
        DiffOptions.OriginalEditable = !ReadOnly;
        DiffOptions.ReadOnly = ReadOnly;

        ApplySettings();

        _refrainFromUpdatingOptionsInternally = false;
        if (IsDiffViewMode)
        {
            InvokeScriptAsync("updateDiffOptions", DiffOptions).Forget();
        }
        else
        {
            InvokeScriptAsync("updateOptions", Options).Forget();
        }

        // Set text, selection, highlighted spans that may have been set but not sent to the editor yet.
        InvokeScriptAsync("updateContent", Text ?? string.Empty).Forget();
        InvokeScriptAsync("updateSelectedSpan", SelectedSpan).Forget();
        HighlightSpansAsync(_spansToHighlight).Forget();

        // We're done loading Monaco Editor.
        IsEditorLoaded = true;
        EditorLoaded?.Invoke(this, new RoutedEventArgs());
    }

    private void ApplySettings()
    {
        Options.WordWrapMinified = _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
        Options.WordWrap = _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? WordWrap.On : WordWrap.Off;
        Options.LineNumbers = _settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? LineNumbersType.On : LineNumbersType.Off;
        Options.RenderLineHighlight = _settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? RenderLineHighlight.All : RenderLineHighlight.None;
        Options.RenderWhitespace = _settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? RenderWhitespace.All : RenderWhitespace.None;
        Options.FontFamily = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
        DiffOptions.WordWrapMinified = _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
        DiffOptions.WordWrap = _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? WordWrap.On : WordWrap.Off;
        DiffOptions.LineNumbers = _settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? LineNumbersType.On : LineNumbersType.Off;
        DiffOptions.RenderLineHighlight = _settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? RenderLineHighlight.All : RenderLineHighlight.None;
        DiffOptions.RenderWhitespace = _settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? RenderWhitespace.All : RenderWhitespace.None;
        DiffOptions.FontFamily = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
    }

    private void GiveFocusToInnerEditor()
    {
        if (_initialized)
        {
            // Make sure inner editor is focused
            Guard.IsNotNull(_view);
            _view.Focus(FocusState.Programmatic);
            SendScriptAsync("editorContext.editor.focus();").Forget();
        }
    }

    [LoggerMessage(2, LogLevel.Error, "An error occured related to the Monaco Editor.")]
    partial void LogInternalError(Exception ex);
}
