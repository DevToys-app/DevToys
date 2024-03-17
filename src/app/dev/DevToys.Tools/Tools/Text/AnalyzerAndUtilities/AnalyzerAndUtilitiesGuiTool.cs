using System.Text.RegularExpressions;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.Tools.Tools.Text.AnalyzerAndUtilities;

[Export(typeof(IGuiTool))]
[Name("TextAnalyzerAndUtilities")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0131',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.AnalyzerAndUtilities.AnalyzerAndUtilities",
    ShortDisplayTitleResourceName = nameof(AnalyzerAndUtilities.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(AnalyzerAndUtilities.LongDisplayTitle),
    DescriptionResourceName = nameof(AnalyzerAndUtilities.Description),
    AccessibleNameResourceName = nameof(AnalyzerAndUtilities.AccessibleName),
    SearchKeywordsResourceName = nameof(AnalyzerAndUtilities.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed class AnalyzerAndUtilitiesGuiTool : IGuiTool, IDisposable
{
    private enum GridRows
    {
        TopAuto,
        Stretch,
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly IUIMultiLineTextInput _textInput = MultilineTextInput();
    private readonly IUISelectDropDownList _endOfLineSequenceDropDownList = SelectDropDownList();
    private readonly IUIButton _originalTextButton = Button();
    private readonly IUIDataGrid _statisticsDataGrid = DataGrid();
    private readonly DisposableSemaphore _updateSemaphore = new();

    private string? _originalText;
    private bool _forbidBackup;
    private CancellationTokenSource? _selectionStatisticsCancellationTokenSource;
    private CancellationTokenSource? _textStatisticsCancellationTokenSource;

    private int _statisticSelectionLength;
    private int _statisticSelectionStartPosition;
    private int _statisticSelectionEndPosition;
    private int _statisticSelectionLineNumber;
    private int _statisticSelectionColumnNumber;
    private int _statisticByteCount;
    private int _statisticCharacterCount;
    private int _statisticWordCount;
    private int _statisticSentenceCount;
    private int _statisticParagraphCount;
    private int _statisticLineCount;
    private EndOfLineSequence _statisticEndOfLineSequence;
    private Dictionary<char, int>? _statisticCharacterFrequency;
    private Dictionary<string, int>? _statisticWordFrequency;

    [ImportingConstructor]
    public AnalyzerAndUtilitiesGuiTool()
    {
        _textInput.SelectionChanged += OnTextInputSelectionChanged;
    }

    public UIToolView View
        => new(
            isScrollable: false,
            Grid()
                .ColumnMediumSpacing()

                .Rows(
                    (GridRows.TopAuto, Auto),
                    (GridRows.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Columns(
                    (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.TopAuto,
                        GridColumns.Stretch,

                        Stack()
                            .Vertical()
                            .LargeSpacing()
                            .WithChildren(

                                // Convert line break
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(

                                        Label()
                                            .Text(AnalyzerAndUtilities.ConvertEndOfLineSequence),
                                        Wrap()
                                            .SmallSpacing()
                                            .WithChildren(
                                                Button().Text(AnalyzerAndUtilities.EndOfLineSequenceLF).OnClick(OnEndOfLineSequenceLFButtonClick),
                                                Button().Text(AnalyzerAndUtilities.EndOfLineSequenceCRLF).OnClick(OnEndOfLineSequenceCRLFButtonClick))),

                                // Convert case
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(

                                        Label()
                                            .Text(AnalyzerAndUtilities.ConvertCase),
                                        Wrap()
                                            .SmallSpacing()
                                            .WithChildren(
                                                Button().Text(AnalyzerAndUtilities.LowerCase).OnClick(OnLowerCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.UpperCase).OnClick(OnUpperCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.SentenceCase).OnClick(OnSentenceCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.TitleCase).OnClick(OnTitleCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.CamelCase).OnClick(OnCamelCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.PascalCase).OnClick(OnPascalCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.SnakeCase).OnClick(OnSnakeCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.ConstantCase).OnClick(OnConstantCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.KebabCase).OnClick(OnKebabCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.CobolCase).OnClick(OnCobolCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.TrainCase).OnClick(OnTrainCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.AlternatingCase).OnClick(OnAlternatingCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.InverseCase).OnClick(OnInverseCaseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.RandomCase).OnClick(OnRandomCaseButtonClick))),

                                // Sort lines
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(

                                        Label()
                                            .Text(AnalyzerAndUtilities.SortLines),
                                        Wrap()
                                            .SmallSpacing()
                                            .WithChildren(
                                                Button().Text(AnalyzerAndUtilities.Alphabetize).OnClick(OnAlphabetizeButtonClick),
                                                Button().Text(AnalyzerAndUtilities.ReverseAlphabetize).OnClick(OnReverseAlphabetizeButtonClick),
                                                Button().Text(AnalyzerAndUtilities.AlphabetizeLast).OnClick(OnAlphabetizeLastButtonClick),
                                                Button().Text(AnalyzerAndUtilities.ReverseAlphabetizeLast).OnClick(OnReverseAlphabetizeLastButtonClick),
                                                Button().Text(AnalyzerAndUtilities.Reverse).OnClick(OnReverseButtonClick),
                                                Button().Text(AnalyzerAndUtilities.Randomize).OnClick(OnRandomizeButtonClick))))),

                    Cell(
                        GridRows.Stretch,
                        GridColumns.Stretch,

                        SplitGrid()
                            .Vertical()
                            .RightPaneLength(new UIGridLength(250, UIGridUnitType.Pixel))

                            .WithLeftPaneChild(
                                _textInput
                                    .Title(AnalyzerAndUtilities.TextInput)
                                    .CanCopyWhenEditable()
                                    .AlwaysWrap()
                                    .OnTextChanged(OnInputTextChanged)
                                    .CommandBarExtraContent(
                                        _originalTextButton
                                            .AccentAppearance()
                                            .Text(AnalyzerAndUtilities.ShowOriginalText)
                                            .OnClick(OnOriginalTextButtonClick)))

                            .WithRightPaneChild(
                                _statisticsDataGrid
                                    .Title(AnalyzerAndUtilities.Statistics)
                                    .ForbidSelectItem()
                                    .Extendable()
                                    .WithColumns(AnalyzerAndUtilities.StatisticTitle, AnalyzerAndUtilities.StatisticValue)))));

    public void Dispose()
    {
        _selectionStatisticsCancellationTokenSource?.Cancel();
        _selectionStatisticsCancellationTokenSource?.Dispose();
        _textStatisticsCancellationTokenSource?.Cancel();
        _textStatisticsCancellationTokenSource?.Dispose();
        _updateSemaphore.Dispose();
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
        {
            _originalText = text;
            OnOriginalTextButtonClick();
        }
    }

    private void OnInputTextChanged(string newText)
    {
        if (!_forbidBackup)
        {
            _originalTextButton.Disable();
            _originalText = newText;
        }
        else
        {
            _originalTextButton.Enable();
        }

        _textStatisticsCancellationTokenSource?.Cancel();
        _textStatisticsCancellationTokenSource?.Dispose();
        _textStatisticsCancellationTokenSource = new CancellationTokenSource();

        ComputeTextStatisticsAsync(newText, _textStatisticsCancellationTokenSource.Token).ForgetSafely();
    }

    private void OnTextInputSelectionChanged(object? sender, EventArgs e)
    {
        _selectionStatisticsCancellationTokenSource?.Cancel();
        _selectionStatisticsCancellationTokenSource?.Dispose();
        _selectionStatisticsCancellationTokenSource = new CancellationTokenSource();

        ComputeSelectionStatisticsAsync(_textInput.Text, _textInput.Selection, _selectionStatisticsCancellationTokenSource.Token).ForgetSafely();
    }

    private void OnEndOfLineSequenceLFButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertLineBreakToLF(_textInput.Text));
    }

    private void OnEndOfLineSequenceCRLFButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertLineBreakToCRLF(_textInput.Text));
    }

    private void OnLowerCaseButtonClick()
    {
        SetTextWithoutBackup(_textInput.Text?.ToLowerInvariant());
    }

    private void OnUpperCaseButtonClick()
    {
        SetTextWithoutBackup(_textInput.Text?.ToUpperInvariant());
    }

    private void OnSentenceCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToSentenceCase(_textInput.Text, CancellationToken.None));
    }

    private void OnTitleCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToTitleCase(_textInput.Text, CancellationToken.None));
    }

    private void OnCamelCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToCamelCase(_textInput.Text, CancellationToken.None));
    }

    private void OnPascalCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToPascalCase(_textInput.Text, CancellationToken.None));
    }

    private void OnSnakeCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToSnakeCase(_textInput.Text, CancellationToken.None));
    }

    private void OnConstantCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToConstantCase(_textInput.Text, CancellationToken.None));
    }

    private void OnKebabCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToKebabCase(_textInput.Text, CancellationToken.None));
    }

    private void OnCobolCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToCobolCase(_textInput.Text, CancellationToken.None));
    }

    private void OnTrainCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToTrainCase(_textInput.Text, CancellationToken.None));
    }

    private void OnAlternatingCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToAlternatingCase(_textInput.Text, CancellationToken.None));
    }

    private void OnInverseCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToInverseCase(_textInput.Text, CancellationToken.None));
    }

    private void OnRandomCaseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ConvertToRandomCase(_textInput.Text, CancellationToken.None));
    }

    private void OnAlphabetizeButtonClick()
    {
        SetTextWithoutBackup(StringHelper.SortLinesAlphabetically(_textInput.Text, _statisticEndOfLineSequence));
    }

    private void OnReverseAlphabetizeButtonClick()
    {
        SetTextWithoutBackup(StringHelper.SortLinesAlphabeticallyDescending(_textInput.Text, _statisticEndOfLineSequence));
    }

    private void OnAlphabetizeLastButtonClick()
    {
        SetTextWithoutBackup(StringHelper.SortLinesByLastWord(_textInput.Text, _statisticEndOfLineSequence));
    }

    private void OnReverseAlphabetizeLastButtonClick()
    {
        SetTextWithoutBackup(StringHelper.SortLinesByLastWordDescending(_textInput.Text, _statisticEndOfLineSequence));
    }

    private void OnReverseButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ReverseLines(_textInput.Text, _statisticEndOfLineSequence));
    }

    private void OnRandomizeButtonClick()
    {
        SetTextWithoutBackup(StringHelper.ShuffleLines(_textInput.Text, _statisticEndOfLineSequence));
    }

    private void OnOriginalTextButtonClick()
    {
        SetTextWithoutBackup(_originalText);
        _originalTextButton.Disable();
    }

    private void SetTextWithoutBackup(string? text)
    {
        _forbidBackup = true;
        _textInput.Text(text ?? string.Empty);
        _forbidBackup = false;
    }

    private async Task ComputeTextStatisticsAsync(string text, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        StringHelper.AnalyzeText(
            text,
            cancellationToken,
            out int byteCount,
            out int characterCount,
            out int wordCount,
            out int sentenceCount,
            out int paragraphCount,
            out int lineCount,
            out EndOfLineSequence endOfLineSequence,
            out Dictionary<char, int> characterFrequency,
            out Dictionary<string, int> wordFrequency);

        using (await _updateSemaphore.WaitAsync(cancellationToken))
        {
            _statisticByteCount = byteCount;
            _statisticCharacterCount = characterCount;
            _statisticWordCount = wordCount;
            _statisticSentenceCount = sentenceCount;
            _statisticParagraphCount = paragraphCount;
            _statisticLineCount = lineCount;
            _statisticEndOfLineSequence = endOfLineSequence;
            _statisticCharacterFrequency = characterFrequency;
            _statisticWordFrequency = wordFrequency;

            DisplayStatisticsAndUpdateDetectedEndOfLineSequence();
        }
    }

    private async Task ComputeSelectionStatisticsAsync(string text, TextSpan selection, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        StringHelper.AnalyzeSpan(
            text,
            selection,
            cancellationToken,
            out int spanLength,
            out int spanStartPosition,
            out int spanEndPosition,
            out int spanLineNumber,
            out int spanColumnNumber);

        using (await _updateSemaphore.WaitAsync(cancellationToken))
        {
            _statisticSelectionLength = spanLength;
            _statisticSelectionStartPosition = spanStartPosition;
            _statisticSelectionEndPosition = spanEndPosition;
            _statisticSelectionLineNumber = spanLineNumber;
            _statisticSelectionColumnNumber = spanColumnNumber;

            DisplayStatisticsAndUpdateDetectedEndOfLineSequence();
        }
    }

    private void DisplayStatisticsAndUpdateDetectedEndOfLineSequence()
    {
        Guard.IsTrue(_updateSemaphore.IsBusy);

        string endOfLineSequenceString
            = _statisticEndOfLineSequence switch
            {
                EndOfLineSequence.CarriageReturn => AnalyzerAndUtilities.EndOfLineSequenceCR,
                EndOfLineSequence.CarriageReturnLineFeed => AnalyzerAndUtilities.EndOfLineSequenceCRLF,
                EndOfLineSequence.LineFeed => AnalyzerAndUtilities.EndOfLineSequenceLF,
                EndOfLineSequence.Mixed => AnalyzerAndUtilities.EndOfLineSequenceMixed,
                EndOfLineSequence.Unknown => AnalyzerAndUtilities.EndOfLineSequenceUnknown,
                _ => string.Empty
            };

        var rows
            = new List<IUIDataGridRow>
            {
                CreateStatisticTitleRow(AnalyzerAndUtilities.SelectionStatistics),
                Row(null, Cell(AnalyzerAndUtilities.SelectionLength), Cell(_statisticSelectionLength.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.SelectionStartPosition), Cell(_statisticSelectionStartPosition.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.SelectionEndPosition), Cell(_statisticSelectionEndPosition.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.SelectionLineNumber), Cell(_statisticSelectionLineNumber.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.SelectionColumnNumber), Cell(_statisticSelectionColumnNumber.ToString())),
                CreateStatisticTitleRow(AnalyzerAndUtilities.TextStatistics),
                Row(null, Cell(AnalyzerAndUtilities.Bytes), Cell(_statisticByteCount.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.Characters), Cell(_statisticCharacterCount.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.Words), Cell(_statisticWordCount.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.Sentences), Cell(_statisticSentenceCount.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.Paragraphs), Cell(_statisticParagraphCount.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.Lines), Cell(_statisticLineCount.ToString())),
                Row(null, Cell(AnalyzerAndUtilities.EndOfLineSequence), Cell(endOfLineSequenceString))
            };

        if (_statisticWordFrequency is not null)
        {
            if (_statisticWordFrequency.Count > 0)
            {
                rows.Add(CreateStatisticTitleRow(AnalyzerAndUtilities.WordFrequency));
            }

            foreach (KeyValuePair<string, int> word in _statisticWordFrequency.OrderByDescending(a => a.Value))
            {
                rows.Add(Row(null, Cell(word.Key), Cell(word.Value.ToString())));
            }
        }

        if (_statisticCharacterFrequency is not null)
        {
            if (_statisticCharacterFrequency.Count > 0)
            {
                rows.Add(CreateStatisticTitleRow(AnalyzerAndUtilities.CharacterFrequency));
            }

            foreach (KeyValuePair<char, int> character in _statisticCharacterFrequency.OrderByDescending(a => a.Value))
            {
                string displayedKey = character.Key.ToString();
                if (displayedKey == " ")
                {
                    displayedKey = "⎵";
                }
                else
                {
                    displayedKey = Regex.Escape(displayedKey);
                }
                rows.Add(Row(null, Cell(displayedKey), Cell(character.Value.ToString())));
            }
        }

        _statisticsDataGrid.WithRows(rows.ToArray());

        _endOfLineSequenceDropDownList.Select((int)_statisticEndOfLineSequence);
    }

    private static IUIDataGridRow CreateStatisticTitleRow(string title)
        => Row(
            null,
            Cell(
                Label()
                    .NeverWrap()
                    .Style(UILabelStyle.Subtitle)
                    .Text(title)),
            Cell(Label()));
}
