#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configuration options for the editor.
    /// </summary>
    public interface IEditorOptions
    {
        /// <summary>
        /// Accept suggestions on provider defined characters.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("acceptSuggestionOnCommitCharacter", NullValueHandling = NullValueHandling.Ignore)]
        bool? AcceptSuggestionOnCommitCharacter { get; set; }

        /// <summary>
        /// Accept suggestions on ENTER.
        /// Defaults to 'on'.
        /// </summary>
        [JsonProperty("acceptSuggestionOnEnter", NullValueHandling = NullValueHandling.Ignore)]
        AcceptSuggestionOnEnter? AcceptSuggestionOnEnter { get; set; }

        /// <summary>
        /// Controls the number of lines in the editor that can be read out by a screen reader
        /// </summary>
        [JsonProperty("accessibilityPageSize", NullValueHandling = NullValueHandling.Ignore)]
        int? AccessibilityPageSize { get; set; }

        /// <summary>
        /// Configure the editor's accessibility support.
        /// Defaults to 'auto'. It is best to leave this to 'auto'.
        /// </summary>
        [JsonProperty("accessibilitySupport", NullValueHandling = NullValueHandling.Ignore)]
        AccessibilitySupport? AccessibilitySupport { get; set; }

        /// <summary>
        /// The aria label for the editor's textarea (when it is focused).
        /// </summary>
        [JsonProperty("ariaLabel", NullValueHandling = NullValueHandling.Ignore)]
        string? AriaLabel { get; set; }

        /// <summary>
        /// Options for auto closing brackets.
        /// Defaults to language defined behavior.
        /// </summary>
        [JsonProperty("autoClosingBrackets", NullValueHandling = NullValueHandling.Ignore)]
        AutoClosingBrackets? AutoClosingBrackets { get; set; }

        /// <summary>
        /// Options for typing over closing quotes or brackets.
        /// </summary>
        [JsonProperty("autoClosingOvertype", NullValueHandling = NullValueHandling.Ignore)]
        AutoClosingOvertype? AutoClosingOvertype { get; set; }

        /// <summary>
        /// Options for auto closing quotes.
        /// Defaults to language defined behavior.
        /// </summary>
        [JsonProperty("autoClosingQuotes", NullValueHandling = NullValueHandling.Ignore)]
        AutoClosingQuotes? AutoClosingQuotes { get; set; }

        /// <summary>
        /// Controls whether the editor should automatically adjust the indentation when users type, paste, move or indent lines.
        /// Defaults to advanced.
        /// </summary>
        [JsonProperty("autoIndent", NullValueHandling = NullValueHandling.Ignore)]
        AutoIndent? AutoIndent { get; set; }

        /// <summary>
        /// Enable that the editor will install an interval to check if its container dom node size
        /// has changed.
        /// Enabling this might have a severe performance impact.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("automaticLayout", NullValueHandling = NullValueHandling.Ignore)]
        bool? AutomaticLayout { get; set; }

        /// <summary>
        /// Options for auto surrounding.
        /// Defaults to always allowing auto surrounding.
        /// </summary>
        [JsonProperty("autoSurround", NullValueHandling = NullValueHandling.Ignore)]
        AutoSurround? AutoSurround { get; set; }

        /// <summary>
        /// Timeout for running code actions on save.
        /// </summary>
        [JsonProperty("codeActionsOnSaveTimeout", NullValueHandling = NullValueHandling.Ignore)]
        int? CodeActionsOnSaveTimeout { get; set; }

        /// <summary>
        /// Show code lens
        /// Defaults to true.
        /// </summary>
        [JsonProperty("codeLens", NullValueHandling = NullValueHandling.Ignore)]
        bool? CodeLens { get; set; }

        /// <summary>
        /// Enable inline color decorators and color picker rendering.
        /// </summary>
        [JsonProperty("colorDecorators", NullValueHandling = NullValueHandling.Ignore)]
        bool? ColorDecorators { get; set; }

        /// <summary>
        /// Control the behaviour of comments in the editor.
        /// </summary>
        [JsonProperty("comments", NullValueHandling = NullValueHandling.Ignore)]
        EditorCommentsOptions? Comments { get; set; }
        /// <summary>
        /// Enable custom contextmenu.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("contextmenu", NullValueHandling = NullValueHandling.Ignore)]
        bool? Contextmenu { get; set; }

        /// <summary>
        /// Syntax highlighting is copied.
        /// </summary>
        [JsonProperty("copyWithSyntaxHighlighting", NullValueHandling = NullValueHandling.Ignore)]
        bool? CopyWithSyntaxHighlighting { get; set; }

        /// <summary>
        /// Control the cursor animation style, possible values are 'blink', 'smooth', 'phase',
        /// 'expand' and 'solid'.
        /// Defaults to 'blink'.
        /// </summary>
        [JsonProperty("cursorBlinking", NullValueHandling = NullValueHandling.Ignore)]
        CursorBlinking? CursorBlinking { get; set; }

        /// <summary>
        /// Enable smooth caret animation.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("cursorSmoothCaretAnimation", NullValueHandling = NullValueHandling.Ignore)]
        bool? CursorSmoothCaretAnimation { get; set; }

        /// <summary>
        /// Control the cursor style, either 'block' or 'line'.
        /// Defaults to 'line'.
        /// </summary>
        [JsonProperty("cursorStyle", NullValueHandling = NullValueHandling.Ignore)]
        CursorStyle? CursorStyle { get; set; }

        /// <summary>
        /// Controls the minimal number of visible leading and trailing lines surrounding the cursor.
        /// Defaults to 0.
        /// </summary>
        [JsonProperty("cursorSurroundingLines", NullValueHandling = NullValueHandling.Ignore)]
        int? CursorSurroundingLines { get; set; }

        /// <summary>
        /// Controls when `cursorSurroundingLines` should be enforced
        /// Defaults to `default`, `cursorSurroundingLines` is not enforced when cursor position is
        /// changed
        /// by mouse.
        /// </summary>
        [JsonProperty("cursorSurroundingLinesStyle", NullValueHandling = NullValueHandling.Ignore)]
        CursorSurroundingLinesStyle? CursorSurroundingLinesStyle { get; set; }

        /// <summary>
        /// Control the width of the cursor when cursorStyle is set to 'line'
        /// </summary>
        [JsonProperty("cursorWidth", NullValueHandling = NullValueHandling.Ignore)]
        int? CursorWidth { get; set; }

        /// <summary>
        /// Disable the use of `transform: translate3d(0px, 0px, 0px)` for the editor margin and
        /// lines layers.
        /// The usage of `transform: translate3d(0px, 0px, 0px)` acts as a hint for browsers to
        /// create an extra layer.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("disableLayerHinting", NullValueHandling = NullValueHandling.Ignore)]
        bool? DisableLayerHinting { get; set; }

        /// <summary>
        /// Disable the optimizations for monospace fonts.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("disableMonospaceOptimizations", NullValueHandling = NullValueHandling.Ignore)]
        bool? DisableMonospaceOptimizations { get; set; }

        /// <summary>
        /// Controls if the editor should allow to move selections via drag and drop.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("dragAndDrop", NullValueHandling = NullValueHandling.Ignore)]
        bool? DragAndDrop { get; set; }

        /// <summary>
        /// Copying without a selection copies the current line.
        /// </summary>
        [JsonProperty("emptySelectionClipboard", NullValueHandling = NullValueHandling.Ignore)]
        bool? EmptySelectionClipboard { get; set; }

        /// <summary>
        /// Class name to be added to the editor.
        /// </summary>
        [JsonProperty("extraEditorClassName", NullValueHandling = NullValueHandling.Ignore)]
        string? ExtraEditorClassName { get; set; }

        /// <summary>
        /// FastScrolling mulitplier speed when pressing `Alt`
        /// Defaults to 5.
        /// </summary>
        [JsonProperty("fastScrollSensitivity", NullValueHandling = NullValueHandling.Ignore)]
        int? FastScrollSensitivity { get; set; }

        /// <summary>
        /// Control the behavior of the find widget.
        /// </summary>
        [JsonProperty("find", NullValueHandling = NullValueHandling.Ignore)]
        EditorFindOptions? Find { get; set; }

        /// <summary>
        /// Display overflow widgets as `fixed`.
        /// Defaults to `false`.
        /// </summary>
        [JsonProperty("fixedOverflowWidgets", NullValueHandling = NullValueHandling.Ignore)]
        bool? FixedOverflowWidgets { get; set; }

        /// <summary>
        /// Enable code folding
        /// Defaults to true.
        /// </summary>
        [JsonProperty("folding", NullValueHandling = NullValueHandling.Ignore)]
        bool? Folding { get; set; }

        /// <summary>
        /// Enable highlight for folded regions.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("foldingHighlight", NullValueHandling = NullValueHandling.Ignore)]
        bool? FoldingHighlight { get; set; }

        /// <summary>
        /// Selects the folding strategy. 'auto' uses the strategies contributed for the current
        /// document, 'indentation' uses the indentation based folding strategy.
        /// Defaults to 'auto'.
        /// </summary>
        [JsonProperty("foldingStrategy", NullValueHandling = NullValueHandling.Ignore)]
        FoldingStrategy? FoldingStrategy { get; set; }

        /// <summary>
        /// The font family
        /// </summary>
        [JsonProperty("fontFamily", NullValueHandling = NullValueHandling.Ignore)]
        string? FontFamily { get; set; }

        /// <summary>
        /// Enable font ligatures.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("fontLigatures", NullValueHandling = NullValueHandling.Ignore)]
        bool? FontLigatures { get; set; }

        /// <summary>
        /// The font size
        /// </summary>
        [JsonProperty("fontSize", NullValueHandling = NullValueHandling.Ignore)]
        int? FontSize { get; set; }

        /// <summary>
        /// The font weight
        /// </summary>
        [JsonProperty("fontWeight", NullValueHandling = NullValueHandling.Ignore)]
        string? FontWeight { get; set; }

        /// <summary>
        /// Enable format on paste.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("formatOnPaste", NullValueHandling = NullValueHandling.Ignore)]
        bool? FormatOnPaste { get; set; }

        /// <summary>
        /// Enable format on type.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("formatOnType", NullValueHandling = NullValueHandling.Ignore)]
        bool? FormatOnType { get; set; }

        /// <summary>
        /// Enable the rendering of the glyph margin.
        /// Defaults to true in vscode and to false in monaco-editor.
        /// </summary>
        [JsonProperty("glyphMargin", NullValueHandling = NullValueHandling.Ignore)]
        bool? GlyphMargin { get; set; }

        /// <summary>
        /// Configuration options for go to location
        /// </summary>
        [JsonProperty("gotoLocation", NullValueHandling = NullValueHandling.Ignore)]
        GoToLocationOptions? GotoLocation { get; set; }

        /// <summary>
        /// Should the cursor be hidden in the overview ruler.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("hideCursorInOverviewRuler", NullValueHandling = NullValueHandling.Ignore)]
        bool? HideCursorInOverviewRuler { get; set; }

        /// <summary>
        /// Enable highlighting of the active indent guide.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("highlightActiveIndentGuide", NullValueHandling = NullValueHandling.Ignore)]
        bool? HighlightActiveIndentGuide { get; set; }

        /// <summary>
        /// Configure the editor's hover.
        /// </summary>
        [JsonProperty("hover", NullValueHandling = NullValueHandling.Ignore)]
        EditorHoverOptions? Hover { get; set; }

        /// <summary>
        /// This editor is used inside a diff editor.
        /// </summary>
        [JsonProperty("inDiffEditor", NullValueHandling = NullValueHandling.Ignore)]
        bool? InDiffEditor { get; set; }

        /// <summary>
        /// The letter spacing
        /// </summary>
        [JsonProperty("letterSpacing", NullValueHandling = NullValueHandling.Ignore)]
        int? LetterSpacing { get; set; }

        /// <summary>
        /// Control the behavior and rendering of the code action lightbulb.
        /// </summary>
        [JsonProperty("lightbulb", NullValueHandling = NullValueHandling.Ignore)]
        EditorLightbulbOptions? Lightbulb { get; set; }

        /// <summary>
        /// The width reserved for line decorations (in px).
        /// Line decorations are placed between line numbers and the editor content.
        /// You can pass in a string in the format floating point followed by "ch". e.g. 1.3ch.
        /// Defaults to 10.
        /// </summary>
        [JsonProperty("lineDecorationsWidth", NullValueHandling = NullValueHandling.Ignore)]
        uint? LineDecorationsWidth { get; set; }

        /// <summary>
        /// The line height
        /// </summary>
        [JsonProperty("lineHeight", NullValueHandling = NullValueHandling.Ignore)]
        int? LineHeight { get; set; }

        /// <summary>
        /// Control the rendering of line numbers.
        /// If it is a function, it will be invoked when rendering a line number and the return value
        /// will be rendered.
        /// Otherwise, if it is a truey, line numbers will be rendered normally (equivalent of using
        /// an identity function).
        /// Otherwise, line numbers will not be rendered.
        /// Defaults to `on`.
        /// </summary>
        [JsonProperty("lineNumbers", NullValueHandling = NullValueHandling.Ignore)]
        LineNumbersType? LineNumbers { get; set; }

        /// <summary>
        /// Control the width of line numbers, by reserving horizontal space for rendering at least
        /// an amount of digits.
        /// Defaults to 5.
        /// </summary>
        [JsonProperty("lineNumbersMinChars", NullValueHandling = NullValueHandling.Ignore)]
        int? LineNumbersMinChars { get; set; }

        /// <summary>
        /// Enable detecting links and making them clickable.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        bool? Links { get; set; }

        /// <summary>
        /// Enable highlighting of matching brackets.
        /// Defaults to 'always'.
        /// </summary>
        [JsonProperty("matchBrackets", NullValueHandling = NullValueHandling.Ignore)]
        MatchBrackets? MatchBrackets { get; set; }

        /// <summary>
        /// Control the behavior and rendering of the minimap.
        /// </summary>
        [JsonProperty("minimap", NullValueHandling = NullValueHandling.Ignore)]
        EditorMinimapOptions? Minimap { get; set; }

        /// <summary>
        /// Control the mouse pointer style, either 'text' or 'default' or 'copy'
        /// Defaults to 'text'
        /// </summary>
        [JsonProperty("mouseStyle", NullValueHandling = NullValueHandling.Ignore)]
        MouseStyle? MouseStyle { get; set; }

        /// <summary>
        /// A multiplier to be used on the `deltaX` and `deltaY` of mouse wheel scroll events.
        /// Defaults to 1.
        /// </summary>
        [JsonProperty("mouseWheelScrollSensitivity", NullValueHandling = NullValueHandling.Ignore)]
        int? MouseWheelScrollSensitivity { get; set; }

        /// <summary>
        /// Zoom the font in the editor when using the mouse wheel in combination with holding Ctrl.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("mouseWheelZoom", NullValueHandling = NullValueHandling.Ignore)]
        bool? MouseWheelZoom { get; set; }

        /// <summary>
        /// Merge overlapping selections.
        /// Defaults to true
        /// </summary>
        [JsonProperty("multiCursorMergeOverlapping", NullValueHandling = NullValueHandling.Ignore)]
        bool? MultiCursorMergeOverlapping { get; set; }

        /// <summary>
        /// The modifier to be used to add multiple cursors with the mouse.
        /// Defaults to 'alt'
        /// </summary>
        [JsonProperty("multiCursorModifier", NullValueHandling = NullValueHandling.Ignore)]
        MultiCursorModifier? MultiCursorModifier { get; set; }

        /// <summary>
        /// Configure the behaviour when pasting a text with the line count equal to the cursor
        /// count.
        /// Defaults to 'spread'.
        /// </summary>
        [JsonProperty("multiCursorPaste", NullValueHandling = NullValueHandling.Ignore)]
        MultiCursorPaste? MultiCursorPaste { get; set; }

        /// <summary>
        /// Enable semantic occurrences highlight.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("occurrencesHighlight", NullValueHandling = NullValueHandling.Ignore)]
        bool? OccurrencesHighlight { get; set; }

        /// <summary>
        /// Controls if a border should be drawn around the overview ruler.
        /// Defaults to `true`.
        /// </summary>
        [JsonProperty("overviewRulerBorder", NullValueHandling = NullValueHandling.Ignore)]
        bool? OverviewRulerBorder { get; set; }

        /// <summary>
        /// The number of vertical lanes the overview ruler should render.
        /// Defaults to 3.
        /// </summary>
        [JsonProperty("overviewRulerLanes", NullValueHandling = NullValueHandling.Ignore)]
        int? OverviewRulerLanes { get; set; }

        /// <summary>
        /// Parameter hint options.
        /// </summary>
        [JsonProperty("parameterHints", NullValueHandling = NullValueHandling.Ignore)]
        EditorParameterHintOptions? ParameterHints { get; set; }

        /// <summary>
        /// Controls whether to focus the inline editor in the peek widget by default.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("peekWidgetDefaultFocus", NullValueHandling = NullValueHandling.Ignore)]
        bool? PeekWidgetDefaultFocus { get; set; }

        /// <summary>
        /// Enable quick suggestions (shadow suggestions)
        /// Defaults to true.
        /// </summary>
        [JsonProperty("quickSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        bool? QuickSuggestions { get; set; }

        /// <summary>
        /// Quick suggestions show delay (in ms)
        /// Defaults to 10 (ms)
        /// </summary>
        [JsonProperty("quickSuggestionsDelay", NullValueHandling = NullValueHandling.Ignore)]
        int? QuickSuggestionsDelay { get; set; }

        /// <summary>
        /// Should the editor be read only.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("readOnly", NullValueHandling = NullValueHandling.Ignore)]
        bool? ReadOnly { get; set; }

        /// <summary>
        /// Enable rendering of control characters.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("renderControlCharacters", NullValueHandling = NullValueHandling.Ignore)]
        bool? RenderControlCharacters { get; set; }

        /// <summary>
        /// Render last line number when the file ends with a newline.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("renderFinalNewline", NullValueHandling = NullValueHandling.Ignore)]
        bool? RenderFinalNewline { get; set; }

        /// <summary>
        /// Enable rendering of indent guides.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("renderIndentGuides", NullValueHandling = NullValueHandling.Ignore)]
        bool? RenderIndentGuides { get; set; }

        /// <summary>
        /// Enable rendering of current line highlight.
        /// Defaults to all.
        /// </summary>
        [JsonProperty("renderLineHighlight", NullValueHandling = NullValueHandling.Ignore)]
        RenderLineHighlight? RenderLineHighlight { get; set; }

        /// <summary>
        /// Should the editor render validation decorations.
        /// Defaults to editable.
        /// </summary>
        [JsonProperty("renderValidationDecorations", NullValueHandling = NullValueHandling.Ignore)]
        string? RenderValidationDecorations { get; set; }

        /// <summary>
        /// Enable rendering of whitespace.
        /// Defaults to none.
        /// </summary>
        [JsonProperty("renderWhitespace", NullValueHandling = NullValueHandling.Ignore)]
        RenderWhitespace? RenderWhitespace { get; set; }

        /// <summary>
        /// When revealing the cursor, a virtual padding (px) is added to the cursor, turning it into
        /// a rectangle.
        /// This virtual padding ensures that the cursor gets revealed before hitting the edge of the
        /// viewport.
        /// Defaults to 30 (px).
        /// </summary>
        [JsonProperty("revealHorizontalRightPadding", NullValueHandling = NullValueHandling.Ignore)]
        int? RevealHorizontalRightPadding { get; set; }

        /// <summary>
        /// Render the editor selection with rounded borders.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("roundedSelection", NullValueHandling = NullValueHandling.Ignore)]
        bool? RoundedSelection { get; set; }

        /// <summary>
        /// Render vertical lines at the specified columns.
        /// Defaults to empty array.
        /// </summary>
        [JsonProperty("rulers", NullValueHandling = NullValueHandling.Ignore)]
        int[]? Rulers { get; set; }

        /// <summary>
        /// Control the behavior and rendering of the scrollbars.
        /// </summary>
        [JsonProperty("scrollbar", NullValueHandling = NullValueHandling.Ignore)]
        EditorScrollbarOptions? Scrollbar { get; set; }

        /// <summary>
        /// Enable that scrolling can go beyond the last column by a number of columns.
        /// Defaults to 5.
        /// </summary>
        [JsonProperty("scrollBeyondLastColumn", NullValueHandling = NullValueHandling.Ignore)]
        int? ScrollBeyondLastColumn { get; set; }

        /// <summary>
        /// Enable that scrolling can go one screen size after the last line.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("scrollBeyondLastLine", NullValueHandling = NullValueHandling.Ignore)]
        bool? ScrollBeyondLastLine { get; set; }

        /// <summary>
        /// Enable Linux primary clipboard.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("selectionClipboard", NullValueHandling = NullValueHandling.Ignore)]
        bool? SelectionClipboard { get; set; }

        /// <summary>
        /// Enable selection highlight.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("selectionHighlight", NullValueHandling = NullValueHandling.Ignore)]
        bool? SelectionHighlight { get; set; }

        /// <summary>
        /// Should the corresponding line be selected when clicking on the line number?
        /// Defaults to true.
        /// </summary>
        [JsonProperty("selectOnLineNumbers", NullValueHandling = NullValueHandling.Ignore)]
        bool? SelectOnLineNumbers { get; set; }

        /// <summary>
        /// Controls whether the fold actions in the gutter stay always visible or hide unless the
        /// mouse is over the gutter.
        /// Defaults to 'mouseover'.
        /// </summary>
        [JsonProperty("showFoldingControls", NullValueHandling = NullValueHandling.Ignore)]
        Show? ShowFoldingControls { get; set; }

        /// <summary>
        /// Controls fading out of unused variables.
        /// </summary>
        [JsonProperty("showUnused", NullValueHandling = NullValueHandling.Ignore)]
        bool? ShowUnused { get; set; }

        /// <summary>
        /// Enable that the editor animates scrolling to a position.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("smoothScrolling", NullValueHandling = NullValueHandling.Ignore)]
        bool? SmoothScrolling { get; set; }

        /// <summary>
        /// Enable snippet suggestions. Default to 'true'.
        /// </summary>
        [JsonProperty("snippetSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        SnippetSuggestions? SnippetSuggestions { get; set; }

        /// <summary>
        /// Performance guard: Stop rendering a line after x characters.
        /// Defaults to 10000.
        /// Use -1 to never stop rendering
        /// </summary>
        [JsonProperty("stopRenderingLineAfter", NullValueHandling = NullValueHandling.Ignore)]
        int? StopRenderingLineAfter { get; set; }

        /// <summary>
        /// Suggest options.
        /// </summary>
        [JsonProperty("suggest", NullValueHandling = NullValueHandling.Ignore)]
        SuggestOptions? Suggest { get; set; }

        /// <summary>
        /// The font size for the suggest widget.
        /// Defaults to the editor font size.
        /// </summary>
        [JsonProperty("suggestFontSize", NullValueHandling = NullValueHandling.Ignore)]
        int? SuggestFontSize { get; set; }

        /// <summary>
        /// The line height for the suggest widget.
        /// Defaults to the editor line height.
        /// </summary>
        [JsonProperty("suggestLineHeight", NullValueHandling = NullValueHandling.Ignore)]
        int? SuggestLineHeight { get; set; }

        /// <summary>
        /// Enable the suggestion box to pop-up on trigger characters.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("suggestOnTriggerCharacters", NullValueHandling = NullValueHandling.Ignore)]
        bool? SuggestOnTriggerCharacters { get; set; }

        /// <summary>
        /// The history mode for suggestions.
        /// </summary>
        [JsonProperty("suggestSelection", NullValueHandling = NullValueHandling.Ignore)]
        SuggestSelection? SuggestSelection { get; set; }

        /// <summary>
        /// Enable tab completion.
        /// </summary>
        [JsonProperty("tabCompletion", NullValueHandling = NullValueHandling.Ignore)]
        TabCompletion? TabCompletion { get; set; }

        /// <summary>
        /// Inserting and deleting whitespace follows tab stops.
        /// </summary>
        [JsonProperty("useTabStops", NullValueHandling = NullValueHandling.Ignore)]
        bool? UseTabStops { get; set; }

        /// <summary>
        /// A string containing the word separators used when doing word navigation.
        /// Defaults to `~!@#$%^&amp;*()-=+[{]}\|;:'",.&lt;&gt;/?
        /// *
        /// </summary>
        [JsonProperty("wordSeparators", NullValueHandling = NullValueHandling.Ignore)]
        string? WordSeparators { get; set; }

        /// <summary>
        /// Control the wrapping of the editor.
        /// When `wordWrap` = "off", the lines will never wrap.
        /// When `wordWrap` = "on", the lines will wrap at the viewport width.
        /// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
        /// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
        /// Defaults to "off".
        /// </summary>
        [JsonProperty("wordWrap", NullValueHandling = NullValueHandling.Ignore)]
        WordWrap? WordWrap { get; set; }

        /// <summary>
        /// Configure word wrapping characters. A break will be introduced after these characters.
        /// Defaults to ' \t})]?|/&amp;.,;¢°′″‰℃、。｡､￠，．：；？！％・･ゝゞヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻ｧｨｩｪｫｬｭｮｯｰ”〉》」』】〕）］｝｣'.
        /// </summary>
        [JsonProperty("wordWrapBreakAfterCharacters", NullValueHandling = NullValueHandling.Ignore)]
        string? WordWrapBreakAfterCharacters { get; set; }

        /// <summary>
        /// Configure word wrapping characters. A break will be introduced before these characters.
        /// Defaults to '([{‘“〈《「『【〔（［｛｢£¥＄￡￥+＋'.
        /// </summary>
        [JsonProperty("wordWrapBreakBeforeCharacters", NullValueHandling = NullValueHandling.Ignore)]
        string? WordWrapBreakBeforeCharacters { get; set; }

        /// <summary>
        /// Control the wrapping of the editor.
        /// When `wordWrap` = "off", the lines will never wrap.
        /// When `wordWrap` = "on", the lines will wrap at the viewport width.
        /// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
        /// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
        /// Defaults to 80.
        /// </summary>
        [JsonProperty("wordWrapColumn", NullValueHandling = NullValueHandling.Ignore)]
        int? WordWrapColumn { get; set; }

        /// <summary>
        /// Force word wrapping when the text appears to be of a minified/generated file.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("wordWrapMinified", NullValueHandling = NullValueHandling.Ignore)]
        bool? WordWrapMinified { get; set; }

        /// <summary>
        /// Control indentation of wrapped lines. Can be: 'none', 'same', 'indent' or 'deepIndent'.
        /// Defaults to 'same' in vscode and to 'none' in monaco-editor.
        /// </summary>
        [JsonProperty("wrappingIndent", NullValueHandling = NullValueHandling.Ignore)]
        WrappingIndent? WrappingIndent { get; set; }

        /// <summary>
        /// Controls the wrapping strategy to use.
        /// Defaults to 'simple'.
        /// </summary>
        [JsonProperty("wrappingStrategy", NullValueHandling = NullValueHandling.Ignore)]
        string? WrappingStrategy { get; set; }
    }

}
