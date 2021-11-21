#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Collections.Generic;
using DevToys.MonacoEditor.Extensions;
using DevToys.MonacoEditor.Helpers;
using DevToys.MonacoEditor.Monaco;
using DevToys.MonacoEditor.Monaco.Editor;
using DevToys.MonacoEditor.Monaco.Helpers;
using DevToys.Shared.Core.Threading;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DevToys.MonacoEditor.CodeEditorControl
{
    /// <summary>
    /// Action delegate for <see cref="CodeEditorCore.AddCommandAsync(int, CommandHandler)"/> and <see cref="CodeEditorCore.AddCommandAsync(int, CommandHandler, string)"/>.
    /// </summary>
    public delegate void CommandHandler();

    /// <summary>
    /// UWP Windows Runtime Component wrapper for the Monaco CodeEditorCore
    /// https://microsoft.github.io/monaco-editor/
    /// This file contains Monaco IEditor method implementations we can call on our control.
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.ieditor.html
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.icommoncodeeditor.html
    /// </summary>
    [TemplatePart(Name = "WebViewElement", Type = typeof(WebView))]
    [TemplateVisualState(Name = NormalState, GroupName = CommonStates)]
    [TemplateVisualState(Name = PointerOverState, GroupName = CommonStates)]
    [TemplateVisualState(Name = FocusedState, GroupName = CommonStates)]
    [TemplateVisualState(Name = DisabledState, GroupName = CommonStates)]
    public sealed partial class CodeEditorCore : Control, IDisposable, IParentAccessorAcceptor
    {
        internal const string CommonStates = "CommonStates";
        internal const string NormalState = "Normal";
        internal const string PointerOverState = "PointerOver";
        internal const string FocusedState = "Focused";
        internal const string DisabledState = "Disabled";

        private readonly DisposableSempahore _mutexMarkers = new();
        private readonly DisposableSempahore _mutexLineDecorations = new();

        private bool _initialized;
        private WebView? _view;
        private ModelHelper? _model;
        private ThemeListener? _themeListener;
        private KeyboardListener? _keyboardListener;
        private long _themeToken;

        public ParentAccessor? ParentAccessor { get; private set; }

        public bool IsSettingValue { get; set; }

        public static DependencyProperty TextProperty { get; }
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    string.Empty,
                    (d, e) =>
                    {
                        if (!((CodeEditorCore)d).IsSettingValue)
                        {
                            _ = ((CodeEditorCore)d).InvokeScriptAsync("updateContent", e.NewValue.ToString());
                        }
                    }));

        /// <summary>
        /// Get or Set the CodeEditorCore Text.
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
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    string.Empty,
                    (d, e) =>
                    {
                        if (!((CodeEditorCore)d).IsSettingValue)
                        {
                            _ = ((CodeEditorCore)d).InvokeScriptAsync("updateSelectedContent", e.NewValue.ToString());
                        }
                    }));

        /// <summary>
        /// Get the current Primary Selected CodeEditorCore Text.
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
                typeof(CodeEditorCore),
                new PropertyMetadata(null));

        public Selection SelectedRange
        {
            get => (Selection)GetValue(SelectedRangeProperty);
            set => SetValue(SelectedRangeProperty, value);
        }

        internal static DependencyProperty CodeLanguageProperty { get; }
            = DependencyProperty.Register(
                nameof(CodeLanguage),
                typeof(string),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    string.Empty,
                    (d, e) =>
                    {
                        if (d is not CodeEditorCore editor)
                        {
                            return;
                        }

                        if (editor.Options != null && editor.IsEditorLoaded)
                        {
                            editor.Options.Language = e.NewValue.ToString();
                        }
                    }));

        /// <summary>
        /// Set the Syntax Language for the Code CodeEditorCore.
        /// 
        /// Note: Most likely to change or move location.
        /// </summary>
        public string? CodeLanguage
        {
            get => (string?)GetValue(CodeLanguageProperty);
            set => SetValue(CodeLanguageProperty, value);
        }

        internal static DependencyProperty ReadOnlyProperty { get; }
            = DependencyProperty.Register(
                nameof(ReadOnly),
                typeof(bool),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    false,
                    (d, e) =>
                    {
                        if (d is not CodeEditorCore editor)
                        {
                            return;
                        }

                        if (editor.Options != null && editor.IsEditorLoaded)
                        {
                            editor.Options.ReadOnly = bool.Parse(e.NewValue?.ToString() ?? "false");
                        }
                        if (editor.DiffOptions != null && editor.IsEditorLoaded)
                        {
                            editor.DiffOptions.OriginalEditable = !bool.Parse(e.NewValue?.ToString() ?? "false");
                            editor.DiffOptions.ReadOnly = bool.Parse(e.NewValue?.ToString() ?? "false");
                        }
                    }));

        /// <summary>
        /// Set the ReadOnly option for the Code CodeEditorCore.
        /// </summary>
        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }

        public static DependencyProperty OptionsProperty { get; }
            = DependencyProperty.Register(
                nameof(Options),
                typeof(StandaloneEditorConstructionOptions),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    null,
                    (d, e) =>
                    {
                        if (d is CodeEditorCore editor)
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
        /// Get or set the CodeEditorCore Options. Node: Will overwrite CodeLanguage.
        /// </summary>
        public StandaloneEditorConstructionOptions Options
        {
            get => (StandaloneEditorConstructionOptions)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        public static DependencyProperty HasGlyphMarginProperty { get; }
            = DependencyProperty.Register(
                nameof(HasGlyphMargin),
                typeof(bool),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    false,
                    (d, e) =>
                    {
                        ((CodeEditorCore)d).Options.GlyphMargin = e.NewValue as bool?;
                        ((CodeEditorCore)d).DiffOptions.GlyphMargin = e.NewValue as bool?;
                    }));

        /// <summary>
        /// Get or Set the CodeEditorCore Text.
        /// </summary>
        public bool HasGlyphMargin
        {
            get => (bool)GetValue(HasGlyphMarginProperty);
            set => SetValue(HasGlyphMarginProperty, value);
        }

        public static DependencyProperty DecorationsProperty { get; }
            = DependencyProperty.Register(
                nameof(Decorations),
                typeof(ModelDeltaDecoration),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    null,
                    async (d, e) =>
                    {
                        if (d is CodeEditorCore editor)
                        {
                            // We only want to do this one at a time per editor.
                            using (await editor._mutexLineDecorations.WaitAsync(CancellationToken.None))
                            {
                                var old = e.OldValue as IObservableVector<ModelDeltaDecoration>;
                                // Clear out the old line decorations if we're replacing them or setting back to null
                                if ((old != null && old.Count > 0) ||
                                         e.NewValue == null)
                                {
                                    await editor.DeltaDecorationsHelperAsync(null);
                                }

                                if (e.NewValue is IObservableVector<ModelDeltaDecoration> value)
                                {
                                    if (value.Count > 0)
                                    {
                                        await editor.DeltaDecorationsHelperAsync(value.ToArray());
                                    }

                                    value.VectorChanged -= editor.Decorations_VectorChanged;
                                    value.VectorChanged += editor.Decorations_VectorChanged;
                                }
                            }
                        }
                    }));

        private async void Decorations_VectorChanged(IObservableVector<ModelDeltaDecoration> sender, IVectorChangedEventArgs @event)
        {
            if (sender != null)
            {
                // Need to recall mutex as this is called from outside of this initial callback setting it up.
                using (await _mutexLineDecorations.WaitAsync(CancellationToken.None))
                {
                    await DeltaDecorationsHelperAsync(sender.ToArray());
                }
            }
        }

        /// <summary>
        /// Gets or sets text Decorations.
        /// </summary>
        public IObservableVector<ModelDeltaDecoration> Decorations
        {
            get => (IObservableVector<ModelDeltaDecoration>)GetValue(DecorationsProperty);
            set => SetValue(DecorationsProperty, value);
        }

        public static DependencyProperty MarkersProperty { get; }
            = DependencyProperty.Register(
                nameof(Markers),
                typeof(IMarkerData),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    null,
                    async (d, e) =>
                    {
                        if (d is CodeEditorCore editor)
                        {
                            // We only want to do this one at a time per editor.
                            using (await editor._mutexMarkers.WaitAsync(CancellationToken.None))
                            {
                                var old = e.OldValue as IObservableVector<IMarkerData>;
                                // Clear out the old markers if we're replacing them or setting back to null
                                if ((old != null && old.Count > 0) ||
                                         e.NewValue == null)
                                {
                                    // TODO: Can I simplify this in this case?
                                    await editor.SetModelMarkersAsync("CodeEditorCore", Array.Empty<IMarkerData>());
                                }

                                if (e.NewValue is IObservableVector<IMarkerData> value)
                                {
                                    if (value.Count > 0)
                                    {
                                        await editor.SetModelMarkersAsync("CodeEditorCore", value.ToArray());
                                    }

                                    value.VectorChanged -= editor.Markers_VectorChanged;
                                    value.VectorChanged += editor.Markers_VectorChanged;
                                }
                            }
                        }
                    }));

        private async void Markers_VectorChanged(IObservableVector<IMarkerData> sender, IVectorChangedEventArgs @event)
        {
            if (sender != null)
            {
                // Need to recall mutex as this is called from outside of this initial callback setting it up.
                using (await _mutexMarkers.WaitAsync(CancellationToken.None))
                {
                    await SetModelMarkersAsync("CodeEditorCore", sender.ToArray());
                }
            }
        }

        /// <summary>
        /// Gets or sets the hint Markers.
        /// Note: This property is a helper for <see cref="SetModelMarkersAsync(string, IMarkerData[])"/>; use this property or the method, not both.
        /// </summary>
        public IObservableVector<IMarkerData> Markers
        {
            get => (IObservableVector<IMarkerData>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }

        public static DependencyProperty IsDiffViewModeProperty { get; }
            = DependencyProperty.Register(
                nameof(IsDiffViewMode),
                typeof(bool),
                typeof(CodeEditorCore),
                new PropertyMetadata(false));

        public bool IsDiffViewMode
        {
            get => (bool)GetValue(IsDiffViewModeProperty);
            set => SetValue(IsDiffViewModeProperty, value);
        }

        public static DependencyProperty DiffLeftTextProperty { get; }
            = DependencyProperty.Register(
                nameof(DiffLeftText),
                typeof(string),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    string.Empty,
                    (d, e) =>
                    {
                        var codeEditor = (CodeEditorCore)d;
                        if (!codeEditor.IsSettingValue && codeEditor.IsDiffViewMode)
                        {
                            _ = codeEditor.InvokeScriptAsync("updateDiffContent", new object[] { e.NewValue.ToString(), codeEditor.DiffRightText });
                        }
                    }));

        public string DiffLeftText
        {
            get => (string)GetValue(DiffLeftTextProperty);
            set => SetValue(DiffLeftTextProperty, value);
        }

        public static DependencyProperty DiffRightTextProperty { get; }
            = DependencyProperty.Register(
                nameof(DiffRightText),
                typeof(string),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    string.Empty,
                    (d, e) =>
                    {
                        var codeEditor = (CodeEditorCore)d;
                        if (!codeEditor.IsSettingValue && codeEditor.IsDiffViewMode)
                        {
                            _ = codeEditor.InvokeScriptAsync("updateDiffContent", new object[] { codeEditor.DiffLeftText, e.NewValue.ToString() });
                        }
                    }));

        public string DiffRightText
        {
            get => (string)GetValue(DiffRightTextProperty);
            set => SetValue(DiffRightTextProperty, value);
        }

        public static DependencyProperty DiffOptionsProperty { get; }
            = DependencyProperty.Register(
                nameof(DiffOptions),
                typeof(DiffEditorConstructionOptions),
                typeof(CodeEditorCore),
                new PropertyMetadata(
                    null,
                    (d, e) =>
                    {
                        if (d is CodeEditorCore editor)
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
        /// Get or set the CodeEditorCore Options. Node: Will overwrite CodeLanguage.
        /// </summary>
        public DiffEditorConstructionOptions DiffOptions
        {
            get => (DiffEditorConstructionOptions)GetValue(DiffOptionsProperty);
            set => SetValue(DiffOptionsProperty, value);
        }

        /// <summary>
        /// When Editor is Loading, it is ready to receive commands to the Monaco Engine.
        /// </summary>
        public event RoutedEventHandler? EditorLoading;

        /// <summary>
        /// When Editor is Loaded, it has been rendered and is ready to be displayed.
        /// </summary>
        public event RoutedEventHandler? EditorLoaded;

        /// <summary>
        /// Called when a link is Ctrl+Clicked on in the editor, set Handled to true to prevent opening.
        /// </summary>
        public event TypedEventHandler<WebView, WebViewNewWindowRequestedEventArgs>? OpenLinkRequested;

        /// <summary>
        /// Called when an internal exception is encountered while executing a command. (for testing/reporting issues)
        /// </summary>
        public event TypedEventHandler<CodeEditorCore, Exception>? InternalException;

        /// <summary>
        /// Custom Keyboard Handler.
        /// </summary>
        public event WebKeyEventHandler? EditorKeyDown;

        public static DependencyProperty IsEditorLoadedProperty { get; }
            = DependencyProperty.Register(
                nameof(IsEditorLoaded),
                typeof(string),
                typeof(CodeEditorCore),
                new PropertyMetadata(false));

        /// <summary>
        /// Template Property used during loading to prevent blank control visibility when it's still loading WebView.
        /// </summary>
        public bool IsEditorLoaded
        {
            get => (bool)GetValue(IsEditorLoadedProperty);
            private set => SetValue(IsEditorLoadedProperty, value);
        }

        /// <summary>
        /// Construct a new IStandAloneCodeEditor.
        /// </summary>
        public CodeEditorCore()
        {
            DefaultStyleKey = typeof(CodeEditorCore);

            Options = new StandaloneEditorConstructionOptions();
            DiffOptions = new DiffEditorConstructionOptions();

            // Set Pass-Thru Properties
            Options.GlyphMargin = HasGlyphMargin;
            DiffOptions.GlyphMargin = HasGlyphMargin;

            // Register for changes
            Options.PropertyChanged += Options_PropertyChanged;
            DiffOptions.PropertyChanged += DiffOptions_PropertyChanged;

            // Initialize this here so property changed event will fire and register collection changed event.
            Decorations = new ObservableVector<ModelDeltaDecoration>();
            Markers = new ObservableVector<IMarkerData>();
            _model = new ModelHelper(this);

            base.Loaded += CodeEditor_Loaded;
            Unloaded += CodeEditor_Unloaded;
        }

        private async void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not StandaloneEditorConstructionOptions options || IsDiffViewMode)
            {
                return;
            }

            if (e.PropertyName == nameof(StandaloneEditorConstructionOptions.Language))
            {
                await InvokeScriptAsync("updateLanguage", options.Language ?? string.Empty);
                if (CodeLanguage != options.Language)
                {
                    CodeLanguage = options.Language;
                }
            }

            if (e.PropertyName == nameof(StandaloneEditorConstructionOptions.GlyphMargin))
            {
                if (HasGlyphMargin != options.GlyphMargin)
                {
                    options.GlyphMargin = HasGlyphMargin;
                }
            }

            if (e.PropertyName == nameof(StandaloneEditorConstructionOptions.ReadOnly))
            {
                if (ReadOnly != options.ReadOnly)
                {
                    options.ReadOnly = ReadOnly;
                }
            }

            await InvokeScriptAsync("updateOptions", options);
        }

        private async void DiffOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not DiffEditorConstructionOptions options || !IsDiffViewMode)
            {
                return;
            }

            if (e.PropertyName == nameof(DiffEditorConstructionOptions.GlyphMargin))
            {
                if (HasGlyphMargin != options.GlyphMargin)
                {
                    options.GlyphMargin = HasGlyphMargin;
                }
            }

            if (e.PropertyName == nameof(DiffEditorConstructionOptions.ReadOnly))
            {
                if (ReadOnly != options.ReadOnly)
                {
                    options.ReadOnly = ReadOnly;
                }
            }

            await InvokeScriptAsync("updateDiffOptions", options);
        }

        private void CodeEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // Do this the 2nd time around.
            if (_model == null && _view != null)
            {
                _model = new ModelHelper(this);

                Options.PropertyChanged += Options_PropertyChanged;
                DiffOptions.PropertyChanged += DiffOptions_PropertyChanged;

                Decorations.VectorChanged += Decorations_VectorChanged;
                Markers.VectorChanged += Markers_VectorChanged;

                _view.NewWindowRequested += WebView_NewWindowRequested;

                _initialized = true;

                EditorLoading?.Invoke(this, new RoutedEventArgs());

                Unloaded += CodeEditor_Unloaded;

                EditorLoaded?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void CodeEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= CodeEditor_Unloaded;

            if (_view != null)
            {
                _view.NavigationStarting -= WebView_NavigationStarting;
                _view.DOMContentLoaded -= WebView_DOMContentLoaded;
                _view.NavigationCompleted -= WebView_NavigationCompleted;
                _view.NewWindowRequested -= WebView_NewWindowRequested;
            }

            Decorations.VectorChanged -= Decorations_VectorChanged;
            Markers.VectorChanged -= Markers_VectorChanged;

            Options.PropertyChanged -= Options_PropertyChanged;
            DiffOptions.PropertyChanged -= DiffOptions_PropertyChanged;

            UnregisterPropertyChangedCallback(RequestedThemeProperty, _themeToken);
            _keyboardListener = null;
            _model = null;
        }

        protected override void OnApplyTemplate()
        {
            if (_view != null)
            {
                _view.GotFocus -= View_GotFocus;
                _view.LostFocus -= View_LostFocus;
                _view.NavigationStarting -= WebView_NavigationStarting;
                _view.DOMContentLoaded -= WebView_DOMContentLoaded;
                _view.NavigationCompleted -= WebView_NavigationCompleted;
                _view.NewWindowRequested -= WebView_NewWindowRequested;
                _initialized = false;
            }

            _view = (WebView)GetTemplateChild("WebViewElement");

            if (_view != null)
            {
                _view.GotFocus += View_GotFocus;
                _view.LostFocus += View_LostFocus;
                _view.NavigationStarting += WebView_NavigationStarting;
                _view.DOMContentLoaded += WebView_DOMContentLoaded;
                _view.NavigationCompleted += WebView_NavigationCompleted;
                _view.NewWindowRequested += WebView_NewWindowRequested;
                Task.Delay(250).ContinueWith(async _ =>
                {
                    await _view.Dispatcher.RunIdleAsync((IdleDispatchedHandlerArgs e) =>
                    {
                        _view.Source = new System.Uri("ms-appx-web:///DevToys.MonacoEditor/CodeEditorControl/CodeEditorCore.html");
                    });
                });
            }

            base.OnApplyTemplate();
        }

        private void View_GotFocus(object sender, RoutedEventArgs args)
        {
            VisualStateManager.GoToState(this, FocusedState, false);
        }

        private void View_LostFocus(object sender, RoutedEventArgs args)
        {
            VisualStateManager.GoToState(this, NormalState, false);
        }

        internal async Task SendScriptAsync(string script,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            await SendScriptAsync<object>(script, member, file, line);
        }

        internal async Task<T?> SendScriptAsync<T>(string script,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            if (_initialized)
            {
                try
                {
                    if (_view is null)
                    {
                        throw new NullReferenceException();
                    }
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
            return await InvokeScriptAsync<T>(method, new object[] { arg }, serialize, member, file, line);
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
                try
                {

                    if (_view is null)
                    {
                        throw new NullReferenceException();
                    }
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
                Debug.WriteLine("WARNING: Tried to call " + method + " before initialized.");
#endif
            }

            return default;
        }

        public void Dispose()
        {
            if (_themeListener != null)
            {
                _themeListener.ThemeChanged -= ThemeListener_ThemeChanged;
            }

            _themeListener = null;

            ParentAccessor?.Dispose();
            ParentAccessor = null;
            CssStyleBroker.DetachEditor(this);
        }

        private void WebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
#if DEBUG
            Debug.WriteLine("DOM Content Loaded");
#endif
            _initialized = true;
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            IsEditorLoaded = true;

            // Update theme
            await InvokeScriptAsync("setTheme", args: new string[] { _themeListener!.AccentColorHtmlHex });
            await InvokeScriptAsync("changeTheme", new string[] { _themeListener.CurrentTheme.ToString(), _themeListener.IsHighContrast.ToString() });

            // If we're supposed to have focus, make sure we try and refocus on our now loaded webview.
            if (FocusManager.GetFocusedElement() == this)
            {
                if (_view is null)
                {
                    throw new NullReferenceException();
                }
                _view.Focus(FocusState.Programmatic);
            }

            EditorLoaded?.Invoke(this, new RoutedEventArgs());
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
#if DEBUG
            Debug.WriteLine("Navigation Starting");
#endif
            ParentAccessor = new ParentAccessor(this);
            ParentAccessor.AddAssemblyForTypeLookup(typeof(Range).GetTypeInfo().Assembly);
            ParentAccessor.RegisterAction("Loaded", CodeEditorLoaded);

            _themeListener = new ThemeListener();
            _themeListener.ThemeChanged += ThemeListener_ThemeChanged;
            _themeToken = RegisterPropertyChangedCallback(RequestedThemeProperty, RequestedTheme_PropertyChanged);

            _keyboardListener = new KeyboardListener(this);

            if (_view is null)
            {
                throw new NullReferenceException();
            }

            _view.AddWebAllowedObject("Debug", new DebugLogger());
            _view.AddWebAllowedObject("Parent", ParentAccessor);
            _view.AddWebAllowedObject("Theme", _themeListener);
            _view.AddWebAllowedObject("Keyboard", _keyboardListener);
        }

        private async void CodeEditorLoaded()
        {
            if (Decorations != null && Decorations.Count > 0)
            {
                // Need to retrigger highlights after load if they were set before load.
                await DeltaDecorationsHelperAsync(Decorations.ToArray());
            }

            Options.ReadOnly = ReadOnly;
            Options.Language = CodeLanguage;
            DiffOptions.OriginalEditable = !ReadOnly;
            DiffOptions.ReadOnly = ReadOnly;

            // Now we're done loading
            EditorLoading?.Invoke(this, new RoutedEventArgs());
        }

        private void WebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            // TODO: Should probably create own event args here as we don't want to expose the referrer to our internal page?
            OpenLinkRequested?.Invoke(sender, args);
        }

        private async void RequestedTheme_PropertyChanged(DependencyObject obj, DependencyProperty property)
        {
            var editor = (CodeEditorCore)obj;
            ElementTheme theme = editor.RequestedTheme;
            string? tstr = string.Empty;

            if (theme == ElementTheme.Default)
            {
                tstr = _themeListener?.CurrentThemeName ?? string.Empty;
            }
            else
            {
                tstr = theme.ToString();
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await InvokeScriptAsync("setTheme", args: new string[] { _themeListener!.AccentColorHtmlHex });
                await InvokeScriptAsync("changeTheme", new string[] { tstr, _themeListener.IsHighContrast.ToString() });
            });
        }

        private async void ThemeListener_ThemeChanged(ThemeListener sender)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await InvokeScriptAsync("setTheme", args: new string[] { sender.AccentColorHtmlHex });
                await InvokeScriptAsync("changeTheme", args: new string[] { sender.CurrentTheme.ToString(), sender.IsHighContrast.ToString() });
            });
        }

        public bool TriggerKeyDown(WebKeyEventArgs args)
        {
            EditorKeyDown?.Invoke(this, args);

            return args.Handled;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (_view != null && FocusManager.GetFocusedElement() == this)
            {
                // Forward Focus onto our inner WebView
                _view.Focus(FocusState.Programmatic);
            }
        }

        #region Reveal Methods

        public IAsyncAction RevealLineAsync(uint lineNumber)
        {
            return SendScriptAsync("editor.revealLine(" + lineNumber + ")").AsAsyncAction();
        }

        public IAsyncAction RevealLineInCenterAsync(uint lineNumber)
        {
            return SendScriptAsync("editor.revealLineInCenter(" + lineNumber + ")").AsAsyncAction();
        }

        public IAsyncAction RevealLineInCenterIfOutsideViewportAsync(uint lineNumber)
        {
            return SendScriptAsync("editor.revealLineInCenterIfOutsideViewport(" + lineNumber + ")").AsAsyncAction();
        }

        public IAsyncAction RevealLinesAsync(uint startLineNumber, uint endLineNumber)
        {
            return SendScriptAsync("editor.revealLines(" + startLineNumber + ", " + endLineNumber + ")").AsAsyncAction();
        }

        public IAsyncAction RevealLinesInCenterAsync(uint startLineNumber, uint endLineNumber)
        {
            return SendScriptAsync("editor.revealLinesInCenter(" + startLineNumber + ", " + endLineNumber + ")").AsAsyncAction();
        }

        public IAsyncAction RevealLinesInCenterIfOutsideViewportAsync(uint startLineNumber, uint endLineNumber)
        {
            return SendScriptAsync("editor.revealLinesInCenterIfOutsideViewport(" + startLineNumber + ", " + endLineNumber + ")").AsAsyncAction();
        }

        public IAsyncAction RevealPositionAsync(IPosition position)
        {
            return RevealPositionAsync(position, false, false);
        }

        public IAsyncAction RevealPositionAsync(IPosition position, bool revealVerticalInCenter)
        {
            return RevealPositionAsync(position, revealVerticalInCenter, false);
        }

        public IAsyncAction RevealPositionAsync(IPosition position, bool revealVerticalInCenter, bool revealHorizontal)
        {
            return SendScriptAsync("editor.revealPosition(JSON.parse('" + position.ToJson() + "'), " + JsonConvert.ToString(revealVerticalInCenter) + ", " + JsonConvert.ToString(revealHorizontal) + ")").AsAsyncAction();
        }

        public IAsyncAction RevealPositionInCenterAsync(IPosition position)
        {
            return SendScriptAsync("editor.revealPositionInCenter(JSON.parse('" + position.ToJson() + "'))").AsAsyncAction();
        }

        public IAsyncAction RevealPositionInCenterIfOutsideViewportAsync(IPosition position)
        {
            return SendScriptAsync("editor.revealPositionInCenterIfOutsideViewport(JSON.parse('" + position.ToJson() + "'))").AsAsyncAction();
        }

        public IAsyncAction RevealRangeAsync(IRange range)
        {
            return SendScriptAsync("editor.revealRange(JSON.parse('" + range.ToJson() + "'))").AsAsyncAction();
        }

        public IAsyncAction RevealRangeAtTopAsync(IRange range)
        {
            return SendScriptAsync("editor.revealRangeAtTop(JSON.parse('" + range.ToJson() + "'))").AsAsyncAction();
        }

        public IAsyncAction RevealRangeInCenterAsync(IRange range)
        {
            return SendScriptAsync("editor.revealRangeInCenter(JSON.parse('" + range.ToJson() + "'))").AsAsyncAction();
        }

        public IAsyncAction RevealRangeInCenterIfOutsideViewportAsync(IRange range)
        {
            return SendScriptAsync("editor.revealRangeInCenterIfOutsideViewport(JSON.parse('" + range.ToJson() + "'))").AsAsyncAction();
        }
        #endregion

        public IAsyncAction AddActionAsync(IActionDescriptor action)
        {
            var wref = new WeakReference<CodeEditorCore>(this);
            ParentAccessor?.RegisterAction(
                "Action" + action.Id,
                new Action(() =>
                {
                    if (wref.TryGetTarget(out CodeEditorCore editor))
                    {
                        action?.Run(editor, null);
                    }
                }));
            return InvokeScriptAsync("addAction", action).AsAsyncAction();
        }

        /// <summary>
        /// Invoke scripts, return value must be strings
        /// </summary>
        /// <param name="script">Script to invoke</param>
        /// <returns>An async operation result to string</returns>
        public IAsyncOperation<string> InvokeScriptAsync(string script)
        {
            if (_view is null)
            {
                throw new NullReferenceException();
            }
            return _view.InvokeScriptAsync("eval", new[] { script });
        }

        public IAsyncOperation<string?> AddCommandAsync(int keybinding, CommandHandler handler)
        {
            return AddCommandAsync(keybinding, handler, string.Empty);
        }

        public IAsyncOperation<string?> AddCommandAsync(int keybinding, CommandHandler handler, string context)
        {
            string? name = "Command" + keybinding;
            ParentAccessor?.RegisterAction(name, new Action(() => { handler?.Invoke(); }));
            return InvokeScriptAsync<string?>("addCommand", new object[] { keybinding, name, context }).AsAsyncOperation();
        }

        public IAsyncOperation<ContextKey> CreateContextKeyAsync(string key, bool defaultValue)
        {
            var ck = new ContextKey(this, key, defaultValue);

            return InvokeScriptAsync("createContext", ck).ContinueWith((noop) =>
            {
                return ck;
            }).AsAsyncOperation();
        }

        public IModel? GetModel()
        {
            return _model;
        }

        public IAsyncOperation<IEnumerable<Marker>?> GetModelMarkersAsync() // TODO: Filter (string? owner, Uri? resource, int? take)
        {
            return SendScriptAsync<IEnumerable<Marker>?>("monaco.editor.getModelMarkers();").AsAsyncOperation();
        }

        public IAsyncAction SetModelMarkersAsync(string owner, [ReadOnlyArray] IMarkerData[] markers)
        {
            return SendScriptAsync("monaco.editor.setModelMarkers(model, " + JsonConvert.ToString(owner) + ", " + JsonConvert.SerializeObject(markers) + ");").AsAsyncAction();
        }

        public IAsyncOperation<Position?> GetPositionAsync()
        {
            return SendScriptAsync<Position?>("editor.getPosition();").AsAsyncOperation();
        }

        public IAsyncAction SetPositionAsync(IPosition position)
        {
            return SendScriptAsync("editor.setPosition(" + JsonConvert.SerializeObject(position) + ");").AsAsyncAction();
        }

        /// <summary>
        /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.icommoncodeeditor.html#deltadecorations
        /// 
        /// Using <see cref="Decorations"/> Property to manipulate decorations instead of calling this directly.
        /// </summary>
        /// <param name="newDecorations"></param>
        /// <returns></returns>
        private IAsyncAction DeltaDecorationsHelperAsync([ReadOnlyArray] ModelDeltaDecoration[]? newDecorations)
        {
            ModelDeltaDecoration[]? newDecorationsAdjust = newDecorations ?? Array.Empty<ModelDeltaDecoration>();

            // Update Styles
            return InvokeScriptAsync("updateStyle", CssStyleBroker.GetInstance(this).GetStyles()).ContinueWith((noop) =>
            {
                // Send Command to Modify Decorations
                // IMPORTANT: Need to cast to object here as we want this to be a single array object passed as a parameter, not a list of parameters to expand.
                return InvokeScriptAsync("updateDecorations", (object)newDecorationsAdjust);
            }).AsAsyncAction();
        }
    }
}
