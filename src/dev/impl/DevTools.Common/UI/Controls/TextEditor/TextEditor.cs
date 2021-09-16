#nullable enable

using DevTools.Common.UI.Extensions;
using DevTools.Core;
using DevTools.Core.Settings;
using DevTools.Core.Threading;
using DiffPlex.DiffBuilder;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace DevTools.Common.UI.Controls.TextEditor
{
    [TemplatePart(Name = ContentElementName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = RootGridName, Type = typeof(Grid))]
    [TemplatePart(Name = LineNumberCanvasName, Type = typeof(Canvas))]
    [TemplatePart(Name = LineNumberGridName, Type = typeof(Grid))]
    [TemplatePart(Name = LineHighlighterAndIndicatorCanvasName, Type = typeof(Canvas))]
    [TemplatePart(Name = LineHighlighterName, Type = typeof(Grid))]
    [TemplatePart(Name = LineIndicatorName, Type = typeof(Border))]
    public sealed class TextEditor : RichEditBox
    {
        private const string ContentElementName = "ContentElement";
        private const string RootGridName = "RootGrid";
        private const string LineNumberCanvasName = "LineNumberCanvas";
        private const string LineNumberGridName = "LineNumberGrid";
        private const string ContentScrollViewerVerticalScrollBarName = "VerticalScrollBar";
        private const string LineHighlighterAndIndicatorCanvasName = "LineHighlighterAndIndicatorCanvas";
        private const string LineHighlighterName = "LineHighlighter";
        private const string LineIndicatorName = "LineIndicator";

        private const string RichEditBoxDefaultLineEnding = "\r\n";

        private readonly InlineDiffBuilder _inlineDiffBuilder = new InlineDiffBuilder();
        private readonly IList<TextBlock> _renderedLineNumberBlocks = new List<TextBlock>();
        private readonly Dictionary<string, double> _miniRequisiteIntegerTextRenderingWidthCache = new();
        private readonly SolidColorBrush _lineNumberDarkModeForegroundBrush = new("#99EEEEEE".ToColor());
        private readonly SolidColorBrush _lineNumberLightModeForegroundBrush = new("#99000000".ToColor());
        private readonly ICommandHandler<KeyRoutedEventArgs> _keyboardCommandHandler;

        private CancellationTokenSource _formattingCancellationTokenSource = new CancellationTokenSource();
        private Border? _lineIndicator;
        private Canvas? _lineHighlighterAndIndicatorCanvas;
        private Canvas? _lineNumberCanvas;
        private Grid? _lineHighlighter;
        private Grid? _lineNumberGrid;
        private Grid? _rootGrid;
        private ScrollBar? _contentScrollViewerVerticalScrollBar;
        private ScrollViewer? _contentScrollViewer;
        private bool _showLineNumbers;
        private bool _highlightCurrentLine;
        private bool _loaded;
        private bool _isTextPendingUpdate;
        private bool _isDocumentLinesCachePendingUpdate = true;
        private bool _isSyntaxColorizationPendingUpdate = true;
        private bool _isSyntaxColorizationUpdateInProgress;
        private string[] _documentLinesCache = Array.Empty<string>(); // internal copy of the active document text in array format
        private string _document = string.Empty; // internal copy of the active document text
        private int _textSelectionStartPosition = 0;
        private int _textSelectionEndPosition = 0;

        public static readonly DependencyProperty SettingsProviderProperty
            = DependencyProperty.Register(
                nameof(SettingsProvider),
                typeof(ISettingsProvider),
                typeof(TextEditor),
                new PropertyMetadata(null, OnSettingsProviderPropertyChangedCallback));

        public ISettingsProvider? SettingsProvider
        {
            get => (ISettingsProvider?)GetValue(SettingsProviderProperty);
            set => SetValue(SettingsProviderProperty, value);
        }

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TextEditor),
                new PropertyMetadata(null, OnTextPropertyChangedCallback));

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public TextEditor()
        {
            DefaultStyleKey = typeof(TextEditor);

            DataContext = this;
            IsSpellCheckEnabled = false;
            SelectionFlyout = null;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            HandwritingView.BorderThickness = new Thickness(0);

            Loaded += OnLoaded;
            SelectionChanged += OnSelectionChanged;
            SizeChanged += OnSizeChanged;
            TextChanged += OnTextChanged;
            TextChanging += OnTextChanging;
            SelectionChanging += OnSelectionChanging;
            Paste += OnPaste;

            CutCommand = new RelayCommand(ExecuteCutCommand, CanExecuteCutCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            PasteCommand = new AsyncRelayCommand(ExecutePasteCommandAsync, CanExecutePasteCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            UndoCommand = new RelayCommand(ExecuteUndoCommand, CanExecuteUndoCommand);
            RedoCommand = new RelayCommand(ExecuteRedoCommand, CanExecuteRedoCommand);
            SelectAllCommand = new RelayCommand(ExecuteSelectAllCommand, CanExecuteSelectAllCommand);

            var swallowedKeys = new List<VirtualKey>()
            {
                VirtualKey.B, VirtualKey.I, VirtualKey.U, VirtualKey.Tab,
                VirtualKey.Number1, VirtualKey.Number2, VirtualKey.Number3,
                VirtualKey.Number4, VirtualKey.Number5, VirtualKey.Number6,
                VirtualKey.Number7, VirtualKey.Number8, VirtualKey.Number9,
                VirtualKey.F3,
            };

            _keyboardCommandHandler
                = new KeyboardCommandHandler(new List<IKeyboardCommand<KeyRoutedEventArgs>>
                {
                    // By default, RichEditBox insert '\v' when user hit "Shift + Enter"
                    // This should be converted to '\r' to match same behaviour as single "Enter"
                    new KeyboardCommand<KeyRoutedEventArgs>(false, false, true, VirtualKey.Enter, (args) => EnterWithAutoIndentation()),
                    new KeyboardCommand<KeyRoutedEventArgs>(VirtualKey.Enter, (args) => EnterWithAutoIndentation()),
                    // Disable RichEditBox default shortcuts (Bold, Underline, Italic)
                    // https://docs.microsoft.com/en-us/windows/desktop/controls/about-rich-edit-controls
                    new KeyboardCommand<KeyRoutedEventArgs>(true, false, false, swallowedKeys, null, shouldHandle: false, shouldSwallow: true),
                    new KeyboardCommand<KeyRoutedEventArgs>(true, false, true, swallowedKeys, null, shouldHandle: false, shouldSwallow: true),
                    new KeyboardCommand<KeyRoutedEventArgs>(true, false, true, (VirtualKey)187, null, shouldHandle: false, shouldSwallow: true), // (VirtualKey)187: =
                    new KeyboardCommand<KeyRoutedEventArgs>(true, false, true, VirtualKey.L, null, shouldHandle: false, shouldSwallow: true),
                    new KeyboardCommand<KeyRoutedEventArgs>(false, false, true, VirtualKey.F3, null, shouldHandle: false, shouldSwallow: true),
                });
        }

        #region CutCommand

        public static readonly DependencyProperty CutCommandProperty
            = DependencyProperty.Register(
                nameof(CutCommand),
                typeof(IRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IRelayCommand CutCommand
        {
            get => (IRelayCommand)GetValue(CutCommandProperty);
            private set => SetValue(CutCommandProperty, value);
        }

        private bool CanExecuteCutCommand()
        {
            return Document.Selection.Length != 0 && IsEnabled && Document.CanCopy();
        }

        private void ExecuteCutCommand()
        {
            Document.Selection.Cut();
        }

        #endregion

        #region CopyCommand

        public static readonly DependencyProperty CopyCommandProperty
            = DependencyProperty.Register(
                nameof(CopyCommand),
                typeof(IRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IRelayCommand CopyCommand
        {
            get => (IRelayCommand)GetValue(CopyCommandProperty);
            private set => SetValue(CopyCommandProperty, value);
        }

        private bool CanExecuteCopyCommand()
        {
            return Document.Selection.Length != 0 && IsEnabled && Document.CanCopy();
        }

        private void ExecuteCopyCommand()
        {
            Document.Selection.Copy();
        }

        #endregion

        #region PasteCommand

        public static readonly DependencyProperty PasteCommandProperty
            = DependencyProperty.Register(
                nameof(PasteCommand),
                typeof(IAsyncRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IAsyncRelayCommand PasteCommand
        {
            get => (IAsyncRelayCommand)GetValue(PasteCommandProperty);
            private set => SetValue(PasteCommandProperty, value);
        }

        private bool CanExecutePasteCommand()
        {
            return Document.CanPaste() && IsEnabled;
        }

        private async Task ExecutePasteCommandAsync()
        {
            await PastePlainTextFromWindowsClipboardAsync(null);
        }

        #endregion

        #region DeleteCommand

        public static readonly DependencyProperty DeleteCommandProperty
            = DependencyProperty.Register(
                nameof(DeleteCommand),
                typeof(IRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IRelayCommand DeleteCommand
        {
            get => (IRelayCommand)GetValue(DeleteCommandProperty);
            private set => SetValue(DeleteCommandProperty, value);
        }

        private bool CanExecuteDeleteCommand()
        {
            return Document.Selection.Length != 0 && IsEnabled && !IsReadOnly;
        }

        private void ExecuteDeleteCommand()
        {
            Document.BeginUndoGroup();
            Document.Selection.SetText(TextSetOptions.None, string.Empty);
            Document.EndUndoGroup();
        }

        #endregion

        #region UndoCommand

        public static readonly DependencyProperty UndoCommandProperty
            = DependencyProperty.Register(
                nameof(UndoCommand),
                typeof(IRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IRelayCommand UndoCommand
        {
            get => (IRelayCommand)GetValue(UndoCommandProperty);
            private set => SetValue(UndoCommandProperty, value);
        }

        private bool CanExecuteUndoCommand()
        {
            return Document.CanUndo() && IsEnabled && !IsReadOnly;
        }

        private void ExecuteUndoCommand()
        {
            Document.Undo();
        }

        #endregion

        #region RedoCommand

        public static readonly DependencyProperty RedoCommandProperty
            = DependencyProperty.Register(
                nameof(RedoCommand),
                typeof(IRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IRelayCommand RedoCommand
        {
            get => (IRelayCommand)GetValue(RedoCommandProperty);
            private set => SetValue(RedoCommandProperty, value);
        }

        private bool CanExecuteRedoCommand()
        {
            return Document.CanRedo() && IsEnabled && !IsReadOnly;
        }

        private void ExecuteRedoCommand()
        {
            Document.Redo();
        }

        #endregion

        #region SelectAllCommand

        public static readonly DependencyProperty SelectAllCommandProperty
            = DependencyProperty.Register(
                nameof(SelectAllCommand),
                typeof(IRelayCommand),
                typeof(TextEditor),
                new PropertyMetadata(null));

        public IRelayCommand SelectAllCommand
        {
            get => (IRelayCommand)GetValue(SelectAllCommandProperty);
            private set => SetValue(SelectAllCommandProperty, value);
        }

        private bool CanExecuteSelectAllCommand()
        {
            return IsEnabled;
        }

        private void ExecuteSelectAllCommand()
        {
            Document.Selection.SetRange(0, int.MaxValue);
        }

        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rootGrid = (Grid)GetTemplateChild(RootGridName);

            _lineNumberGrid = (Grid)GetTemplateChild(LineNumberGridName);
            _lineNumberCanvas = (Canvas)GetTemplateChild(LineNumberCanvasName);

            _lineHighlighterAndIndicatorCanvas = (Canvas)GetTemplateChild(LineHighlighterAndIndicatorCanvasName);
            _lineHighlighter = (Grid)GetTemplateChild(LineHighlighterName);
            _lineIndicator = (Border)GetTemplateChild(LineIndicatorName);

            _contentScrollViewer = (ScrollViewer)GetTemplateChild(ContentElementName);
            _contentScrollViewer.ViewChanged += OnContentScrollViewerViewChanged;
            _contentScrollViewer.SizeChanged += OnContentScrollViewerSizeChanged;

            _contentScrollViewer.ApplyTemplate();
            var scrollViewerRoot = (FrameworkElement)VisualTreeHelper.GetChild(_contentScrollViewer, 0);
            _contentScrollViewerVerticalScrollBar = (ScrollBar)scrollViewerRoot.FindName(ContentScrollViewerVerticalScrollBarName);
            _contentScrollViewerVerticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;

            _lineNumberGrid.SizeChanged += OnLineNumberGridSizeChanged;
            _rootGrid.SizeChanged += OnRootGridSizeChanged;
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            var result = _keyboardCommandHandler.Handle(e);

            if (result.ShouldHandle)
            {
                e.Handled = true;
            }

            if (!result.ShouldSwallow)
            {
                base.OnKeyDown(e);
            }
        }

        private void ApplySettings()
        {
            ISettingsProvider? settingsProvider = SettingsProvider;
            if (settingsProvider is not null)
            {
                FontFamily = (FontFamily)Application.Current.Resources[settingsProvider.GetSetting(PredefinedSettings.TextEditorFont)];
                TextWrapping = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? TextWrapping.Wrap : TextWrapping.NoWrap;
                _showLineNumbers = settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers);
                _highlightCurrentLine = settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine);

                SetDefaultTabStopAndLineSpacing(FontFamily, FontSize);

                UpdateLayout();

                if (_showLineNumbers)
                {
                    ShowLineNumbers();
                }
                else
                {
                    HideLineNumbers();
                }

                UpdateSyntaxColorization(cancelOnGoingFormattingTask: false);
            }
        }

        private void SetDefaultTabStopAndLineSpacing(FontFamily font, double fontSize)
        {
            Document.DefaultTabStop = (float)GetTextSize(font, fontSize, "text").Width;
            ITextParagraphFormat format = Document.GetDefaultParagraphFormat();
            format.SetLineSpacing(LineSpacingRule.Exactly, (float)fontSize);
            Document.SetDefaultParagraphFormat(format);
        }

        private void ResetRootGridClipping()
        {
            if (!_loaded || _rootGrid is null)
            {
                return;
            }

            _rootGrid!.Clip = new RectangleGeometry
            {
                Rect = new Rect(
                    0,
                    0,
                    _rootGrid.ActualWidth,
                    Math.Clamp(_rootGrid.ActualHeight, .0f, double.PositiveInfinity))
            };
        }

        private void ShowLineNumbers()
        {
            if (!_loaded)
            {
                return;
            }

            ResetLineNumberCanvasClipping();
            UpdateLineNumbersRendering();

            // Call UpdateLineHighlighterAndIndicator to adjust it's state
            UpdateLineHighlighterAndIndicator();
        }

        private void HideLineNumbers()
        {
            if (!_loaded)
            {
                return;
            }

            Assumes.NotNull(_lineNumberGrid, nameof(_lineNumberGrid));

            foreach (var lineNumberBlock in _renderedLineNumberBlocks)
            {
                lineNumberBlock.Visibility = Visibility.Collapsed;
            }

            _lineNumberGrid!.BorderThickness = new Thickness(0, 0, 0, 0);
            _lineNumberGrid.Margin = new Thickness(0, 0, 0, 0);
            _lineNumberGrid.Width = .0f;

            // Call UpdateLineHighlighterAndIndicator to adjust it's state
            // Since when line highlighter is disabled, we still show the line indicator when line numbers are showing
            UpdateLineHighlighterAndIndicator();
        }

        private void UpdateLineNumbersRendering()
        {
            if (!_loaded || !_showLineNumbers)
            {
                return;
            }

            Assumes.NotNull(_contentScrollViewer, nameof(_contentScrollViewer));

            ITextRange? startRange
                = Document.GetRangeFromPoint(
                    new Point(
                        _contentScrollViewer!.HorizontalOffset,
                        _contentScrollViewer.VerticalOffset),
                    PointOptions.ClientCoordinates);

            ITextRange? endRange
                = Document.GetRangeFromPoint(
                    new Point(
                        _contentScrollViewer.HorizontalOffset + _contentScrollViewer.ViewportWidth,
                        _contentScrollViewer.VerticalOffset + _contentScrollViewer.ViewportHeight),
                    PointOptions.ClientCoordinates);

            string[]? document = GetDocumentLinesCache();

            Dictionary<int, Rect> lineNumberTextRenderingPositions = CalculateLineNumberTextRenderingPositions(document, startRange, endRange);

            double minLineNumberTextRenderingWidth
                = CalculateMinimumRequisiteIntegerTextRenderingWidth(
                    FontFamily,
                    FontSize,
                    (document.Length - 1).ToString().Length);

            RenderLineNumbersInternal(lineNumberTextRenderingPositions, minLineNumberTextRenderingWidth);
        }

        private void UpdateSyntaxColorization(bool cancelOnGoingFormattingTask)
        {
            if (!_loaded || (_isSyntaxColorizationUpdateInProgress && !cancelOnGoingFormattingTask))
            {
                return;
            }

            // _formattingCancellationTokenSource.Cancel();
            // _formattingCancellationTokenSource.Dispose();
            // _formattingCancellationTokenSource = new CancellationTokenSource();

            // Assumes.NotNull(_contentScrollViewer, nameof(_contentScrollViewer));

            _isSyntaxColorizationUpdateInProgress = true;
            _isSyntaxColorizationPendingUpdate = false;

            // ITextRange? startRange
            //     = Document.GetRangeFromPoint(
            //         new Point(
            //             _contentScrollViewer!.HorizontalOffset - 500,
            //             _contentScrollViewer.VerticalOffset - 500),
            //         PointOptions.ClientCoordinates);

            // ITextRange? endRange
            //     = Document.GetRangeFromPoint(
            //         new Point(
            //             _contentScrollViewer.HorizontalOffset + _contentScrollViewer.ViewportWidth + 500,
            //             _contentScrollViewer.VerticalOffset + _contentScrollViewer.ViewportHeight + 500),
            //         PointOptions.ClientCoordinates);

            // ITextRange visibleSpan = Document.GetRange(startRange.StartPosition, endRange.EndPosition);

            // ElementTheme theme = RequestedTheme;
            // if (theme == ElementTheme.Default)
            // {
            //     if (Window.Current.Content is FrameworkElement frameworkElement)
            //     {
            //         theme = frameworkElement.RequestedTheme;
            //     }
            // }

            // var formatter = new TextFormatter(theme, Document, visibleSpan);
            // formatter.FormatAsync(Languages.CSharp, _formattingCancellationTokenSource.Token)
            //     .ContinueWith(_ =>
            //     {
            //         _isSyntaxColorizationUpdateInProgress = false;
            //     });
        }

        private string[] GetDocumentLinesCache()
        {
            if (_isDocumentLinesCachePendingUpdate)
            {
                _documentLinesCache = (GetText() + RichEditBoxDefaultLineEnding).Split(RichEditBoxDefaultLineEnding);
                _isDocumentLinesCachePendingUpdate = false;
            }

            return _documentLinesCache;
        }

        private Dictionary<int, Rect> CalculateLineNumberTextRenderingPositions(string[] lines, ITextRange startRange, ITextRange endRange)
        {
            int offset = 0;
            Dictionary<int, Rect>? lineRects = new(); // 1 - based

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string line = lines[i] ?? string.Empty;

                ITextRange range = Document.GetRange(offset, offset + line.Length);

                // Use "offset + line.Length + 1" instead of just "offset" here is to capture the line right above the viewport
                if (offset + line.Length + 1 >= startRange.StartPosition && offset <= endRange.EndPosition)
                {
                    range.GetRect(PointOptions.ClientCoordinates, out Rect rect, out _);

                    lineRects[i + 1] = rect;
                }
                else if (offset > endRange.EndPosition)
                {
                    break;
                }

                if (range.Length == 0 && range.Character == '\v')
                {
                    offset++;
                }

                offset += line.Length + 1; // 1 for line ending: 'RichEditBoxDefaultLineEnding'
            }

            return lineRects;
        }

        /// <summary>
        /// Get minimum rendering width needed for displaying number text with certain length.
        /// Take length of 3 as example, it is going to iterate thru all possible combinations like:
        /// 111, 222, 333, 444 ... 999 to get minimum rendering length needed to display all of them (the largest width is the min here).
        /// For mono font text, the width is always the same for same length but for non-mono font text, it depends.
        /// Thus we need to calculate here to determine width needed for rendering integer number only text.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        /// <param name="numberTextLength"></param>
        /// <returns></returns>
        private double CalculateMinimumRequisiteIntegerTextRenderingWidth(FontFamily fontFamily, double fontSize, int numberTextLength)
        {
            string? cacheKey = $"{fontFamily.Source}-{(int)fontSize}-{numberTextLength}";

            if (_miniRequisiteIntegerTextRenderingWidthCache.ContainsKey(cacheKey))
            {
                return _miniRequisiteIntegerTextRenderingWidthCache[cacheKey];
            }

            double minRequisiteWidth = 0;

            for (int i = 0; i < 10; i++)
            {
                string? str = new((char)('0' + i), numberTextLength);
                double width = GetTextSize(fontFamily, fontSize, str).Width;
                if (width > minRequisiteWidth)
                {
                    minRequisiteWidth = width;
                }
            }

            _miniRequisiteIntegerTextRenderingWidthCache[cacheKey] = minRequisiteWidth;
            return minRequisiteWidth;
        }

        private void RenderLineNumbersInternal(Dictionary<int, Rect> lineNumberTextRenderingPositions, double minLineNumberTextRenderingWidth)
        {
            Assumes.NotNull(_lineNumberCanvas, nameof(_lineNumberCanvas));
            Assumes.NotNull(_lineNumberGrid, nameof(_lineNumberGrid));

            double padding = FontSize / 2;
            Thickness lineNumberPadding = new Thickness(padding, 2, padding + 2, 2);
            double lineHeight = GetSingleLineHeight();
            double lineNumberTextBlockHeight = lineHeight + Padding.Top + lineNumberPadding.Top;
            SolidColorBrush? lineNumberForeground = (ActualTheme == ElementTheme.Dark) ? _lineNumberDarkModeForegroundBrush : _lineNumberLightModeForegroundBrush;

            int numOfReusableLineNumberBlocks = _renderedLineNumberBlocks.Count;

            foreach ((int lineNumber, Rect rect) in lineNumberTextRenderingPositions)
            {
                var margin
                    = new Thickness(
                        lineNumberPadding.Left,
                        rect.Top + lineNumberPadding.Top + Padding.Top,
                        lineNumberPadding.Right,
                        lineNumberPadding.Bottom);

                // Re-use already rendered line number blocks
                if (numOfReusableLineNumberBlocks > 0)
                {
                    int index = numOfReusableLineNumberBlocks - 1;
                    _renderedLineNumberBlocks[index].Text = lineNumber.ToString();
                    _renderedLineNumberBlocks[index].Margin = margin;
                    _renderedLineNumberBlocks[index].Height = lineNumberTextBlockHeight;
                    _renderedLineNumberBlocks[index].Width = minLineNumberTextRenderingWidth;
                    _renderedLineNumberBlocks[index].Visibility = Visibility.Visible;
                    _renderedLineNumberBlocks[index].Foreground = lineNumberForeground;

                    numOfReusableLineNumberBlocks--;
                }
                else // Render new line number block when there is nothing to re-use
                {
                    var lineNumberBlock = new TextBlock()
                    {
                        Text = lineNumber.ToString(),
                        Height = lineNumberTextBlockHeight,
                        Width = minLineNumberTextRenderingWidth,
                        Margin = margin,
                        TextAlignment = TextAlignment.Right,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalTextAlignment = TextAlignment.Right,
                        Foreground = lineNumberForeground
                    };

                    _lineNumberCanvas!.Children.Add(lineNumberBlock);
                    _renderedLineNumberBlocks.Add(lineNumberBlock);
                }
            }

            // Hide all un-used rendered line number block to avoid rendering collision from happening
            for (int i = 0; i < numOfReusableLineNumberBlocks; i++)
            {
                _renderedLineNumberBlocks[i].Visibility = Visibility.Collapsed;
            }

            _lineNumberGrid!.BorderThickness = new Thickness(0, 0, 0.08 * lineHeight, 0);
            _lineNumberGrid.Width = lineNumberPadding.Left + minLineNumberTextRenderingWidth + lineNumberPadding.Right;
        }

        private void ResetLineNumberCanvasClipping()
        {
            if (!_loaded || !_showLineNumbers)
            {
                return;
            }

            Assumes.NotNull(_lineNumberGrid, nameof(_lineNumberGrid));

            _lineNumberGrid!.Margin = new Thickness(0, 0, (-1 * Padding.Left) + 1, 0);
            _lineNumberGrid.Clip = new RectangleGeometry
            {
                Rect = new Rect(
                    0,
                    Padding.Top,
                    _lineNumberGrid.ActualWidth,
                    Math.Clamp(_lineNumberGrid.ActualHeight - (Padding.Top + Padding.Bottom), .0f, double.PositiveInfinity))
            };
        }

        private void UpdateLineHighlighterAndIndicator()
        {
            if (!_loaded || _lineHighlighter is null || _lineIndicator is null || _rootGrid is null)
            {
                return;
            }

            if (!_showLineNumbers && !_highlightCurrentLine)
            {
                _lineHighlighter!.Visibility = Visibility.Collapsed;
                _lineIndicator!.Visibility = Visibility.Collapsed;
                return;
            }

            Document.Selection.GetRect(PointOptions.ClientCoordinates, out Rect selectionRect, out var _);

            double singleLineHeight = GetSingleLineHeight();
            Thickness thickness = new Thickness(0.08 * singleLineHeight);
            double height = selectionRect.Height;

            // Just to make sure height is a positive number and not smaller than single line height
            if (height < singleLineHeight)
            {
                height = singleLineHeight;
            }

            // Show line highlighter rect when it is enabled when selection is single line only
            if (_highlightCurrentLine && height < singleLineHeight * 1.5f)
            {
                _lineHighlighter!.Height = height;
                _lineHighlighter.Margin = new Thickness(0, selectionRect.Y + Padding.Top, 0, 0);
                _lineHighlighter.Width = Math.Clamp(_rootGrid!.ActualWidth, 0, double.PositiveInfinity);

                _lineHighlighter.Visibility = Visibility.Visible;
            }
            else
            {
                _lineHighlighter!.Visibility = Visibility.Collapsed;
            }

            // Show line indicator when line numbers are enabled and when selection is single line only
            if (_showLineNumbers && height < singleLineHeight * 1.5f)
            {
                _lineIndicator!.Height = height;
                _lineIndicator.Margin = new Thickness(0, selectionRect.Y + Padding.Top, 0, 0);
                _lineIndicator.BorderThickness = thickness;
                _lineIndicator.Width = 0.1 * singleLineHeight;

                _lineIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                _lineIndicator!.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Thread safe way of getting the text in the active story (document)
        /// </summary>
        private string GetText()
        {
            return _document;
        }

        private double GetSingleLineHeight()
        {
            Document.GetRange(0, 0).GetRect(PointOptions.ClientCoordinates, out Rect rect, out _);
            return rect.Height <= 0 ? 1.35 * FontSize : rect.Height;
        }

        private void EnterWithAutoIndentation()
        {
            // Automatically indent on new lines based on current line's leading spaces/tabs
            GetLineColumnSelection(out var startLineIndex, out _, out var startColumnIndex, out _, out _, out _);
            var lines = GetDocumentLinesCache();
            var leadingSpacesAndTabs = LeadingSpacesAndTabs(lines[startLineIndex - 1].Substring(0, startColumnIndex - 1));
            Document.Selection.SetText(TextSetOptions.None, RichEditBoxDefaultLineEnding + leadingSpacesAndTabs);
            Document.Selection.StartPosition = Document.Selection.EndPosition;
        }

        /// <summary>
        /// Returns 1-based indexing values
        /// </summary>
        private void GetLineColumnSelection(
            out int startLineIndex,
            out int endLineIndex,
            out int startColumnIndex,
            out int endColumnIndex,
            out int selectedCount,
            out int lineCount)
        {
            var lines = GetDocumentLinesCache();
            GetTextSelectionPosition(out var start, out var end);

            startLineIndex = 1;
            startColumnIndex = 1;
            endLineIndex = 1;
            endColumnIndex = 1;
            selectedCount = 0;
            lineCount = lines.Length - 1;

            var length = 0;
            bool startLocated = false;

            for (int i = 0; i < lineCount + 1; i++)
            {
                var line = lines[i];

                if (line.Length + length >= start && !startLocated)
                {
                    startLineIndex = i + 1;
                    startColumnIndex = start - length + 1;
                    startLocated = true;
                }

                if (line.Length + length >= end)
                {
                    if (i == startLineIndex - 1)
                    {
                        selectedCount = end - start;
                    }
                    else
                    {
                        selectedCount = end - start + (i - startLineIndex) + 1;
                    }

                    endLineIndex = i + 1;
                    endColumnIndex = end - length + 1;

                    // Reposition end position to previous line's end position if last selected char is RichEditBoxDefaultLineEnding ('\r')
                    if (endColumnIndex == 1 && end != start)
                    {
                        endLineIndex--;
                        endColumnIndex = lines[i - 1].Length + 1;
                    }

                    return;
                }

                length += line.Length + 1;
            }
        }

        /// <summary>
        /// Thread safe way of getting the current document selection position
        /// </summary>
        private void GetTextSelectionPosition(out int startPosition, out int endPosition)
        {
            startPosition = _textSelectionStartPosition;
            endPosition = _textSelectionEndPosition;
        }

        private async Task PastePlainTextFromWindowsClipboardAsync(TextControlPasteEventArgs? args)
        {
            if (args != null)
            {
                args.Handled = true;
            }

            if (!Document.CanPaste())
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

                Document.BeginUndoGroup();
                Document.Selection.SetText(TextSetOptions.None, text);
                Document.Selection.StartPosition = Document.Selection.EndPosition;
                Document.EndUndoGroup();
            }
            catch (Exception ex)
            {
                // TODO: Log this.
            }
        }

        private void SettingProvider_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            ApplySettings();
        }

        private void OnContentScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs _)
        {
            UpdateLineNumbersRendering();
            UpdateSyntaxColorization(cancelOnGoingFormattingTask: false);
        }

        private void OnContentScrollViewerSizeChanged(object sender, SizeChangedEventArgs _)
        {
            UpdateLineNumbersRendering();
            UpdateSyntaxColorization(cancelOnGoingFormattingTask: false);
        }

        private void OnVerticalScrollBarValueChanged(object sender, RangeBaseValueChangedEventArgs _)
        {
            Assumes.NotNull(_contentScrollViewer, nameof(_contentScrollViewer));
            Assumes.NotNull(_lineNumberCanvas, nameof(_lineNumberCanvas));
            Assumes.NotNull(_lineHighlighterAndIndicatorCanvas, nameof(_lineHighlighterAndIndicatorCanvas));

            // Make sure line number canvas is in sync with editor's ScrollViewer
            _contentScrollViewer!.StartExpressionAnimation(_lineNumberCanvas!, Axis.Y);

            // Make sure line highlighter and indicator canvas is in sync with editor's ScrollViewer
            _contentScrollViewer!.StartExpressionAnimation(_lineHighlighterAndIndicatorCanvas!, Axis.Y);
        }

        private void OnLineNumberGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetLineNumberCanvasClipping();
        }

        private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs _)
        {
            ResetRootGridClipping();
        }

        private void OnPaste(object sender, TextControlPasteEventArgs e)
        {
            PastePlainTextFromWindowsClipboardAsync(e).Forget();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;

            ResetRootGridClipping();

            UpdateLineHighlighterAndIndicator();
            if (_showLineNumbers)
            {
                ShowLineNumbers();
            }

            UpdateSyntaxColorization(cancelOnGoingFormattingTask: false);
        }

        private void OnSelectionChanging(RichEditBox sender, RichEditBoxSelectionChangingEventArgs args)
        {
            _textSelectionStartPosition = args.SelectionStart;
            _textSelectionEndPosition = args.SelectionStart + args.SelectionLength;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs _)
        {
            UpdateLineHighlighterAndIndicator();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs _)
        {
            UpdateLineHighlighterAndIndicator();
        }

        private void OnTextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                Document.GetText(TextGetOptions.UseCrlf, out var document);
                _document = document;

                if (!_isTextPendingUpdate)
                {
                    _isTextPendingUpdate = true;
                    Text = document;
                    _isTextPendingUpdate = false;
                }

                _isDocumentLinesCachePendingUpdate = true;
                _isSyntaxColorizationPendingUpdate = true;
            }
        }

        private void OnTextChanged(object sender, RoutedEventArgs _)
        {
            UpdateLineNumbersRendering();

            if (_isSyntaxColorizationPendingUpdate)
            {
                UpdateSyntaxColorization(cancelOnGoingFormattingTask: true);
            }
        }

        private static void OnSettingsProviderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ISettingsProvider? settingProvider = e.NewValue as ISettingsProvider;
            if (settingProvider is not null)
            {
                var textEditorCore = (TextEditor)d;
                settingProvider.SettingChanged += textEditorCore.SettingProvider_SettingChanged;
                textEditorCore.ApplySettings();
            }
        }

        private static void OnTextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string? text = e.NewValue as string;
            var textEditorCore = (TextEditor)d;


            if (!textEditorCore._isTextPendingUpdate)
            {
                textEditorCore._isTextPendingUpdate = true;
                bool isReadOnly = textEditorCore.IsReadOnly;
                textEditorCore.IsReadOnly = false;
                textEditorCore.Document.SetText(TextSetOptions.None, text ?? string.Empty);
                textEditorCore.IsReadOnly = isReadOnly;
                textEditorCore._isTextPendingUpdate = false;
            }
        }

        private static Size GetTextSize(FontFamily font, double fontSize, string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontFamily = font,
                FontSize = fontSize
            };
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return textBlock.DesiredSize;
        }

        private static string LeadingSpacesAndTabs(string str)
        {
            int i = 0;
            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (ch != ' ' && ch != '\t')
                {
                    break;
                }
            }
            return str.Substring(0, i);
        }
    }
}
