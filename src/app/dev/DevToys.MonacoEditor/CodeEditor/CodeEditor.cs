using System.Reflection;
using System.Runtime.CompilerServices;
using DevToys.Core.Threading;
using DevToys.MonacoEditor.Extensions;
using DevToys.MonacoEditor.Monaco;
using DevToys.MonacoEditor.Monaco.Editor;
using DevToys.MonacoEditor.WebInterop;
using DevToys.MonacoEditor.WebInterop.Parent;
using DevToys.MonacoEditor.WebInterop.Theme;
using DevToys.UI.Framework.Threading;
using Microsoft.Web.WebView2.Core;
using Windows.Foundation;
using Range = DevToys.MonacoEditor.Monaco.Range;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DispatcherQueue = Windows.UI.Core.CoreDispatcher;
#else
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

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
public sealed partial class CodeEditor : Control, IParentAccessorAcceptor, IDisposable
{
    internal const string CommonStates = "CommonStates";
    internal const string NormalState = "Normal";
    internal const string PointerOverState = "PointerOver";
    internal const string FocusedState = "Focused";
    internal const string DisabledState = "Disabled";

#if WINDOWS_UWP
    private DispatcherQueue UIAccess => DispatcherQueueExtensions.DispatcherQueue;
#else
    private DispatcherQueue UIAccess => this.DispatcherQueue;
#endif

    private readonly DebugLogger _debugLogger = new();
    private readonly ThemeListener _themeListener;
    private readonly long _themeToken;

    private ICodeEditorPresenter? _view;
    private int _focusCount;
    private bool _initialized;
    private ModelHelper? _model;

    public CodeEditor()
    {
        DefaultStyleKey = typeof(CodeEditor);

        UIAccess.ThrowIfNotOnUIThread();

        ParentAccessor = new ParentAccessor(this);
        ParentAccessor.AddAssemblyForTypeLookup(typeof(Range).GetTypeInfo().Assembly);
        ParentAccessor.RegisterAction("Loaded", OnMonacoEditorLoaded);
        ParentAccessor.RegisterAction("GotFocus", OnMonacoEditorGotFocus);
        ParentAccessor.RegisterAction("LostFocus", OnMonacoEditorLostFocus);

        _themeListener = new ThemeListener(UIAccess);
        _themeListener.ThemeChanged += ThemeListener_ThemeChanged;
        _themeToken = RegisterPropertyChangedCallback(ActualThemeProperty, ActualTheme_PropertyChanged);

        Options = new StandaloneEditorConstructionOptions();
        DiffOptions = new DiffEditorConstructionOptions();

        Options.GlyphMargin = false;
        DiffOptions.GlyphMargin = false;

        Options.PropertyChanged += Options_PropertyChanged;
        DiffOptions.PropertyChanged += DiffOptions_PropertyChanged;

        Unloaded += CodeEditor_Unloaded;
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
                    if (d is CodeEditor { IsSettingValue: false } codeEditor && codeEditor._initialized)
                    {
                        // link:otherScriptsToBeOrganized.ts:updateContent
                        await codeEditor.InvokeScriptAsync("updateContent", e.NewValue.ToString() ?? string.Empty);
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

    public static DependencyProperty SelectedTextProperty { get; }
        = DependencyProperty.Register(
            nameof(SelectedText),
            typeof(string),
            typeof(CodeEditor),
            new PropertyMetadata(
                string.Empty,
                async (d, e) =>
                {
                    if (d is CodeEditor { IsSettingValue: false } codeEditor && codeEditor._initialized)
                    {
                        // link:updateSelectedContent.ts:updateSelectedContent
                        await codeEditor.InvokeScriptAsync("updateSelectedContent", e.NewValue.ToString() ?? string.Empty);
                    }
                }));

    /// <summary>
    /// Gets the current Primary Selected CodeEditor Text, or replace the current selection by the given text.
    /// </summary>
    public string SelectedText
    {
        get => (string)GetValue(SelectedTextProperty);
        set => SetValue(SelectedTextProperty, value);
    }

    public static DependencyProperty SelectedRangeProperty { get; }
        = DependencyProperty.Register(
            nameof(SelectedRange),
            typeof(Selection),
            typeof(CodeEditor),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the span to select in the editor
    /// </summary>
    public Selection SelectedRange
    {
        get => (Selection)GetValue(SelectedRangeProperty);
        internal set => SetValue(SelectedRangeProperty, value);
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

#if HAS_UNO
    public new void Dispose()
#else
    public void Dispose()
#endif
    {
        ParentAccessor.Dispose();
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
                return await _view.RunScriptAsync<T>(script, member, file, line);
            }
            catch (Exception e)
            {
                InternalException?.Invoke(this, e);
            }
        }
        else
        {
#if DEBUG
            Debug.WriteLine("WARNING: Tried to call '" + script + "' before initialized.");
#endif
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
                InternalException?.Invoke(this, e);
            }
        }
        else
        {
#if DEBUG
            Debug.WriteLine("WARNING: Tried to call '" + method + "' before initialized.");
#endif
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

    private void CodeEditor_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_view != null)
        {
            _view.NavigationStarting -= WebView_NavigationStarting;
            _view.DOMContentLoaded -= WebView_DOMContentLoaded;
            _view.NavigationCompleted -= WebView_NavigationCompleted;
            _view.NewWindowRequested -= WebView_NewWindowRequested;
            _view.DotNetObjectInjectionRequested -= WebView_DotNetObjectInjectionRequested;
        }

        Options.PropertyChanged -= Options_PropertyChanged;
        DiffOptions.PropertyChanged -= DiffOptions_PropertyChanged;
        _themeListener.ThemeChanged -= ThemeListener_ThemeChanged;

        UnregisterPropertyChangedCallback(ActualThemeProperty, _themeToken);

        _initialized = false;
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

        InvokeScriptAsync("updateOptions", options).Forget();
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

        InvokeScriptAsync("updateDiffOptions", options).Forget();
    }

    private void ActualTheme_PropertyChanged(DependencyObject obj, DependencyProperty property)
    {
        ElementTheme theme = ActualTheme;
        string themeName = string.Empty;

        if (theme == ElementTheme.Default)
        {
            themeName = _themeListener.CurrentThemeName;
        }
        else
        {
            themeName = theme.ToString();
        }

        UIAccess.RunOnUIThreadAsync(async () =>
        {
            await InvokeScriptAsync("setTheme", args: new string[] { _themeListener!.AccentColorHtmlHex });
            await InvokeScriptAsync("changeTheme", new string[] { themeName, _themeListener.IsHighContrast.ToString() });
        }).Forget();
    }

    private void ThemeListener_ThemeChanged(ThemeListener sender)
    {
        if (RequestedTheme == ElementTheme.Default)
        {
            UIAccess.RunOnUIThreadAsync(async () =>
            {
                await InvokeScriptAsync("setTheme", args: new string[] { sender.AccentColorHtmlHex });
                await InvokeScriptAsync("changeTheme", args: new string[] { sender.CurrentTheme.ToString(), sender.IsHighContrast.ToString() });
            }).Forget();
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
            VisualStateManager.GoToState(this, FocusedState, false);
        }
    }

    private void OnMonacoEditorLostFocus()
    {
        _focusCount = Math.Max(0, _focusCount - 1);
        if (_focusCount == 0)
        {
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
            new string[] { _themeListener.CurrentTheme.ToString(), _themeListener.IsHighContrast.ToString() })
            .Forget();

        // Update options.
        Options.SmoothScrolling = true;
        Options.Minimap = new EditorMinimapOptions() { Enabled = false };
        Options.ReadOnly = ReadOnly;
        Options.Language = CodeLanguage;
        DiffOptions.OriginalEditable = !ReadOnly;
        DiffOptions.ReadOnly = ReadOnly;

        // We're done loading Monaco Editor.
        IsEditorLoaded = true;
        EditorLoaded?.Invoke(this, new RoutedEventArgs());
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
}
