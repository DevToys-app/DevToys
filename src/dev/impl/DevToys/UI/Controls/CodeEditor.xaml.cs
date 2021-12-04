#nullable enable

using System;
using DevToys.Api.Core.Settings;
using DevToys.Core;
using DevToys.Core.Settings;
using DevToys.MonacoEditor.CodeEditorControl;
using DevToys.MonacoEditor.Monaco.Editor;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace DevToys.UI.Controls
{
    public sealed partial class CodeEditor : UserControl, IDisposable
    {
        private readonly object _lockObject = new();
        private int _codeEditorCodeReloadTentative;
        private CodeEditorCore _codeEditorCore;

        public static readonly DependencyProperty SettingsProviderProperty
            = DependencyProperty.Register(
                nameof(SettingsProvider),
                typeof(ISettingsProvider),
                typeof(CodeEditor),
                new PropertyMetadata(
                    null,
                    (d, e) =>
                    {
                        if (e.NewValue is ISettingsProvider settingsProvider)
                        {
                            var codeEditor = (CodeEditor)d;
                            settingsProvider.SettingChanged += codeEditor.SettingsProvider_SettingChanged;
                        }
                    }));

        public ISettingsProvider? SettingsProvider
        {
            get => (ISettingsProvider?)GetValue(SettingsProviderProperty);
            set => SetValue(SettingsProviderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(CodeEditor),
                new PropertyMetadata(null, (d, e) => { ((CodeEditor)d).UpdateUI(); }));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty ErrorMessageProperty
            = DependencyProperty.Register(
                nameof(ErrorMessage),
                typeof(string),
                typeof(CodeEditor),
                new PropertyMetadata(string.Empty));

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(CodeEditor),
                new PropertyMetadata(false, OnIsReadOnlyPropertyChangedCalled));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static DependencyProperty CodeLanguageProperty { get; }
            = DependencyProperty.Register(
                nameof(CodeLanguage),
                typeof(string),
                typeof(CodeEditor),
                new PropertyMetadata(string.Empty));

        public string? CodeLanguage
        {
            get => (string?)GetValue(CodeLanguageProperty);
            set => SetValue(CodeLanguageProperty, value);
        }

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(CodeEditor),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsDiffViewModeProperty
            = DependencyProperty.Register(
                nameof(IsDiffViewMode),
                typeof(bool),
                typeof(CodeEditor),
                new PropertyMetadata(false));

        public bool IsDiffViewMode
        {
            get => (bool)GetValue(IsDiffViewModeProperty);
            set => SetValue(IsDiffViewModeProperty, value);
        }

        public static readonly DependencyProperty DiffLeftTextProperty
            = DependencyProperty.Register(
                nameof(DiffLeftText),
                typeof(string),
                typeof(CodeEditor),
                new PropertyMetadata(string.Empty));

        public string DiffLeftText
        {
            get => (string)GetValue(DiffLeftTextProperty);
            set => SetValue(DiffLeftTextProperty, value);
        }

        public static readonly DependencyProperty DiffRightTextProperty
            = DependencyProperty.Register(
                nameof(DiffRightText),
                typeof(string),
                typeof(CodeEditor),
                new PropertyMetadata(string.Empty));

        public string DiffRightText
        {
            get => (string)GetValue(DiffRightTextProperty);
            set => SetValue(DiffRightTextProperty, value);
        }

        public static readonly DependencyProperty InlineDiffViewModeProperty
            = DependencyProperty.Register(
                nameof(InlineDiffViewMode),
                typeof(bool),
                typeof(CodeEditor),
                new PropertyMetadata(
                    false,
                    (d, e) =>
                    {
                        lock (((CodeEditor)d)._lockObject)
                        {
                            ((CodeEditor)d)._codeEditorCore.DiffOptions.RenderSideBySide = !(bool)e.NewValue;
                        }
                    }));

        public bool InlineDiffViewMode
        {
            get => (bool)GetValue(InlineDiffViewModeProperty);
            set => SetValue(InlineDiffViewModeProperty, value);
        }

        public CodeEditor()
        {
            InitializeComponent();

            _codeEditorCore = ReloadCodeEditorCore();

            UpdateUI();
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                _codeEditorCore.Dispose();
            }
        }

        private void CodeEditorCore_InternalException(CodeEditorCore sender, Exception args)
        {
            if (_codeEditorCodeReloadTentative >= 5)
            {
                ErrorMessage = $"{args.Message}\r\n{args.InnerException?.Message}";
                Logger.LogFault(nameof(CodeEditor), args, args.InnerException?.Message ?? args.Message);
            }
            else
            {
                ReloadCodeEditorCore();
            }

            _codeEditorCodeReloadTentative++;
        }

        private void CodeEditorCore_Loading(object sender, RoutedEventArgs e)
        {
            lock (_lockObject)
            {
                _codeEditorCore.EditorLoading -= CodeEditorCore_Loading;

                _codeEditorCore.HasGlyphMargin = false;
                _codeEditorCore.Options.GlyphMargin = false;
                _codeEditorCore.Options.MouseWheelZoom = false;
                _codeEditorCore.Options.OverviewRulerBorder = false;
                _codeEditorCore.Options.ScrollBeyondLastLine = false;
                _codeEditorCore.Options.FontLigatures = true;
                _codeEditorCore.Options.SnippetSuggestions = SnippetSuggestions.None;
                _codeEditorCore.Options.CodeLens = false;
                _codeEditorCore.Options.QuickSuggestions = false;
                _codeEditorCore.Options.WordBasedSuggestions = false;
                _codeEditorCore.Options.Minimap = new EditorMinimapOptions()
                {
                    Enabled = false
                };
                _codeEditorCore.Options.Hover = new EditorHoverOptions()
                {
                    Enabled = false
                };

                _codeEditorCore.DiffOptions.GlyphMargin = false;
                _codeEditorCore.DiffOptions.MouseWheelZoom = false;
                _codeEditorCore.DiffOptions.OverviewRulerBorder = false;
                _codeEditorCore.DiffOptions.ScrollBeyondLastLine = false;
                _codeEditorCore.DiffOptions.FontLigatures = true;
                _codeEditorCore.DiffOptions.SnippetSuggestions = SnippetSuggestions.None;
                _codeEditorCore.DiffOptions.CodeLens = false;
                _codeEditorCore.DiffOptions.QuickSuggestions = false;
                _codeEditorCore.DiffOptions.Minimap = new EditorMinimapOptions()
                {
                    Enabled = false
                };
                _codeEditorCore.DiffOptions.Hover = new EditorHoverOptions()
                {
                    Enabled = false
                };

                ApplySettings();
            }
        }

        private Button GetCopyButton()
        {
            return (Button)(CopyButton ?? FindName(nameof(CopyButton)));
        }

        private Button GetPasteButton()
        {
            return (Button)(PasteButton ?? FindName(nameof(PasteButton)));
        }

        private Button GetOpenFileButton()
        {
            return (Button)(OpenFileButton ?? FindName(nameof(OpenFileButton)));
        }

        private Button GetClearButton()
        {
            return (Button)(ClearButton ?? FindName(nameof(ClearButton)));
        }

        private TextBlock GetHeaderTextBlock()
        {
            return (TextBlock)(HeaderTextBlock ?? FindName(nameof(HeaderTextBlock)));
        }

        private CodeEditorCore ReloadCodeEditorCore()
        {
            lock (_lockObject)
            {
                if (_codeEditorCore is not null)
                {
                    _codeEditorCore.EditorLoading -= CodeEditorCore_Loading;
                    _codeEditorCore.InternalException -= CodeEditorCore_InternalException;
                    _codeEditorCore.SetBinding(CodeEditorCore.CodeLanguageProperty, new Binding());
                    _codeEditorCore.SetBinding(CodeEditorCore.TextProperty, new Binding());
                    _codeEditorCore.SetBinding(CodeEditorCore.IsDiffViewModeProperty, new Binding());
                    _codeEditorCore.SetBinding(CodeEditorCore.DiffLeftTextProperty, new Binding());
                    _codeEditorCore.SetBinding(CodeEditorCore.DiffRightTextProperty, new Binding());
                    _codeEditorCore.SetBinding(AutomationProperties.LabeledByProperty, new Binding());
                    CodeEditorCoreContainer.Children.Clear();
                    _codeEditorCore.Dispose();
                }

                _codeEditorCore = new CodeEditorCore();
                _codeEditorCore.EditorLoading += CodeEditorCore_Loading;
                _codeEditorCore.InternalException += CodeEditorCore_InternalException;

                _codeEditorCore.SetBinding(
                    CodeEditorCore.CodeLanguageProperty,
                    new Binding()
                    {
                        Path = new PropertyPath(nameof(CodeLanguage)),
                        Source = this,
                        Mode = BindingMode.OneWay
                    });

                _codeEditorCore.SetBinding(
                    CodeEditorCore.TextProperty,
                    new Binding()
                    {
                        Path = new PropertyPath(nameof(Text)),
                        Source = this,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                _codeEditorCore.SetBinding(
                    CodeEditorCore.IsDiffViewModeProperty,
                    new Binding()
                    {
                        Path = new PropertyPath(nameof(IsDiffViewMode)),
                        Source = this,
                        Mode = BindingMode.OneWay
                    });

                _codeEditorCore.SetBinding(
                    CodeEditorCore.DiffLeftTextProperty,
                    new Binding()
                    {
                        Path = new PropertyPath(nameof(DiffLeftText)),
                        Source = this,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                _codeEditorCore.SetBinding(
                    CodeEditorCore.DiffRightTextProperty,
                    new Binding()
                    {
                        Path = new PropertyPath(nameof(DiffRightText)),
                        Source = this,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                _codeEditorCore.SetBinding(
                    AutomationProperties.LabeledByProperty,
                    new Binding()
                    {
                        ElementName = nameof(HeaderTextBlock),
                        Source = this,
                        Mode = BindingMode.OneTime
                    });

                CodeEditorCoreContainer.Children.Add(_codeEditorCore);
                return _codeEditorCore;
            }
        }

        private void ApplySettings()
        {
            ISettingsProvider? settingsProvider = SettingsProvider;
            if (settingsProvider is not null)
            {
                lock (_lockObject)
                {
                    _codeEditorCore.Options.WordWrapMinified = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
                    _codeEditorCore.Options.WordWrap = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? WordWrap.On : WordWrap.Off;
                    _codeEditorCore.Options.LineNumbers = settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? LineNumbersType.On : LineNumbersType.Off;
                    _codeEditorCore.Options.RenderLineHighlight = settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? RenderLineHighlight.All : RenderLineHighlight.None;
                    _codeEditorCore.Options.RenderWhitespace = settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? RenderWhitespace.All : RenderWhitespace.None;
                    _codeEditorCore.Options.FontFamily = settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
                    _codeEditorCore.DiffOptions.WordWrapMinified = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
                    _codeEditorCore.DiffOptions.WordWrap = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? WordWrap.On : WordWrap.Off;
                    _codeEditorCore.DiffOptions.LineNumbers = settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? LineNumbersType.On : LineNumbersType.Off;
                    _codeEditorCore.DiffOptions.RenderLineHighlight = settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? RenderLineHighlight.All : RenderLineHighlight.None;
                    _codeEditorCore.DiffOptions.RenderWhitespace = settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace) ? RenderWhitespace.All : RenderWhitespace.None;
                    _codeEditorCore.DiffOptions.FontFamily = settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
                }
            }
        }

        private void UpdateUI()
        {
            if (Header is not null)
            {
                GetHeaderTextBlock().Visibility = Visibility.Visible;
            }

            if (IsReadOnly)
            {
                GetCopyButton().Visibility = Visibility.Visible;
                if (PasteButton is not null)
                {
                    PasteButton.Visibility = Visibility.Collapsed;
                    OpenFileButton.Visibility = Visibility.Collapsed;
                    ClearButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (CopyButton is not null)
                {
                    CopyButton.Visibility = Visibility.Collapsed;
                }

                GetPasteButton().Visibility = Visibility.Visible;
                GetOpenFileButton().Visibility = Visibility.Visible;
                GetClearButton().Visibility = Visibility.Visible;
            }

            if (IsDiffViewMode)
            {
                if (CopyButton is not null)
                {
                    CopyButton.Visibility = Visibility.Collapsed;
                }

                if (PasteButton is not null)
                {
                    PasteButton.Visibility = Visibility.Collapsed;
                    OpenFileButton.Visibility = Visibility.Collapsed;
                    ClearButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataPackageView? dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text))
                {
                    return;
                }

                string? text = await dataPackageView.GetTextAsync();

                lock (_lockObject)
                {
                    _codeEditorCore.SelectedText = text;
                    _codeEditorCore.Focus(FocusState.Programmatic);
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault("Failed to paste in code editor", ex);
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetText(Text ?? string.Empty);

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                Logger.LogFault("Failed to copy from code editor", ex);
            }
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            filePicker.FileTypeFilter.Add("*");

            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file is not null)
            {
                try
                {
                    string? text = await FileIO.ReadTextAsync(file);
                    await Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            Text = text;
                        });
                }
                catch (Exception ex)
                {
                    Logger.LogFault("Failed to load a file into a code editor", ex);
                    // TODO: Show a modal explaining the user that we can't read the file. Maybe it's not a text file.
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        private void SettingsProvider_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (e.SettingName.Contains("TextEditor"))
            {
                ApplySettings();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < CommandsToolBar.ActualWidth + 100)
            {
                CommandsToolBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommandsToolBar.Visibility = Visibility.Visible;
            }
        }

        private static void OnIsReadOnlyPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var codeEditor = (CodeEditor)sender;
            lock (codeEditor._lockObject)
            {
                codeEditor._codeEditorCore.ReadOnly = (bool)eventArgs.NewValue;
            }
            codeEditor.UpdateUI();
        }
    }
}
