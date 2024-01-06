using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Testers.RegExTester;

[Export(typeof(IGuiTool))]
[Name("RegExTester")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0113',
    GroupName = PredefinedCommonToolGroupNames.Testers,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Testers.RegExTester.RegExTester",
    ShortDisplayTitleResourceName = nameof(RegExTester.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(RegExTester.LongDisplayTitle),
    DescriptionResourceName = nameof(RegExTester.Description),
    AccessibleNameResourceName = nameof(RegExTester.AccessibleName),
    SearchKeywordsResourceName = nameof(RegExTester.SearchKeywords))]
[NoCompactOverlaySupport] // UI is too crowded for it. TODO: Consider adding an event to IThemeListener to notify when entering Picture-in-Picture mode and update the UI accordingly.
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed class RegExTesterGuiTool : IGuiTool, IDisposable
{
    private static readonly IUIDataGridRow[] cheatSheetRows
        = new[]
        {
            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategoryCharacterClasses),
            CreateCheatSheetRow(@".", RegExTester.CheatSheetAnyCharacter, RegExTester.CheatSheetAnyCharacterDetails),
            CreateCheatSheetRow(@"[Abc]", RegExTester.CheatSheetCharacterSet, RegExTester.CheatSheetCharacterSetDetails),
            CreateCheatSheetRow(@"[^Abc]", RegExTester.CheatSheetNegatedCharacterSet, RegExTester.CheatSheetNegatedCharacterSetDetails),
            CreateCheatSheetRow(@"[A-Z]", RegExTester.CheatSheetCharacterRange, RegExTester.CheatSheetCharacterRangeDetails),
            CreateCheatSheetRow(@"[\s\S]", RegExTester.CheatSheetMatchAny, RegExTester.CheatSheetMatchAnyDetails),
            CreateCheatSheetRow(@"\X", RegExTester.CheatSheetUnicodeGrapheme, RegExTester.CheatSheetUnicodeGraphemeDetails),
            CreateCheatSheetRow(@"\w", RegExTester.CheatSheetWord, RegExTester.CheatSheetWordDetails),
            CreateCheatSheetRow(@"\W", RegExTester.CheatSheetNotWord, RegExTester.CheatSheetNotWordDetails),
            CreateCheatSheetRow(@"\d", RegExTester.CheatSheetDigit, RegExTester.CheatSheetDigitDetails),
            CreateCheatSheetRow(@"\D", RegExTester.CheatSheetNotDigit, RegExTester.CheatSheetNotDigitDetails),
            CreateCheatSheetRow(@"\s", RegExTester.CheatSheetWhitespace, RegExTester.CheatSheetWhitespaceDetails),
            CreateCheatSheetRow(@"\S", RegExTester.CheatSheetNotWhitespace, RegExTester.CheatSheetNotWhitespaceDetails),
            CreateCheatSheetRow(@"\h", RegExTester.CheatSheetHorizontalWhitespace, RegExTester.CheatSheetHorizontalWhitespaceDetails),
            CreateCheatSheetRow(@"\H", RegExTester.CheatSheetNotHorizontalWhitespace, RegExTester.CheatSheetNotHorizontalWhitespaceDetails),
            CreateCheatSheetRow(@"\V", RegExTester.CheatSheetVerticalWhitespace, RegExTester.CheatSheetVerticalWhitespaceDetails),
            CreateCheatSheetRow(@"\V", RegExTester.CheatSheetNotVerticalWhitespace, RegExTester.CheatSheetNotVerticalWhitespaceDetails),
            CreateCheatSheetRow(@"\R", RegExTester.CheatSheetLineBreak, RegExTester.CheatSheetLineBreakDetails),
            CreateCheatSheetRow(@"\N", RegExTester.CheatSheetNotLineBreak, RegExTester.CheatSheetNotLineBreakDetails),
            CreateCheatSheetRow(@"\p{L}", RegExTester.CheatSheetUnicodeCategory, RegExTester.CheatSheetUnicodeCategoryDetails),
            CreateCheatSheetRow(@"\P{L}", RegExTester.CheatSheetNotUnicodeCategory, RegExTester.CheatSheetNotUnicodeCategoryDetails),
            CreateCheatSheetRow(@"\p{Han}", RegExTester.CheatSheetUnicodeScript, RegExTester.CheatSheetUnicodeScriptDetails),
            CreateCheatSheetRow(@"\P{Han}", RegExTester.CheatSheetNotUnicodeScript, RegExTester.CheatSheetNotUnicodeScriptDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategoryAnchors),
            CreateCheatSheetRow(@"\A", RegExTester.CheatSheetBeginningOfString, RegExTester.CheatSheetBeginningOfStringDetails),
            CreateCheatSheetRow(@"\Z", RegExTester.CheatSheetEndOfString, RegExTester.CheatSheetEndOfStringDetails),
            CreateCheatSheetRow(@"\z", RegExTester.CheatSheetStrictEndOfString, RegExTester.CheatSheetStrictEndOfStringDetails),
            CreateCheatSheetRow(@"^", RegExTester.CheatSheetBeginning, RegExTester.CheatSheetBeginningDetails),
            CreateCheatSheetRow(@"$", RegExTester.CheatSheetEnd, RegExTester.CheatSheetEndDetails),
            CreateCheatSheetRow(@"\b", RegExTester.CheatSheetWordBoundary, RegExTester.CheatSheetWordBoundaryDetails),
            CreateCheatSheetRow(@"\B", RegExTester.CheatSheetNotWordBoundary, RegExTester.CheatSheetNotWordBoundaryDetails),
            CreateCheatSheetRow(@"\G", RegExTester.CheatSheetPreviousMatchEnd, RegExTester.CheatSheetPreviousMatchEndDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategoryEscapedCharacters),
            CreateCheatSheetRow(@"\+", RegExTester.CheatSheetReservedCharacters, RegExTester.CheatSheetReservedCharactersDetails),
            CreateCheatSheetRow(@"\000", RegExTester.CheatSheetOctalEscape, RegExTester.CheatSheetOctalEscapeDetails),
            CreateCheatSheetRow(@"\xFF", RegExTester.CheatSheetHexadecimalEscape, RegExTester.CheatSheetHexadecimalEscapeDetails),
            CreateCheatSheetRow(@"\x{FF}", RegExTester.CheatSheetUnicodeEscape, RegExTester.CheatSheetUnicodeEscapeDetails),
            CreateCheatSheetRow(@"\cI", RegExTester.CheatSheetControlCharacterEscape, RegExTester.CheatSheetControlCharacterEscapeDetails),
            CreateCheatSheetRow(@"\Q...\E", RegExTester.CheatSheetEscapeSequence, RegExTester.CheatSheetEscapeSequenceDetails),
            CreateCheatSheetRow(@"\t", RegExTester.CheatSheetTab, RegExTester.CheatSheetTabDetails),
            CreateCheatSheetRow(@"\n", RegExTester.CheatSheetLineFeed, RegExTester.CheatSheetLineFeedDetails),
            CreateCheatSheetRow(@"\f", RegExTester.CheatSheetFormFeed, RegExTester.CheatSheetFormFeedDetails),
            CreateCheatSheetRow(@"\r", RegExTester.CheatSheetCarriageReturn, RegExTester.CheatSheetCarriageReturnDetails),
            CreateCheatSheetRow(@"\0", RegExTester.CheatSheetNull, RegExTester.CheatSheetNullDetails),
            CreateCheatSheetRow(@"\a", RegExTester.CheatSheetBell, RegExTester.CheatSheetBellDetails),
            CreateCheatSheetRow(@"\e", RegExTester.CheatSheetEsc, RegExTester.CheatSheetEscDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategoryGroupsReferences),
            CreateCheatSheetRow(@"(ABC)", RegExTester.CheatSheetCapturingGroup, RegExTester.CheatSheetCapturingGroupDetails),
            CreateCheatSheetRow(@"(?<name>ABC)", RegExTester.CheatSheetNamedCapturingGroup, RegExTester.CheatSheetNamedCapturingGroupDetails),
            CreateCheatSheetRow(@"\k'name'", RegExTester.CheatSheetNamedReference, RegExTester.CheatSheetNamedReferenceDetails),
            CreateCheatSheetRow(@"\1", RegExTester.CheatSheetNumericReference, RegExTester.CheatSheetNumericReferenceDetails),
            CreateCheatSheetRow(@"(?|(a)|(b))", RegExTester.CheatSheetBranchResetGroup, RegExTester.CheatSheetBranchResetGroupDetails),
            CreateCheatSheetRow(@"(?:ABC)", RegExTester.CheatSheetNonCapturingGroup, RegExTester.CheatSheetNonCapturingGroupDetails),
            CreateCheatSheetRow(@"(?>ABC)", RegExTester.CheatSheetAtomicGroup, RegExTester.CheatSheetAtomicGroupDetails),
            CreateCheatSheetRow(@"(?(DEFINE)(?'foo'ABC))", RegExTester.CheatSheetDefine, RegExTester.CheatSheetDefineDetails),
            CreateCheatSheetRow(@"\g'1'", RegExTester.CheatSheetNumericSubroutine, RegExTester.CheatSheetNumericSubroutineDetails),
            CreateCheatSheetRow(@"\g'name'", RegExTester.CheatSheetNamedSubroutine, RegExTester.CheatSheetNamedSubroutineDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategoryLookaround),
            CreateCheatSheetRow(@"(?=ABC)", RegExTester.CheatSheetPositiveLookahead, RegExTester.CheatSheetPositiveLookaheadDetails),
            CreateCheatSheetRow(@"(?!ABC)", RegExTester.CheatSheetNegativeLookahead, RegExTester.CheatSheetNegativeLookaheadDetails),
            CreateCheatSheetRow(@"(?<=ABC)", RegExTester.CheatSheetPositiveLookbehind, RegExTester.CheatSheetPositiveLookbehindDetails),
            CreateCheatSheetRow(@"(?<!ABC)", RegExTester.CheatSheetNegativeLookbehind, RegExTester.CheatSheetNegativeLookbehindDetails),
            CreateCheatSheetRow(@"\K", RegExTester.CheatSheetKeepOut, RegExTester.CheatSheetKeepOutDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategoryQuantifiersAlternation),
            CreateCheatSheetRow(@"+", RegExTester.CheatSheetPlus, RegExTester.CheatSheetPlusDetails),
            CreateCheatSheetRow(@"*", RegExTester.CheatSheetStar, RegExTester.CheatSheetStarDetails),
            CreateCheatSheetRow(@"{1,3}", RegExTester.CheatSheetQuantifier, RegExTester.CheatSheetQuantifierDetails),
            CreateCheatSheetRow(@"?", RegExTester.CheatSheetOptional, RegExTester.CheatSheetOptionalDetails),
            CreateCheatSheetRow(@"?", RegExTester.CheatSheetLazy, RegExTester.CheatSheetLazyDetails),
            CreateCheatSheetRow(@"+", RegExTester.CheatSheetPossessive, RegExTester.CheatSheetPossessiveDetails),
            CreateCheatSheetRow(@"|", RegExTester.CheatSheetAlternation, RegExTester.CheatSheetAlternationDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategorySpecial),
            CreateCheatSheetRow(@"(?#foo)", RegExTester.CheatSheetComment, RegExTester.CheatSheetCommentDetails),
            CreateCheatSheetRow(@"(?(?=A)B|C)", RegExTester.CheatSheetConditional, RegExTester.CheatSheetConditionalDetails),
            CreateCheatSheetRow(@"(?(1)B|C)", RegExTester.CheatSheetGroupConditional, RegExTester.CheatSheetGroupConditionalDetails),
            CreateCheatSheetRow(@"(?R)", RegExTester.CheatSheetRecursion, RegExTester.CheatSheetRecursionDetails),
            CreateCheatSheetRow(@"(?i)", RegExTester.CheatSheetModeModifier, RegExTester.CheatSheetModeModifierDetails),

            CreateCheatSheetTitleRow(RegExTester.CheatSheetCategorySubstitution),
            CreateCheatSheetRow(@"$0", RegExTester.CheatSheetMatch, RegExTester.CheatSheetMatchDetails),
            CreateCheatSheetRow(@"$1", RegExTester.CheatSheetCaptureGroup, RegExTester.CheatSheetCaptureGroupDetails),
            CreateCheatSheetRow(@"\n", RegExTester.CheatSheetEscapedCharacters, RegExTester.CheatSheetEscapedCharactersDetails)
        };

    private static readonly SettingDefinition<bool> allMatches
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(allMatches)}",
            defaultValue: true);

    private static readonly SettingDefinition<bool> ecmaScript
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(ecmaScript)}",
            defaultValue: false);

    private static readonly SettingDefinition<bool> cultureInvariant
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(cultureInvariant)}",
            defaultValue: false);

    private static readonly SettingDefinition<bool> ignoreCase
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(ignoreCase)}",
            defaultValue: false);

    private static readonly SettingDefinition<bool> ignoreWhitespace
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(ignoreWhitespace)}",
            defaultValue: false);

    private static readonly SettingDefinition<bool> singleline
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(singleline)}",
            defaultValue: false);

    private static readonly SettingDefinition<bool> multiline
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(multiline)}",
            defaultValue: false);

    private static readonly SettingDefinition<bool> rightToLeft
        = new(
            name: $"{nameof(RegExTesterGuiTool)}.{nameof(rightToLeft)}",
            defaultValue: false);

    private enum GridRows
    {
        TopAuto,
        Stretch,
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUISetting _ignoreWhitespaceSetting = Setting("regex-tester-ignore-whitespace-setting");
    private readonly IUISetting _singlelineSetting = Setting("regex-tester-singleline-setting");
    private readonly IUISetting _rightToLeftSetting = Setting("regex-tester-right-to-left-setting");
    private readonly IUIDataGrid _matchesDataGrid = DataGrid("regex-tester-match-information-data-grid");
    private readonly IUISingleLineTextInput _regexInput = SingleLineTextInput("regex-tester-regular-expression-input");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("regex-tester-text-input");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public RegExTesterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        OnEcmaScriptOptionChangedAsync(_settingsProvider.GetSetting(ecmaScript));
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

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

                            .WithChildren(
                                Label()
                                    .Text(RegExTester.Configuration),

                                SettingGroup("regex-tester-settings")
                                    .Icon("FluentSystemIcons", '\uF6A9')
                                    .Title(RegExTester.Options)

                                    .WithSettings(
                                        Setting("regex-tester-all-matches-setting")
                                            .Title(RegExTester.AllMatches)
                                            .Description(RegExTester.AllMatchesDescription)
                                            .Handle(
                                                _settingsProvider,
                                                allMatches,
                                                stateDescriptionWhenOn: RegExTester.AllMatches,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync),

                                        Setting("regex-tester-ecma-script-setting")
                                            .Title(RegExTester.EcmaScript)
                                            .Description(RegExTester.EcmaScriptDescription)
                                            .Handle(
                                                _settingsProvider,
                                                ecmaScript,
                                                stateDescriptionWhenOn: RegExTester.EcmaScript,
                                                stateDescriptionWhenOff: null,
                                                OnEcmaScriptOptionChangedAsync),

                                        Setting("regex-tester-culture-invariant-setting")
                                            .Title(RegExTester.CultureInvariant)
                                            .Description(RegExTester.CultureInvariantDescription)
                                            .Handle(
                                                _settingsProvider,
                                                cultureInvariant,
                                                stateDescriptionWhenOn: RegExTester.CultureInvariant,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync),

                                        Setting("regex-tester-ignore-case-setting")
                                            .Title(RegExTester.IgnoreCase)
                                            .Description(RegExTester.IgnoreCaseDescription)
                                            .Handle(
                                                _settingsProvider,
                                                ignoreCase,
                                                stateDescriptionWhenOn: RegExTester.IgnoreCase,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync),

                                        _ignoreWhitespaceSetting
                                            .Title(RegExTester.IgnoreWhitespace)
                                            .Description(RegExTester.IgnoreWhitespaceDescription)
                                            .Handle(
                                                _settingsProvider,
                                                ignoreWhitespace,
                                                stateDescriptionWhenOn: RegExTester.IgnoreWhitespace,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync),

                                        _singlelineSetting
                                            .Title(RegExTester.Singleline)
                                            .Description(RegExTester.SinglelineDescription)
                                            .Handle(
                                                _settingsProvider,
                                                singleline,
                                                stateDescriptionWhenOn: RegExTester.Singleline,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync),

                                        Setting("regex-tester-multiline-setting")
                                            .Title(RegExTester.Multiline)
                                            .Description(RegExTester.MultilineDescription)
                                            .Handle(
                                                _settingsProvider,
                                                multiline,
                                                stateDescriptionWhenOn: RegExTester.Multiline,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync),

                                        _rightToLeftSetting
                                            .Title(RegExTester.RightToLeft)
                                            .Description(RegExTester.RightToLeftDescription)
                                            .Handle(
                                                _settingsProvider,
                                                rightToLeft,
                                                stateDescriptionWhenOn: RegExTester.RightToLeft,
                                                stateDescriptionWhenOff: null,
                                                OnSettingChangedAsync)))),

                    Cell(
                        GridRows.Stretch,
                        GridColumns.Stretch,

                        SplitGrid()
                            .Vertical()
                            .LeftPaneLength(new UIGridLength(2, UIGridUnitType.Fraction))
                            .RightPaneLength(new UIGridLength(1, UIGridUnitType.Fraction))

                            .WithLeftPaneChild(
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

                                            _regexInput
                                                .Title(RegExTester.RegularExpression)
                                                .OnTextChanged(OnRegularExpressionChangedAsync)),

                                        Cell(
                                            GridRows.Stretch,
                                            GridColumns.Stretch,

                                            _inputText
                                                .Title(RegExTester.Text)
                                                .OnTextChanged(OnTextChangedAsync))))

                            .WithRightPaneChild(
                                SplitGrid()
                                    .Horizontal()

                                    .WithTopPaneChild(
                                        DataGrid()
                                            .Title(RegExTester.CheatSheetTitle)
                                            .Extendable()
                                            .WithColumns(RegExTester.CheatSheetDescription, RegExTester.CheatSheetSyntax)
                                            .WithRows(cheatSheetRows))

                                    .WithBottomPaneChild(
                                        _matchesDataGrid
                                            .Title(RegExTester.MatchInformation)
                                            .WithColumns(
                                                RegExTester.MatchName,
                                                RegExTester.MatchLocation,
                                                RegExTester.MatchValue)
                                            .OnRowSelected(OnMatchesDataGridRowSelected))))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
        {
            _inputText.Text(text); // This will trigger a regex test.
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private ValueTask OnEcmaScriptOptionChangedAsync(bool value)
    {
        if (value)
        {
            _settingsProvider.SetSetting(ignoreWhitespace, false);
            _settingsProvider.SetSetting(singleline, false);
            _settingsProvider.SetSetting(rightToLeft, false);
            _ignoreWhitespaceSetting.Disable();
            _singlelineSetting.Disable();
            _rightToLeftSetting.Disable();
        }
        else
        {
            _ignoreWhitespaceSetting.Enable();
            _singlelineSetting.Enable();
            _rightToLeftSetting.Enable();
        }

        return OnSettingChangedAsync(value);
    }

    private void OnMatchesDataGridRowSelected(IUIDataGridRow? selectedRow)
    {
        if (selectedRow is not null)
        {
            if (selectedRow.Value is Capture matchOrGroup)
            {
                _inputText.Select(matchOrGroup.Index, matchOrGroup.Length);
            }
        }
        else
        {
            _inputText.Select(0, 0);
        }
    }

    private ValueTask OnSettingChangedAsync(bool _)
    {
        StartRegExMatch();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnRegularExpressionChangedAsync(string regularExpression)
    {
        StartRegExMatch();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnTextChangedAsync(string regularExpression)
    {
        StartRegExMatch();
        return ValueTask.CompletedTask;
    }

    private void StartRegExMatch()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask
            = RegExMatchAsync(
                _regexInput.Text,
                _inputText.Text,
                _settingsProvider.GetSetting(allMatches),
                _settingsProvider.GetSetting(ecmaScript),
                _settingsProvider.GetSetting(cultureInvariant),
                _settingsProvider.GetSetting(ignoreCase),
                _settingsProvider.GetSetting(ignoreWhitespace),
                _settingsProvider.GetSetting(singleline),
                _settingsProvider.GetSetting(multiline),
                _settingsProvider.GetSetting(rightToLeft),
                _cancellationTokenSource.Token);
    }

    private async Task RegExMatchAsync(
        string regex,
        string text,
        bool allMatches,
        bool isEcmaScript,
        bool isCultureInvariant,
        bool ignoreCase,
        bool ignoreWhitespace,
        bool singleline,
        bool multiline,
        bool rightToLeft,
        CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            try
            {
                RegexOptions options
                    = GetRegexOptions(
                        isEcmaScript,
                        isCultureInvariant,
                        ignoreCase,
                        ignoreWhitespace,
                        singleline,
                        multiline,
                        rightToLeft);

                var regexObject = new Regex(regex, options);
                MatchCollection matches = regexObject.Matches(text);

                int amountOfColorsInUIHighlightedTextSpanColor = Enum.GetNames(typeof(UIHighlightedTextSpanColor)).Length;

                var highlightedSpans = new List<UIHighlightedTextSpan>();
                var gridRows = new List<IUIDataGridRow>();

                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    if (match.Length > 0)
                    {
                        highlightedSpans.Add(
                            new UIHighlightedTextSpan(
                                match.Index,
                                match.Length,
                                UIHighlightedTextSpanColor.Blue));

                        gridRows.Add(
                            Row(
                                match,
                                Cell($"{RegExTester.MatchNamePrefix} {i + 1}\t"),
                                Cell(Label().Style(UILabelStyle.Caption).NeverWrap().Text($"{match.Index}-{match.Index + match.Length}")),
                                Cell(match.Value)));

                        // Start from 1 to skip the first group which is the entire match
                        for (int j = 1; j < match.Groups.Count; j++)
                        {
                            Group group = match.Groups[j];
                            if (group.Length > 0)
                            {
                                // Skip the first two values of the enum
                                var color = (UIHighlightedTextSpanColor)(((j - 1) % (amountOfColorsInUIHighlightedTextSpanColor - 2)) + 2);

                                highlightedSpans.Add(
                                    new UIHighlightedTextSpan(
                                        group.Index,
                                        group.Length,
                                        color));

                                gridRows.Add(
                                    Row(
                                        group,
                                        Cell($"\t{RegExTester.MatchGroupPrefix} {group.Name}\t"),
                                        Cell(Label().Style(UILabelStyle.Caption).NeverWrap().Text($"{group.Index}-{group.Index + group.Length}")),
                                        Cell(group.Value)));
                            }
                        }
                    }

                    if (!allMatches)
                    {
                        break;
                    }
                }

                _inputText.Highlight(highlightedSpans.ToArray());

                _matchesDataGrid.WithRows(gridRows.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while matching regular expression.");
            }
        }
    }

    private static RegexOptions GetRegexOptions(
        bool isEcmaScript,
        bool isCultureInvariant,
        bool ignoreCase,
        bool ignoreWhitespace,
        bool singleline,
        bool multiline,
        bool rightToLeft)
    {
        RegexOptions options = RegexOptions.None;

        if (isEcmaScript)
        {
            options |= RegexOptions.ECMAScript;
        }

        if (isCultureInvariant)
        {
            options |= RegexOptions.CultureInvariant;
        }

        if (ignoreCase)
        {
            options |= RegexOptions.IgnoreCase;
        }

        if (ignoreWhitespace)
        {
            options |= RegexOptions.IgnorePatternWhitespace;
        }

        if (singleline)
        {
            options |= RegexOptions.Singleline;
        }

        if (multiline)
        {
            options |= RegexOptions.Multiline;
        }

        if (rightToLeft)
        {
            options |= RegexOptions.RightToLeft;
        }

        return options;
    }

    private static IUIDataGridRow CreateCheatSheetRow(string syntax, string description, string details)
        => Row(
            null,
            details: Label().Text(details),
            Cell(
                Label()
                    .NeverWrap()
                    .Text(description)),
            Cell(
                Label()
                    .NeverWrap()
                    .Style(UILabelStyle.BodyStrong)
                    .Text($"\t{syntax}\t")));

    private static IUIDataGridRow CreateCheatSheetTitleRow(string title)
        => Row(
            null,
            Cell(
                Label()
                    .NeverWrap()
                    .Style(UILabelStyle.Subtitle)
                    .Text(title)),
            Cell(Label()));
}
