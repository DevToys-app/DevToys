#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using DevToys.Core;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace DevToys.UI.Controls
{
    public sealed partial class CustomTextBox : UserControl, ICustomTextBox
    {
        private bool _isTextPendingUpdate;
        private IEnumerable<HighlightSpan>? _highlightedSpans;

        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(CustomTextBox),
                new PropertyMetadata(null));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty IsRichTextEditProperty
            = DependencyProperty.Register(
                nameof(IsRichTextEdit),
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnIsRichTextEditPropertyChangedCalled));

        public bool IsRichTextEdit
        {
            get => (bool)GetValue(IsRichTextEditProperty);
            set => SetValue(IsRichTextEditProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnIsReadOnlyPropertyChangedCalled));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty AcceptsReturnProperty
            = DependencyProperty.Register(
                nameof(AcceptsReturn),
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnAcceptsReturnPropertyChangedCalled));

        public bool AcceptsReturn
        {
            get => (bool)GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }

        public static readonly DependencyProperty CanClearWhenReadOnlyProperty
            = DependencyProperty.Register(
                nameof(CanClearWhenReadOnly),
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnCanClearWhenReadOnlyPropertyChangedCalled));

        public bool CanClearWhenReadOnly
        {
            get => (bool)GetValue(CanClearWhenReadOnlyProperty);
            set => SetValue(CanClearWhenReadOnlyProperty, value);
        }

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(CustomTextBox),
                new PropertyMetadata(string.Empty, OnTextPropertyChangedCalled));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty SelectionStartProperty
            = DependencyProperty.Register(
                nameof(SelectionStart),
                typeof(int),
                typeof(CustomTextBox),
                new PropertyMetadata(0));

        public int SelectionStart
        {
            get => (int)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public CustomTextBox()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            ActualThemeChanged += OnActualThemeChanged;

            CutCommand = new RelayCommand(ExecuteCutCommand, CanExecuteCutCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            PasteCommand = new RelayCommand(ExecutePasteCommand, CanExecutePasteCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            UndoCommand = new RelayCommand(ExecuteUndoCommand, CanExecuteUndoCommand);
            RedoCommand = new RelayCommand(ExecuteRedoCommand, CanExecuteRedoCommand);
            SelectAllCommand = new RelayCommand(ExecuteSelectAllCommand, CanExecuteSelectAllCommand);

            DataContext = this;

            UpdateUI();
        }

        #region CutCommand

        public IRelayCommand CutCommand { get; }

        private bool CanExecuteCutCommand()
        {
            return RichEditBox != null && RichEditBox.TextDocument.Selection.Length != 0 && IsEnabled && RichEditBox.TextDocument.CanCopy();
        }

        private void ExecuteCutCommand()
        {
            RichEditBox.TextDocument.Selection.Cut();
            Clipboard.Flush();
        }

        #endregion

        #region CopyCommand

        public IRelayCommand CopyCommand { get; }

        private bool CanExecuteCopyCommand()
        {
            return RichEditBox != null && RichEditBox.TextDocument.Selection.Length != 0 && IsEnabled && RichEditBox.TextDocument.CanCopy();
        }

        private void ExecuteCopyCommand()
        {
            RichEditBox.TextDocument.Selection.Copy();
            Clipboard.Flush();
        }

        #endregion

        #region PasteCommand

        public IRelayCommand PasteCommand { get; }

        private bool CanExecutePasteCommand()
        {
            return RichEditBox != null && RichEditBox.TextDocument.CanPaste() && IsEnabled;
        }

        private void ExecutePasteCommand()
        {
            PasteButton_Click(this, null!);
        }

        #endregion

        #region DeleteCommand

        public IRelayCommand DeleteCommand { get; }

        private bool CanExecuteDeleteCommand()
        {
            return RichEditBox != null && RichEditBox.TextDocument.Selection.Length != 0 && IsEnabled && !IsReadOnly;
        }

        private void ExecuteDeleteCommand()
        {
            RichEditBox.TextDocument.BeginUndoGroup();
            RichEditBox.TextDocument.Selection.SetText(TextSetOptions.None, string.Empty);
            RichEditBox.TextDocument.EndUndoGroup();
        }

        #endregion

        #region UndoCommand

        public IRelayCommand UndoCommand { get; }

        private bool CanExecuteUndoCommand()
        {
            return RichEditBox != null && RichEditBox.TextDocument.CanUndo() && IsEnabled && !IsReadOnly;
        }

        private void ExecuteUndoCommand()
        {
            RichEditBox.TextDocument.Undo();
        }

        #endregion

        #region RedoCommand

        public IRelayCommand RedoCommand { get; }

        private bool CanExecuteRedoCommand()
        {
            return RichEditBox != null && RichEditBox.TextDocument.CanRedo() && IsEnabled && !IsReadOnly;
        }

        private void ExecuteRedoCommand()
        {
            RichEditBox.TextDocument.Redo();
        }

        #endregion

        #region SelectAllCommand

        public IRelayCommand SelectAllCommand { get; }

        private bool CanExecuteSelectAllCommand()
        {
            return IsEnabled;
        }

        private void ExecuteSelectAllCommand()
        {
            RichEditBox.TextDocument.Selection.SetRange(0, int.MaxValue);
        }

        #endregion

        public void SetHighlights(IEnumerable<HighlightSpan>? spans)
        {
            IEnumerable<HighlightSpan>? highlightsToRemove = _highlightedSpans?.Except(spans ?? Array.Empty<HighlightSpan>());
            IEnumerable<HighlightSpan>? highlightsToAdd = spans?.Except(_highlightedSpans ?? Array.Empty<HighlightSpan>());

            _highlightedSpans = spans;

            if (!IsRichTextEdit)
            {
                return;
            }

            RichEditBox? richEditBox = GetRichEditBox();
            richEditBox.TextDocument.BatchDisplayUpdates();

            if (highlightsToRemove is not null)
            {
                foreach (HighlightSpan span in highlightsToRemove)
                {
                    ITextRange range
                         = richEditBox.TextDocument.GetRange(
                             span.StartIndex,
                             span.StartIndex + span.Length);
                    range.CharacterFormat.BackgroundColor = Colors.Transparent;
                    range.CharacterFormat.ForegroundColor = ActualTheme == ElementTheme.Dark ? Colors.White : Colors.Black;
                }
            }

            if (highlightsToAdd is not null)
            {
                foreach (HighlightSpan span in highlightsToAdd)
                {
                    ITextRange range
                         = richEditBox.TextDocument.GetRange(
                             span.StartIndex,
                             span.StartIndex + span.Length);
                    range.CharacterFormat.BackgroundColor = span.BackgroundColor;
                    range.CharacterFormat.ForegroundColor = span.ForegroundColor;
                }
            }

            richEditBox.TextDocument.ApplyDisplayUpdates();
        }

        public void ScrollToBottom()
        {
            if (IsRichTextEdit)
            {
                RichEditBox.Document.GetRange(0, Text.Length).ScrollIntoView(PointOptions.None);
            }
            else
            {
                var grid = (Grid)VisualTreeHelper.GetChild(TextBox, 0);
                for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
                {
                    object obj = VisualTreeHelper.GetChild(grid, i);
                    if (obj is not ScrollViewer)
                    {
                        continue;
                    }

                    ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);

                    break;
                }
            }
        }

        private void UpdateUI()
        {
            if (Header is not null)
            {
                GetHeaderTextBlock().Visibility = Visibility.Visible;
            }

            if (IsRichTextEdit)
            {
                RichEditBox? richEditBox = GetRichEditBox();
                richEditBox.Visibility = Visibility.Visible;
                richEditBox.TextChanging += RichEditBox_TextChanging;
                richEditBox.SelectionFlyout = null;

                if (TextBox is not null)
                {
                    GetTextBox().Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (RichEditBox is not null)
                {
                    GetRichEditBox().Visibility = Visibility.Collapsed;
                }
                GetTextBox().Visibility = Visibility.Visible;
            }

            if (IsReadOnly)
            {
                if (PasteButton is not null)
                {
                    GetPasteButton().Visibility = Visibility.Collapsed;
                    GetOpenFileButton().Visibility = Visibility.Collapsed;
                    GetClearButton().Visibility = Visibility.Collapsed;
                }

                if (!AcceptsReturn)
                {
                    GetInlinedCopyButton().Visibility = Visibility.Visible;
                    if (CopyButton is not null)
                    {
                        GetCopyButton().Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (InlinedCopyButton is not null)
                    {
                        GetInlinedCopyButton().Visibility = Visibility.Collapsed;
                    }
                    GetCopyButton().Visibility = Visibility.Visible;
                    if (CanClearWhenReadOnly)
                    {
                        GetClearButton().Visibility = Visibility.Visible;
                    }
                    else if (ClearButton is not null)
                    {
                        GetClearButton().Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                GetPasteButton().Visibility = Visibility.Visible;
                if (AcceptsReturn)
                {
                    GetOpenFileButton().Visibility = Visibility.Visible;
                    GetClearButton().Visibility = Visibility.Visible;
                }

                if (InlinedCopyButton is not null)
                {
                    GetInlinedCopyButton().Visibility = Visibility.Collapsed;
                }

                if (CopyButton is not null)
                {
                    GetCopyButton().Visibility = Visibility.Collapsed;
                }
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

        private Button GetInlinedCopyButton()
        {
            return (Button)(InlinedCopyButton ?? FindName(nameof(InlinedCopyButton)));
        }

        private TextBlock GetHeaderTextBlock()
        {
            return (TextBlock)(HeaderTextBlock ?? FindName(nameof(HeaderTextBlock)));
        }

        private TextBox GetTextBox()
        {
            return (TextBox)(TextBox ?? FindName(nameof(TextBox)));
        }

        private RichEditBox GetRichEditBox()
        {
            return (RichEditBox)(RichEditBox ?? FindName(nameof(RichEditBox)));
        }

        private void CopyTextBoxSelectionToClipboard()
        {
            var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(TextBox.SelectedText);
            Clipboard.SetContentWithOptions(dataPackage, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
            Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
        }

        private void CopyRichEditBoxSelectionToClipboard()
        {
            RichEditBox.Document.Selection.GetText(TextGetOptions.UseCrlf, out string? text);
            var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(text);
            Clipboard.SetContentWithOptions(dataPackage, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
            Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            SetHighlights(_highlightedSpans);
        }

        private async void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsRichTextEdit)
            {
                RichEditBox? richEditBox = GetRichEditBox();
                if (!richEditBox.TextDocument.CanPaste())
                {
                    return;
                }

                try
                {
                    DataPackageView? dataPackageView = Clipboard.GetContent();
                    if (!dataPackageView.Contains(StandardDataFormats.Text))
                    {
                        return;
                    }

                    string? text = await dataPackageView.GetTextAsync();

                    richEditBox.TextDocument.BeginUndoGroup();
                    richEditBox.TextDocument.Selection.SetText(TextSetOptions.None, text);
                    richEditBox.TextDocument.Selection.StartPosition = richEditBox.TextDocument.Selection.EndPosition;
                    richEditBox.TextDocument.EndUndoGroup();
                }
                catch (Exception ex)
                {
                    Logger.LogFault("Failed to paste in custom text box", ex);
                }
            }
            else
            {
                TextBox.PasteFromClipboard();
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
                data.SetText(Text);

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
            }
            catch (Exception ex)
            {
                Logger.LogFault("Failed to copy from custom text box", ex);
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
                catch
                {
                    // TODO: Show a modal explaining the user that we can't read the file. Maybe it's not a text file.
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        private void RichEditBox_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                if (!_isTextPendingUpdate)
                {
                    RichEditBox.TextDocument.GetText(TextGetOptions.UseCrlf, out string? document);
                    _isTextPendingUpdate = true;
                    Text = document;
                    SetHighlights(_highlightedSpans);
                    _isTextPendingUpdate = false;
                }
            }
        }

        private void TextBox_CopyingToClipboard(TextBox sender, TextControlCopyingToClipboardEventArgs args)
        {
            CopyTextBoxSelectionToClipboard();
            args.Handled = true;
        }

        private void TextBox_CuttingToClipboard(TextBox sender, TextControlCuttingToClipboardEventArgs args)
        {
            CopyTextBoxSelectionToClipboard();
            args.Handled = true;
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectionStart = TextBox.SelectionStart;
        }

        private void RichEditBox_CopyingToClipboard(RichEditBox sender, TextControlCopyingToClipboardEventArgs args)
        {
            CopyRichEditBoxSelectionToClipboard();
            args.Handled = true;
        }

        private void RichEditBox_CuttingToClipboard(RichEditBox sender, TextControlCuttingToClipboardEventArgs args)
        {
            CopyRichEditBoxSelectionToClipboard();
            args.Handled = true;
        }

        private void RichEditBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectionStart = RichEditBox.Document.Selection.StartPosition;
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

        private void InputSizeFit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TextBox is not null)
            {
                InputSizeFit.MinHeight = TextBox.MinHeight;
                TextBox.Height = InputSizeFit.ActualHeight;
                TextBox.Width = InputSizeFit.ActualWidth;
            }

            if (RichEditBox is not null)
            {
                InputSizeFit.MinHeight = RichEditBox.MinHeight;
                RichEditBox.Height = InputSizeFit.ActualHeight;
                RichEditBox.Width = InputSizeFit.ActualWidth;
            }
        }

        private static void OnIsReadOnlyPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CustomTextBox)sender).UpdateUI();
        }

        private static void OnAcceptsReturnPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CustomTextBox)sender).UpdateUI();
        }

        private static void OnCanClearWhenReadOnlyPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CustomTextBox)sender).UpdateUI();
        }

        private static void OnIsRichTextEditPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CustomTextBox)sender).UpdateUI();
        }

        private static void OnTextPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var customTextBox = (CustomTextBox)sender;
            if (customTextBox.IsRichTextEdit)
            {
                if (!customTextBox._isTextPendingUpdate)
                {
                    string? text = eventArgs.NewValue as string;

                    customTextBox._isTextPendingUpdate = true;
                    bool isReadOnly = customTextBox.IsReadOnly;
                    customTextBox.IsReadOnly = false;
                    customTextBox.RichEditBox.Document.SetText(TextSetOptions.None, text ?? string.Empty);
                    customTextBox.SetHighlights(customTextBox._highlightedSpans);
                    customTextBox.IsReadOnly = isReadOnly;
                    customTextBox._isTextPendingUpdate = false;
                }
            }
        }
    }
}
