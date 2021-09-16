#nullable enable

using ColorCode;
using DevTools.Core;
using DevTools.Core.Settings;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace DevTools.Common.UI.Controls.FormattedTextBlock
{
    public sealed partial class FormattedTextBlock : UserControl, IFormattedTextBlock
    {
        private readonly Dictionary<string, double> _miniRequisiteIntegerTextRenderingWidthCache = new();
        private readonly SolidColorBrush _lineNumberDarkModeForegroundBrush = new("#99EEEEEE".ToColor());
        private readonly SolidColorBrush _lineNumberLightModeForegroundBrush = new("#99000000".ToColor());

        private bool _showLineNumbers;
        private bool _highlightCurrentLine;
        private bool _loaded;
        private string? _document;

        public static readonly DependencyProperty SettingsProviderProperty
            = DependencyProperty.Register(
                nameof(SettingsProvider),
                typeof(ISettingsProvider),
                typeof(FormattedTextBlock),
                new PropertyMetadata(null, OnSettingsProviderPropertyChangedCallback));

        public ISettingsProvider? SettingsProvider
        {
            get => (ISettingsProvider?)GetValue(SettingsProviderProperty);
            set => SetValue(SettingsProviderProperty, value);
        }

        public FormattedTextBlock()
        {
            InitializeComponent();

            RichTextBlock.Loaded += OnLoaded;
            RichTextBlock.SelectionChanged += OnSelectionChanged;
            RichTextBlock.SizeChanged += OnSizeChanged;
            RichTextBlock.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true);
        }

        public void SetText(string? text, ILanguage? language = null)
        {
            _document = text;

            RichTextBlock.Blocks.Clear();
            RichTextBlock.TextHighlighters.Clear();

            if (!string.IsNullOrEmpty(text))
            {
                var formatter = new RichTextBlockFormatter(ActualTheme);
                formatter.FormatRichTextBlock(text, language, RichTextBlock);
            }

            ApplySettings();
        }

        public void SetInlineTextDiff(string? oldText, string? newText, bool lineDiff)
        {
            oldText ??= string.Empty;
            newText ??= string.Empty;

            var inlineDiffBuilder
                = new InlineDiffBuilder(new Differ());

            IChunker chunker;
            if (lineDiff)
            {
                chunker = LineChunker.Instance;
            }
            else
            {
                chunker = CustomWordChunker.Instance;
            }

            DiffPaneModel diffPaneModel
                = inlineDiffBuilder.BuildDiffModel(
                    oldText,
                    newText,
                    ignoreWhitespace: false,
                    ignoreCase: false,
                    chunker);

            if (!diffPaneModel.HasDifferences)
            {
                _document = newText;
                var p = new Paragraph();
                var r = new Run()
                {
                    Text = _document
                };
                p.Inlines.Add(r);
                RichTextBlock.Blocks.Clear();
                RichTextBlock.TextHighlighters.Clear();
                RichTextBlock.Blocks.Add(p);
                ApplySettings();
                return;
            }
            else
            {
                _document = DiffPaneModelToString(diffPaneModel, lineDiff);
            }

            var insertedTextLength = 0;

            var deletedHighlighter
                = new TextHighlighter()
                {
                    Background = new SolidColorBrush(Colors.Red)
                };
            var insertedHighlighter
                = new TextHighlighter()
                {
                    Background = new SolidColorBrush(Colors.Green)
                };
            var modifiedHighlighter
                = new TextHighlighter()
                {
                    Background = new SolidColorBrush(Colors.Yellow)
                };

            for (int i = 0; i < diffPaneModel.Lines.Count; i++)
            {
                DiffPiece diffPiece = diffPaneModel.Lines[i];

                if (diffPiece.Type != ChangeType.Unchanged)
                {
                    int startIndex = insertedTextLength;

                    var textRange
                        = new TextRange()
                        {
                            StartIndex = startIndex,
                            Length = diffPiece.Text.Length
                        };

                    if (diffPiece.Type == ChangeType.Inserted)
                    {
                        insertedHighlighter.Ranges.Add(textRange);
                    }
                    else if (diffPiece.Type == ChangeType.Deleted)
                    {
                        deletedHighlighter.Ranges.Add(textRange);
                    }
                    else if (diffPiece.Type == ChangeType.Modified)
                    {
                        modifiedHighlighter.Ranges.Add(textRange);
                    }
                }

                insertedTextLength += diffPiece.Text.Length;
                if (lineDiff)
                {
                    insertedTextLength += "\r\n".Length;
                }
            }

            var documentParagraph = new Paragraph();
            var run = new Run()
            {
                Text = _document
            };
            documentParagraph.Inlines.Add(run);
            RichTextBlock.Blocks.Clear();
            RichTextBlock.TextHighlighters.Clear();

            RichTextBlock.TextHighlighters.Add(deletedHighlighter);
            RichTextBlock.TextHighlighters.Add(insertedHighlighter);
            RichTextBlock.TextHighlighters.Add(modifiedHighlighter);
            RichTextBlock.Blocks.Add(documentParagraph);

            ApplySettings();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ContentElement.ViewChanged += OnContentScrollViewerViewChanged;
            ContentElement.SizeChanged += OnContentScrollViewerSizeChanged;
            ContentElement.ApplyTemplate();

            RootGrid.SizeChanged += OnRootGridSizeChanged;
        }

        private void ApplySettings()
        {
            ISettingsProvider? settingsProvider = SettingsProvider;
            if (settingsProvider is not null)
            {
                RichTextBlock.FontFamily = (FontFamily)Application.Current.Resources[settingsProvider.GetSetting(PredefinedSettings.TextEditorFont)];
                RichTextBlock.TextWrapping = settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping) ? TextWrapping.Wrap : TextWrapping.NoWrap;
                _showLineNumbers = settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers);
                _highlightCurrentLine = settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine);

                UpdateLayout();

                if (_showLineNumbers)
                {
                    ShowLineNumbers();
                }
                else
                {
                    HideLineNumbers();
                }
            }
        }

        private void ShowLineNumbers()
        {
            if (!_loaded)
            {
                return;
            }

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

            LineNumberGrid!.Children.Clear();
            LineNumberGrid.BorderThickness = new Thickness(0, 0, 0, 0);
            LineNumberGrid.Width = .0f;

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

            LineNumberGrid!.Children.Clear();

            int lineCount = LineCount(_document);

            double minLineNumberTextRenderingWidth
                = CalculateMinimumRequisiteIntegerTextRenderingWidth(
                    RichTextBlock.FontFamily,
                    RichTextBlock.FontSize,
                    lineCount.ToString().Length);

            double padding = RichTextBlock.FontSize / 2;
            Thickness lineNumberPadding = new(padding, 0, padding + 2, 1);
            //double lineHeight = RichTextBlock.LineHeight;
            double lineHeight = GetTextSize(RichTextBlock.FontFamily, RichTextBlock.FontSize, "1").Height;
            double lineNumberTextBlockHeight = lineHeight + RichTextBlock.Padding.Top + lineNumberPadding.Top;
            SolidColorBrush? lineNumberForeground = (ActualTheme == ElementTheme.Dark) ? _lineNumberDarkModeForegroundBrush : _lineNumberLightModeForegroundBrush;

            for (int i = 0; i < lineCount; i++)
            {
                int lineNumber = i + 1;

                var margin
                    = new Thickness(
                        lineNumberPadding.Left,
                        lineNumberPadding.Top,
                        lineNumberPadding.Right,
                        lineNumberPadding.Bottom);

                var lineNumberBlock = new TextBlock()
                {
                    Text = lineNumber.ToString(),
                    //  Height = lineNumberTextBlockHeight,
                    //  Width = minLineNumberTextRenderingWidth,
                    Margin = margin,
                    TextAlignment = TextAlignment.Right,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalTextAlignment = TextAlignment.Right,
                    Foreground = lineNumberForeground,
                    FontFamily = RichTextBlock.FontFamily,
                    FontSize = RichTextBlock.FontSize
                };

                LineNumberGrid!.Children.Add(lineNumberBlock);
            }

            LineNumberGrid!.BorderThickness = new Thickness(0, 0, 0.08 * lineHeight, 0);
            LineNumberGrid.Width = lineNumberPadding.Left + minLineNumberTextRenderingWidth + lineNumberPadding.Right;
        }

        private void UpdateLineHighlighterAndIndicator()
        {
            if (!_loaded)
            {
                return;
            }

            if (!_showLineNumbers && !_highlightCurrentLine)
            {
                LineHighlighter!.Visibility = Visibility.Collapsed;
                LineIndicator!.Visibility = Visibility.Collapsed;
                return;
            }

            TextPointer? anchorPoint = RichTextBlock.SelectionStart;
            if (anchorPoint is null)
            {
                anchorPoint = RichTextBlock.ContentStart!;
                Assumes.NotNull(anchorPoint, nameof(anchorPoint));
            }

            Rect selectionRect = anchorPoint.GetCharacterRect(anchorPoint.LogicalDirection);

            double singleLineHeight = GetSingleLineHeight();
            Thickness thickness = new Thickness(0.08 * singleLineHeight);
            double height = selectionRect.Height;
            bool selectedMultipleLines = RichTextBlock.SelectedText.IndexOfAny(new[] { '\n', '\r' }) > -1;

            // Just to make sure height is a positive number and not smaller than single line height
            if (height < singleLineHeight)
            {
                height = singleLineHeight;
            }

            // Show line highlighter rect when it is enabled when selection is single line only
            if (_highlightCurrentLine && !selectedMultipleLines)
            {
                LineHighlighter!.Height = height;
                LineHighlighter.Margin = new Thickness(0, selectionRect.Top, 0, 0);
                LineHighlighter.Width = Math.Clamp(RootGrid!.ActualWidth, 0, double.PositiveInfinity);

                LineHighlighter.Visibility = Visibility.Visible;
            }
            else
            {
                LineHighlighter!.Visibility = Visibility.Collapsed;
            }

            // Show line indicator when line numbers are enabled and when selection is single line only
            if (_showLineNumbers && !selectedMultipleLines)
            {
                LineIndicator!.Height = height;
                LineIndicator.Margin = new Thickness(0, selectionRect.Top, 0, 0);
                LineIndicator.BorderThickness = thickness;
                LineIndicator.Width = 0.1 * singleLineHeight;

                LineIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                LineIndicator!.Visibility = Visibility.Collapsed;
            }
        }

        private double GetSingleLineHeight()
        {
            Rect rect = RichTextBlock.ContentStart.GetCharacterRect(LogicalDirection.Forward);
            return rect.Height <= 0 ? 1.35 * FontSize : rect.Height;
        }

        private void ResetRootGridClipping()
        {
            if (!_loaded || RootGrid is null)
            {
                return;
            }

            //RootGrid!.Clip = new RectangleGeometry
            //{
            //    Rect = new Rect(
            //        0,
            //        0,
            //        RootGrid.ActualWidth,
            //        Math.Clamp(RootGrid.ActualHeight, .0f, double.PositiveInfinity))
            //};
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;

            ResetRootGridClipping();

            UpdateLineHighlighterAndIndicator();
            if (_showLineNumbers)
            {
                ShowLineNumbers();
            }
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs _)
        {
            UpdateLineHighlighterAndIndicator();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs _)
        {
            UpdateLineHighlighterAndIndicator();
        }

        private void OnContentScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs _)
        {
            UpdateLineNumbersRendering();
        }

        private void OnContentScrollViewerSizeChanged(object sender, SizeChangedEventArgs _)
        {
            UpdateLineNumbersRendering();
        }

        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = RichTextBlock.GetPositionFromPoint(e.GetCurrentPoint(RichTextBlock).Position);
            RichTextBlock.Select(point, point);
            UpdateLineHighlighterAndIndicator();
        }

        private void SettingProvider_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            ApplySettings();
        }

        private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs _)
        {
            ResetRootGridClipping();
        }

        private static void OnSettingsProviderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ISettingsProvider? settingProvider = e.NewValue as ISettingsProvider;
            if (settingProvider is not null)
            {
                var textEditorCore = (FormattedTextBlock)d;
                settingProvider.SettingChanged += textEditorCore.SettingProvider_SettingChanged;
                textEditorCore.ApplySettings();
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

        private static int LineCount(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 1;
            }

            int count = 1;
            int len = text!.Length;
            for (int i = 0; i != len; ++i)
            {
                switch (text[i])
                {
                    case '\r':
                        ++count;
                        if (i + 1 != len && text[i + 1] == '\n')
                        {
                            ++i;
                        }
                        break;

                    case '\n':
                        // case '\v':
                        // case '\f':
                        // case '\u0085':
                        // case '\u2028':
                        // case '\u2029':
                        ++count;
                        break;
                }
            }

            return count;
        }

        private static string DiffPaneModelToString(DiffPaneModel diffPaneModel, bool lineDiff)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < diffPaneModel.Lines.Count; i++)
            {
                stringBuilder.Append(diffPaneModel.Lines[i].Text);
                if (lineDiff)
                {
                    stringBuilder.Append("\r\n");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
