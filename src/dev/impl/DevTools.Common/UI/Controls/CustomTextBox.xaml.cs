#nullable enable

using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevTools.Common.UI.Controls
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

        public object Header
        {
            get => (object)GetValue(HeaderProperty);
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

            var richEditBox = GetRichEditBox();
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

        private void UpdateUI()
        {
            if (Header is not null)
            {
                GetHeaderContentPresenter().Visibility = Visibility.Visible;
            }

            if (IsRichTextEdit)
            {
                var richEditBox = GetRichEditBox();
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
                }

                if (PasteButton is not null)
                {
                    GetPasteButton().Visibility = Visibility.Collapsed;
                    GetOpenFileButton().Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                GetPasteButton().Visibility = Visibility.Visible;
                if (AcceptsReturn)
                {
                    GetOpenFileButton().Visibility = Visibility.Visible;
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

        private Button GetInlinedCopyButton()
        {
            return (Button)(InlinedCopyButton ?? FindName(nameof(InlinedCopyButton)));
        }

        private ContentPresenter GetHeaderContentPresenter()
        {
            return (ContentPresenter)(HeaderContentPresenter ?? FindName(nameof(HeaderContentPresenter)));
        }

        private TextBox GetTextBox()
        {
            return (TextBox)(TextBox ?? FindName(nameof(TextBox)));
        }

        private RichEditBox GetRichEditBox()
        {
            return (RichEditBox)(RichEditBox ?? FindName(nameof(RichEditBox)));
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
                var richEditBox = GetRichEditBox();
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
                    // TODO: Log this.
                }
            }
            else
            {
                TextBox.PasteFromClipboard();
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var data = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            data.SetText(Text);

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
                catch
                {
                    // TODO: Show a modal explaining the user that we can't read the file. Maybe it's not a text file.
                }
            }
        }

        private void RichEditBox_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                if (!_isTextPendingUpdate)
                {
                    RichEditBox.TextDocument.GetText(TextGetOptions.UseCrlf, out var document);
                    _isTextPendingUpdate = true;
                    Text = document;
                    SetHighlights(_highlightedSpans);
                    _isTextPendingUpdate = false;
                }
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