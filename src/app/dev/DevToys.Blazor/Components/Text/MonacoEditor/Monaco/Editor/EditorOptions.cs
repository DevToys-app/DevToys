///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for the editor.
/// </summary>
public class EditorOptions
{
    /// <summary>
    /// This editor is used inside a diff editor.
    /// </summary>
    public bool? InDiffEditor { get; set; }

    /// <summary>
    /// The aria label for the editor's textarea (when it is focused).
    /// </summary>
    public string? AriaLabel { get; set; }

    /// <summary>
    /// The `tabindex` property of the editor's textarea
    /// </summary>
    public int? TabIndex { get; set; }

    /// <summary>
    /// Render vertical lines at the specified columns.
    /// Defaults to empty array.
    /// </summary>
    public int[]? Rulers { get; set; }

    /// <summary>
    /// A string containing the word separators used when doing word navigation.
    /// Defaults to `~!@#$%^&///()-=+[{]}\\|;:\'",.<>/?
    /// </summary>
    public string? WordSeparators { get; set; }

    /// <summary>
    /// Enable Linux primary clipboard.
    /// Defaults to true.
    /// </summary>
    public bool? SelectionClipboard { get; set; }

    /// <summary>
    /// Control the rendering of line numbers.
    /// If it is a function, it will be invoked when rendering a line number and the return value will be rendered.
    /// Otherwise, if it is a truthy, line numbers will be rendered normally (equivalent of using an identity function).
    /// Otherwise, line numbers will not be rendered.
    /// Defaults to `on`.
    /// </summary>
    public string? LineNumbers { get; set; }

    public Func<int, string>? LineNumbersLambda { get; set; }

    /// <summary>
    /// Controls the minimal number of visible leading and trailing lines surrounding the cursor.
    /// Defaults to 0.
    /// </summary>
    public int? CursorSurroundingLines { get; set; }

    /// <summary>
    /// Controls when `cursorSurroundingLines` should be enforced
    /// Defaults to `default`, `cursorSurroundingLines` is not enforced when cursor position is changed
    /// by mouse.
    /// </summary>
    public string? CursorSurroundingLinesStyle { get; set; }

    /// <summary>
    /// Render last line number when the file ends with a newline.
    /// Defaults to true.
    /// </summary>
    public bool? RenderFinalNewline { get; set; }

    /// <summary>
    /// Remove unusual line terminators like LINE SEPARATOR (LS), PARAGRAPH SEPARATOR (PS).
    /// Defaults to 'prompt'.
    /// </summary>
    public string? UnusualLineTerminators { get; set; }

    /// <summary>
    /// Should the corresponding line be selected when clicking on the line number?
    /// Defaults to true.
    /// </summary>
    public bool? SelectOnLineNumbers { get; set; }

    /// <summary>
    /// Control the width of line numbers, by reserving horizontal space for rendering at least an amount of digits.
    /// Defaults to 5.
    /// </summary>
    public int? LineNumbersMinChars { get; set; }

    /// <summary>
    /// Enable the rendering of the glyph margin.
    /// Defaults to true in vscode and to false in monaco-editor.
    /// </summary>
    public bool? GlyphMargin { get; set; }

    /// <summary>
    /// The width reserved for line decorations (in px).
    /// Line decorations are placed between line numbers and the editor content.
    /// You can pass in a string in the format floating point followed by "ch". e.g. 1.3ch.
    /// Defaults to 10.
    /// </summary>
    public int? LineDecorationsWidth { get; set; }

    public string? LineDecorationsWidthString { get; set; }

    /// <summary>
    /// When revealing the cursor, a virtual padding (px) is added to the cursor, turning it into a rectangle.
    /// This virtual padding ensures that the cursor gets revealed before hitting the edge of the viewport.
    /// Defaults to 30 (px).
    /// </summary>
    public int? RevealHorizontalRightPadding { get; set; }

    /// <summary>
    /// Render the editor selection with rounded borders.
    /// Defaults to true.
    /// </summary>
    public bool? RoundedSelection { get; set; }

    /// <summary>
    /// Class name to be added to the editor.
    /// </summary>
    public string? ExtraEditorClassName { get; set; }

    /// <summary>
    /// Should the editor be read only. See also `domReadOnly`.
    /// Defaults to false.
    /// </summary>
    public bool? ReadOnly { get; set; }

    /// <summary>
    /// Should the textarea used for input use the DOM `readonly` attribute.
    /// Defaults to false.
    /// </summary>
    public bool? DomReadOnly { get; set; }

    /// <summary>
    /// Enable linked editing.
    /// Defaults to false.
    /// </summary>
    public bool? LinkedEditing { get; set; }

    /// <summary>
    /// deprecated, use linkedEditing instead
    /// </summary>
    public bool? RenameOnType { get; set; }

    /// <summary>
    /// Should the editor render validation decorations.
    /// Defaults to editable.
    /// </summary>
    public string? RenderValidationDecorations { get; set; }

    /// <summary>
    /// Control the behavior and rendering of the scrollbars.
    /// </summary>
    public EditorScrollbarOptions? Scrollbar { get; set; }

    /// <summary>
    /// Control the behavior of experimental options
    /// </summary>
    public EditorExperimentalOptions? Experimental { get; set; }

    /// <summary>
    /// Control the behavior and rendering of the minimap.
    /// </summary>
    public EditorMinimapOptions? Minimap { get; set; }

    /// <summary>
    /// Control the behavior of the find widget.
    /// </summary>
    public EditorFindOptions? Find { get; set; }

    /// <summary>
    /// Display overflow widgets as `fixed`.
    /// Defaults to `false`.
    /// </summary>
    public bool? FixedOverflowWidgets { get; set; }

    /// <summary>
    /// The number of vertical lanes the overview ruler should render.
    /// Defaults to 3.
    /// </summary>
    public int? OverviewRulerLanes { get; set; }

    /// <summary>
    /// Controls if a border should be drawn around the overview ruler.
    /// Defaults to `true`.
    /// </summary>
    public bool? OverviewRulerBorder { get; set; }

    /// <summary>
    /// Control the cursor animation style, possible values are 'blink', 'smooth', 'phase', 'expand' and 'solid'.
    /// Defaults to 'blink'.
    /// </summary>
    public string? CursorBlinking { get; set; }

    /// <summary>
    /// Zoom the font in the editor when using the mouse wheel in combination with holding Ctrl.
    /// Defaults to false.
    /// </summary>
    public bool? MouseWheelZoom { get; set; }

    /// <summary>
    /// Control the mouse pointer style, either 'text' or 'default' or 'copy'
    /// Defaults to 'text'
    /// </summary>
    public string? MouseStyle { get; set; }

    /// <summary>
    /// Enable smooth caret animation.
    /// Defaults to false.
    /// </summary>
    public bool? CursorSmoothCaretAnimation { get; set; }

    /// <summary>
    /// Control the cursor style, either 'block' or 'line'.
    /// Defaults to 'line'.
    /// </summary>
    public string? CursorStyle { get; set; }

    /// <summary>
    /// Control the width of the cursor when cursorStyle is set to 'line'
    /// </summary>
    public int? CursorWidth { get; set; }

    /// <summary>
    /// Enable font ligatures.
    /// Defaults to false.
    /// </summary>
    public bool? FontLigatures { get; set; }

    /// <summary>
    /// Disable the use of `transform: translate3d(0px, 0px, 0px)` for the editor margin and lines layers.
    /// The usage of `transform: translate3d(0px, 0px, 0px)` acts as a hint for browsers to create an extra layer.
    /// Defaults to false.
    /// </summary>
    public bool? DisableLayerHinting { get; set; }

    /// <summary>
    /// Disable the optimizations for monospace fonts.
    /// Defaults to false.
    /// </summary>
    public bool? DisableMonospaceOptimizations { get; set; }

    /// <summary>
    /// Should the cursor be hidden in the overview ruler.
    /// Defaults to false.
    /// </summary>
    public bool? HideCursorInOverviewRuler { get; set; }

    /// <summary>
    /// Enable that scrolling can go one screen size after the last line.
    /// Defaults to true.
    /// </summary>
    public bool? ScrollBeyondLastLine { get; set; }

    /// <summary>
    /// Enable that scrolling can go beyond the last column by a number of columns.
    /// Defaults to 5.
    /// </summary>
    public int? ScrollBeyondLastColumn { get; set; }

    /// <summary>
    /// Enable that the editor animates scrolling to a position.
    /// Defaults to false.
    /// </summary>
    public bool? SmoothScrolling { get; set; }

    /// <summary>
    /// Enable that the editor will install an interval to check if its container dom node size has changed.
    /// Enabling this might have a severe performance impact.
    /// Defaults to false.
    /// </summary>
    public bool? AutomaticLayout { get; set; }

    /// <summary>
    /// Control the wrapping of the editor.
    /// When `wordWrap` = "off", the lines will never wrap.
    /// When `wordWrap` = "on", the lines will wrap at the viewport width.
    /// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
    /// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
    /// Defaults to "off".
    /// </summary>
    public string? WordWrap { get; set; }

    /// <summary>
    /// Override the `wordWrap` setting.
    /// </summary>
    public string? WordWrapOverride1 { get; set; }

    /// <summary>
    /// Override the `wordWrapOverride1` setting.
    /// </summary>
    public string? WordWrapOverride2 { get; set; }

    /// <summary>
    /// Control the wrapping of the editor.
    /// When `wordWrap` = "off", the lines will never wrap.
    /// When `wordWrap` = "on", the lines will wrap at the viewport width.
    /// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
    /// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
    /// Defaults to 80.
    /// </summary>
    public int? WordWrapColumn { get; set; }

    /// <summary>
    /// Control indentation of wrapped lines. Can be: 'none', 'same', 'indent' or 'deepIndent'.
    /// Defaults to 'same' in vscode and to 'none' in monaco-editor.
    /// </summary>
    public string? WrappingIndent { get; set; }

    /// <summary>
    /// Controls the wrapping strategy to use.
    /// Defaults to 'simple'.
    /// </summary>
    public string? WrappingStrategy { get; set; }

    /// <summary>
    /// Configure word wrapping characters. A break will be introduced before these characters.
    /// </summary>
    public string? WordWrapBreakBeforeCharacters { get; set; }

    /// <summary>
    /// Configure word wrapping characters. A break will be introduced after these characters.
    /// </summary>
    public string? WordWrapBreakAfterCharacters { get; set; }

    /// <summary>
    /// Performance guard: Stop rendering a line after x characters.
    /// Defaults to 10000.
    /// Use -1 to never stop rendering
    /// </summary>
    public int? StopRenderingLineAfter { get; set; }

    /// <summary>
    /// Configure the editor's hover.
    /// </summary>
    public EditorHoverOptions? Hover { get; set; }

    /// <summary>
    /// Enable detecting links and making them clickable.
    /// Defaults to true.
    /// </summary>
    public bool? Links { get; set; }

    /// <summary>
    /// Enable inline color decorators and color picker rendering.
    /// </summary>
    public bool? ColorDecorators { get; set; }

    /// <summary>
    /// Control the behaviour of comments in the editor.
    /// </summary>
    public EditorCommentsOptions? Comments { get; set; }

    /// <summary>
    /// Enable custom contextmenu.
    /// Defaults to true.
    /// </summary>
    public bool? Contextmenu { get; set; }

    /// <summary>
    /// A multiplier to be used on the `deltaX` and `deltaY` of mouse wheel scroll events.
    /// Defaults to 1.
    /// </summary>
    public int? MouseWheelScrollSensitivity { get; set; }

    /// <summary>
    /// FastScrolling mulitplier speed when pressing `Alt`
    /// Defaults to 5.
    /// </summary>
    public int? FastScrollSensitivity { get; set; }

    /// <summary>
    /// Enable that the editor scrolls only the predominant axis. Prevents horizontal drift when scrolling vertically on a trackpad.
    /// Defaults to true.
    /// </summary>
    public bool? ScrollPredominantAxis { get; set; }

    /// <summary>
    /// Enable that the selection with the mouse and keys is doing column selection.
    /// Defaults to false.
    /// </summary>
    public bool? ColumnSelection { get; set; }

    /// <summary>
    /// The modifier to be used to add multiple cursors with the mouse.
    /// Defaults to 'alt'
    /// </summary>
    public string? MultiCursorModifier { get; set; }

    /// <summary>
    /// Merge overlapping selections.
    /// Defaults to true
    /// </summary>
    public bool? MultiCursorMergeOverlapping { get; set; }

    /// <summary>
    /// Configure the behaviour when pasting a text with the line count equal to the cursor count.
    /// Defaults to 'spread'.
    /// </summary>
    public string? MultiCursorPaste { get; set; }

    /// <summary>
    /// Configure the editor's accessibility support.
    /// Defaults to 'auto'. It is best to leave this to 'auto'.
    /// </summary>
    public string? AccessibilitySupport { get; set; }

    /// <summary>
    /// Controls the number of lines in the editor that can be read out by a screen reader
    /// </summary>
    public int? AccessibilityPageSize { get; set; }

    /// <summary>
    /// Suggest options.
    /// </summary>
    public SuggestOptions? Suggest { get; set; }

    public InlineSuggestOptions? InlineSuggest { get; set; }

    /// <summary>
    /// Smart select options.
    /// </summary>
    public SmartSelectOptions? SmartSelect { get; set; }

    /// <summary>
    ///
    /// </summary>
    public GotoLocationOptions? GotoLocation { get; set; }

    /// <summary>
    /// Enable quick suggestions (shadow suggestions)
    /// Defaults to true.
    /// </summary>
    public QuickSuggestionsOptions? QuickSuggestions { get; set; }

    /// <summary>
    /// Quick suggestions show delay (in ms)
    /// Defaults to 10 (ms)
    /// </summary>
    public int? QuickSuggestionsDelay { get; set; }

    /// <summary>
    /// Controls the spacing around the editor.
    /// </summary>
    public EditorPaddingOptions? Padding { get; set; }

    /// <summary>
    /// Parameter hint options.
    /// </summary>
    public EditorParameterHintOptions? ParameterHints { get; set; }

    /// <summary>
    /// Options for auto closing brackets.
    /// Defaults to language defined behavior.
    /// </summary>
    public string? AutoClosingBrackets { get; set; }

    /// <summary>
    /// Options for auto closing quotes.
    /// Defaults to language defined behavior.
    /// </summary>
    public string? AutoClosingQuotes { get; set; }

    /// <summary>
    /// Options for pressing backspace near quotes or bracket pairs.
    /// </summary>
    public string? AutoClosingDelete { get; set; }

    /// <summary>
    /// Options for typing over closing quotes or brackets.
    /// </summary>
    public string? AutoClosingOvertype { get; set; }

    /// <summary>
    /// Options for auto surrounding.
    /// Defaults to always allowing auto surrounding.
    /// </summary>
    public string? AutoSurround { get; set; }

    /// <summary>
    /// Controls whether the editor should automatically adjust the indentation when users type, paste, move or indent lines.
    /// Defaults to advanced.
    /// </summary>
    public string? AutoIndent { get; set; }

    /// <summary>
    /// Emulate selection behaviour of tab characters when using spaces for indentation.
    /// This means selection will stick to tab stops.
    /// </summary>
    public bool? StickyTabStops { get; set; }

    /// <summary>
    /// Enable format on type.
    /// Defaults to false.
    /// </summary>
    public bool? FormatOnType { get; set; }

    /// <summary>
    /// Enable format on paste.
    /// Defaults to false.
    /// </summary>
    public bool? FormatOnPaste { get; set; }

    /// <summary>
    /// Controls if the editor should allow to move selections via drag and drop.
    /// Defaults to false.
    /// </summary>
    public bool? DragAndDrop { get; set; }

    /// <summary>
    /// Enable the suggestion box to pop-up on trigger characters.
    /// Defaults to true.
    /// </summary>
    public bool? SuggestOnTriggerCharacters { get; set; }

    /// <summary>
    /// Accept suggestions on ENTER.
    /// Defaults to 'on'.
    /// </summary>
    public string? AcceptSuggestionOnEnter { get; set; }

    /// <summary>
    /// Accept suggestions on provider defined characters.
    /// Defaults to true.
    /// </summary>
    public bool? AcceptSuggestionOnCommitCharacter { get; set; }

    /// <summary>
    /// Enable snippet suggestions. Default to 'true'.
    /// </summary>
    public string? SnippetSuggestions { get; set; }

    /// <summary>
    /// Copying without a selection copies the current line.
    /// </summary>
    public bool? EmptySelectionClipboard { get; set; }

    /// <summary>
    /// Syntax highlighting is copied.
    /// </summary>
    public bool? CopyWithSyntaxHighlighting { get; set; }

    /// <summary>
    /// The history mode for suggestions.
    /// </summary>
    public string? SuggestSelection { get; set; }

    /// <summary>
    /// The font size for the suggest widget.
    /// Defaults to the editor font size.
    /// </summary>
    public int? SuggestFontSize { get; set; }

    /// <summary>
    /// The line height for the suggest widget.
    /// Defaults to the editor line height.
    /// </summary>
    public int? SuggestLineHeight { get; set; }

    /// <summary>
    /// Enable tab completion.
    /// </summary>
    public string? TabCompletion { get; set; }

    /// <summary>
    /// Enable selection highlight.
    /// Defaults to true.
    /// </summary>
    public bool? SelectionHighlight { get; set; }

    /// <summary>
    /// Enable semantic occurrences highlight.
    /// Defaults to true.
    /// </summary>
    public bool? OccurrencesHighlight { get; set; }

    /// <summary>
    /// Show code lens
    /// Defaults to true.
    /// </summary>
    public bool? CodeLens { get; set; }

    /// <summary>
    /// Code lens font family. Defaults to editor font family.
    /// </summary>
    public string? CodeLensFontFamily { get; set; }

    /// <summary>
    /// Code lens font size. Default to 90% of the editor font size
    /// </summary>
    public int? CodeLensFontSize { get; set; }

    /// <summary>
    /// Control the behavior and rendering of the code action lightbulb.
    /// </summary>
    public EditorLightbulbOptions? Lightbulb { get; set; }

    /// <summary>
    /// Timeout for running code actions on save.
    /// </summary>
    public int? CodeActionsOnSaveTimeout { get; set; }

    /// <summary>
    /// Enable code folding.
    /// Defaults to true.
    /// </summary>
    public bool? Folding { get; set; }

    /// <summary>
    /// Selects the folding strategy. 'auto' uses the strategies contributed for the current document, 'indentation' uses the indentation based folding strategy.
    /// Defaults to 'auto'.
    /// </summary>
    public string? FoldingStrategy { get; set; }

    /// <summary>
    /// Enable highlight for folded regions.
    /// Defaults to true.
    /// </summary>
    public bool? FoldingHighlight { get; set; }

    /// <summary>
    /// Auto fold imports folding regions.
    /// Defaults to true.
    /// </summary>
    public bool? FoldingImportsByDefault { get; set; }

    /// <summary>
    /// Maximum number of foldable regions.
    /// Defaults to 5000.
    /// </summary>
    public int? FoldingMaximumRegions { get; set; }

    /// <summary>
    /// Controls whether the fold actions in the gutter stay always visible or hide unless the mouse is over the gutter.
    /// Defaults to 'mouseover'.
    /// </summary>
    public string? ShowFoldingControls { get; set; }

    /// <summary>
    /// Controls whether clicking on the empty content after a folded line will unfold the line.
    /// Defaults to false.
    /// </summary>
    public bool? UnfoldOnClickAfterEndOfLine { get; set; }

    /// <summary>
    /// Enable highlighting of matching brackets.
    /// Defaults to 'always'.
    /// </summary>
    public string? MatchBrackets { get; set; }

    /// <summary>
    /// Enable rendering of whitespace.
    /// Defaults to 'selection'.
    /// </summary>
    public string? RenderWhitespace { get; set; }

    /// <summary>
    /// Enable rendering of control characters.
    /// Defaults to true.
    /// </summary>
    public bool? RenderControlCharacters { get; set; }

    /// <summary>
    /// Enable rendering of current line highlight.
    /// Defaults to all.
    /// </summary>
    public string? RenderLineHighlight { get; set; }

    /// <summary>
    /// Control if the current line highlight should be rendered only the editor is focused.
    /// Defaults to false.
    /// </summary>
    public bool? RenderLineHighlightOnlyWhenFocus { get; set; }

    /// <summary>
    /// Inserting and deleting whitespace follows tab stops.
    /// </summary>
    public bool? UseTabStops { get; set; }

    /// <summary>
    /// The font family
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// The font weight
    /// </summary>
    public string? FontWeight { get; set; }

    /// <summary>
    /// The font size
    /// </summary>
    public int? FontSize { get; set; }

    /// <summary>
    /// The line height
    /// </summary>
    public int? LineHeight { get; set; }

    /// <summary>
    /// The letter spacing
    /// </summary>
    public int? LetterSpacing { get; set; }

    /// <summary>
    /// Controls fading out of unused variables.
    /// </summary>
    public bool? ShowUnused { get; set; }

    /// <summary>
    /// Controls whether to focus the inline editor in the peek widget by default.
    /// Defaults to false.
    /// </summary>
    public string? PeekWidgetDefaultFocus { get; set; }

    /// <summary>
    /// Controls whether the definition link opens element in the peek widget.
    /// Defaults to false.
    /// </summary>
    public bool? DefinitionLinkOpensInPeek { get; set; }

    /// <summary>
    /// Controls strikethrough deprecated variables.
    /// </summary>
    public bool? ShowDeprecated { get; set; }

    /// <summary>
    /// Control the behavior and rendering of the inline hints.
    /// </summary>
    public EditorInlayHintsOptions? InlayHints { get; set; }

    /// <summary>
    /// Control if the editor should use shadow DOM.
    /// </summary>
    public bool? UseShadowDOM { get; set; }

    /// <summary>
    /// Controls the behavior of editor guides.
    /// </summary>
    public GuidesOptions? Guides { get; set; }

    /// <summary>
    /// Controls the behavior of the unicode highlight feature
    /// (by default, ambiguous and invisible characters are highlighted).
    /// </summary>
    public UnicodeHighlightOptions? UnicodeHighlight { get; set; }

    /// <summary>
    /// Configures bracket pair colorization (disabled by default).
    /// </summary>
    public BracketPairColorizationOptions? BracketPairColorization { get; set; }

    /// <summary>
    /// Controls dropping into the editor from an external source.
    ///
    /// When enabled, this shows a preview of the drop location and triggers an `onDropIntoEditor` event.
    /// </summary>
    public DropIntoEditorOptions? DropIntoEditor { get; set; }
}
