#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Core;
using DevToys.Core.Settings;
using DevToys.MonacoEditor.Monaco.Editor;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace DevToys.UI.Controls
{
    public sealed partial class CodeEditor : UserControl
    {
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

        public CodeEditor()
        {
            this.InitializeComponent();

            CodeEditorCore.Loading+=CodeEditorCore_Loading;

            UpdateUI();
        }

        private void CodeEditorCore_Loading(object sender, RoutedEventArgs e)
        {
            CodeEditorCore.HasGlyphMargin = false;
            CodeEditorCore.Options.GlyphMargin = false;
            CodeEditorCore.Options.MouseWheelZoom = false;
            CodeEditorCore.Options.OverviewRulerBorder = false;
            CodeEditorCore.Options.ScrollBeyondLastLine = false;
            CodeEditorCore.Options.Minimap = new EditorMinimapOptions()
            {
                Enabled = false
            };

            ApplySettings();
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

        private void ApplySettings()
        {
            ISettingsProvider? settingsProvider = SettingsProvider;
            if (settingsProvider is not null)
            {
                CodeEditorCore.Options.WordWrapMinified = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
                CodeEditorCore.Options.WordWrap = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? WordWrap.On : WordWrap.Off;
                CodeEditorCore.Options.LineNumbers = settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers) ? LineNumbersType.On : LineNumbersType.Off;
                CodeEditorCore.Options.RenderLineHighlight = settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine) ? RenderLineHighlight.All : RenderLineHighlight.None;

                //FontFamily = (FontFamily)Application.Current.Resources[settingsProvider.GetSetting(PredefinedSettings.TextEditorFont)];

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

                // Inject Ctrl+V in the editor.
                InputInjector inputInjector = InputInjector.TryCreate();
                var ctrlKey = new InjectedInputKeyboardInfo();
                ctrlKey.VirtualKey = (ushort)VirtualKey.LeftControl;
                ctrlKey.KeyOptions = InjectedInputKeyOptions.None;
                var vKey = new InjectedInputKeyboardInfo();
                vKey.VirtualKey = (ushort)VirtualKey.V;
                vKey.KeyOptions = InjectedInputKeyOptions.None;

                CodeEditorCore.Focus(FocusState.Pointer);
                await Task.Delay(100);

                inputInjector.InjectKeyboardInput(new[] { ctrlKey, vKey });

                ctrlKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
                inputInjector.InjectKeyboardInput(new[] { ctrlKey });
            }
            catch (Exception ex)
            {
                Logger.LogFault("Failed to paste in code editor", ex);
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var data = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            data.SetText(Text ?? string.Empty);

            Clipboard.SetContent(data);
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
                    string text = await FileIO.ReadTextAsync(file);
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
            ApplySettings();
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
            codeEditor.CodeEditorCore.ReadOnly = (bool)eventArgs.NewValue;
            codeEditor.UpdateUI();
        }
    }
}
