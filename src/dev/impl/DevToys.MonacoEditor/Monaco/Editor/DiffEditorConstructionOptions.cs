#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    public sealed class DiffEditorConstructionOptions : IDiffEditorConstructionOptions, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Dictionary<string, object?> _propertyBackingDictionary = new Dictionary<string, object?>();

        private T? GetPropertyValue<T>([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (_propertyBackingDictionary.TryGetValue(propertyName, out object? value))
            {
                return (T?)value;
            }

            return default;
        }

        private bool SetPropertyValue<T>(T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (EqualityComparer<T?>.Default.Equals(newValue, GetPropertyValue<T?>(propertyName)))
            {
                return false;
            }

            if (_propertyBackingDictionary.TryGetValue(propertyName, out object? value)
                && value is T typedValue
                && typedValue.Equals(newValue))
            {
                return true;
            }

            _propertyBackingDictionary[propertyName] = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Accept suggestions on provider defined characters.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("acceptSuggestionOnCommitCharacter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AcceptSuggestionOnCommitCharacter
        {
            get => GetPropertyValue<bool?>();
            set => SetPropertyValue(value);
        }

        /// <summary>
        /// Accept suggestions on ENTER.
        /// Defaults to 'on'.
        /// </summary>
        [JsonProperty("acceptSuggestionOnEnter", NullValueHandling = NullValueHandling.Ignore)]
        public AcceptSuggestionOnEnter? AcceptSuggestionOnEnter
        {
            get => GetPropertyValue<AcceptSuggestionOnEnter?>();
            set => SetPropertyValue(value);
        }

        /// <summary>
        /// Controls the number of lines in the editor that can be read out by a screen reader
        /// </summary>
        [JsonProperty("accessibilityPageSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? AccessibilityPageSize { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Configure the editor's accessibility support.
        /// Defaults to 'auto'. It is best to leave this to 'auto'.
        /// </summary>
        [JsonProperty("accessibilitySupport", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilitySupport? AccessibilitySupport { get => GetPropertyValue<AccessibilitySupport?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The aria label for the editor's textarea (when it is focused).
        /// </summary>
        [JsonProperty("ariaLabel", NullValueHandling = NullValueHandling.Ignore)]
        public string? AriaLabel { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Options for auto closing brackets.
        /// Defaults to language defined behavior.
        /// </summary>
        [JsonProperty("autoClosingBrackets", NullValueHandling = NullValueHandling.Ignore)]
        public AutoClosingBrackets? AutoClosingBrackets { get => GetPropertyValue<AutoClosingBrackets?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Options for typing over closing quotes or brackets.
        /// </summary>
        [JsonProperty("autoClosingOvertype", NullValueHandling = NullValueHandling.Ignore)]
        public AutoClosingOvertype? AutoClosingOvertype { get => GetPropertyValue<AutoClosingOvertype?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Options for auto closing quotes.
        /// Defaults to language defined behavior.
        /// </summary>
        [JsonProperty("autoClosingQuotes", NullValueHandling = NullValueHandling.Ignore)]
        public AutoClosingQuotes? AutoClosingQuotes { get => GetPropertyValue<AutoClosingQuotes?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable auto indentation adjustment.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("autoIndent", NullValueHandling = NullValueHandling.Ignore)]
        public AutoIndent? AutoIndent { get => GetPropertyValue<AutoIndent?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable that the editor will install an interval to check if its container dom node size
        /// has changed.
        /// Enabling this might have a severe performance impact.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("automaticLayout", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AutomaticLayout { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Options for auto surrounding.
        /// Defaults to always allowing auto surrounding.
        /// </summary>
        [JsonProperty("autoSurround", NullValueHandling = NullValueHandling.Ignore)]
        public AutoSurround? AutoSurround { get => GetPropertyValue<AutoSurround?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Timeout for running code actions on save.
        /// </summary>
        [JsonProperty("codeActionsOnSaveTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public int? CodeActionsOnSaveTimeout { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Show code lens
        /// Defaults to true.
        /// </summary>
        [JsonProperty("codeLens", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CodeLens { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable inline color decorators and color picker rendering.
        /// </summary>
        [JsonProperty("colorDecorators", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ColorDecorators { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the behaviour of comments in the editor.
        /// </summary>
        [JsonProperty("comments", NullValueHandling = NullValueHandling.Ignore)]
        public EditorCommentsOptions? Comments { get => GetPropertyValue<EditorCommentsOptions>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable custom contextmenu.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("contextmenu", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Contextmenu { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Syntax highlighting is copied.
        /// </summary>
        [JsonProperty("copyWithSyntaxHighlighting", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CopyWithSyntaxHighlighting { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the cursor animation style, possible values are 'blink', 'smooth', 'phase',
        /// 'expand' and 'solid'.
        /// Defaults to 'blink'.
        /// </summary>
        [JsonProperty("cursorBlinking", NullValueHandling = NullValueHandling.Ignore)]
        public CursorBlinking? CursorBlinking { get => GetPropertyValue<CursorBlinking?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable smooth caret animation.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("cursorSmoothCaretAnimation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CursorSmoothCaretAnimation { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the cursor style, either 'block' or 'line'.
        /// Defaults to 'line'.
        /// </summary>
        [JsonProperty("cursorStyle", NullValueHandling = NullValueHandling.Ignore)]
        public CursorStyle? CursorStyle { get => GetPropertyValue<CursorStyle?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls the minimal number of visible leading and trailing lines surrounding the cursor.
        /// Defaults to 0.
        /// </summary>
        [JsonProperty("cursorSurroundingLines", NullValueHandling = NullValueHandling.Ignore)]
        public int? CursorSurroundingLines { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls when `cursorSurroundingLines` should be enforced
        /// Defaults to `default`, `cursorSurroundingLines` is not enforced when cursor position is
        /// changed
        /// by mouse.
        /// </summary>
        [JsonProperty("cursorSurroundingLinesStyle", NullValueHandling = NullValueHandling.Ignore)]
        public CursorSurroundingLinesStyle? CursorSurroundingLinesStyle { get => GetPropertyValue<CursorSurroundingLinesStyle?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the width of the cursor when cursorStyle is set to 'line'
        /// </summary>
        [JsonProperty("cursorWidth", NullValueHandling = NullValueHandling.Ignore)]
        public int? CursorWidth { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Disable the use of `transform: translate3d(0px, 0px, 0px)` for the editor margin and
        /// lines layers.
        /// The usage of `transform: translate3d(0px, 0px, 0px)` acts as a hint for browsers to
        /// create an extra layer.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("disableLayerHinting", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DisableLayerHinting { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Disable the optimizations for monospace fonts.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("disableMonospaceOptimizations", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DisableMonospaceOptimizations { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls if the editor should allow to move selections via drag and drop.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("dragAndDrop", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DragAndDrop { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Copying without a selection copies the current line.
        /// </summary>
        [JsonProperty("emptySelectionClipboard", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EmptySelectionClipboard { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Class name to be added to the editor.
        /// </summary>
        [JsonProperty("extraEditorClassName", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExtraEditorClassName { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }

        /// <summary>
        /// FastScrolling mulitplier speed when pressing `Alt`
        /// Defaults to 5.
        /// </summary>
        [JsonProperty("fastScrollSensitivity", NullValueHandling = NullValueHandling.Ignore)]
        public int? FastScrollSensitivity { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the behavior of the find widget.
        /// </summary>
        [JsonProperty("find", NullValueHandling = NullValueHandling.Ignore)]
        public EditorFindOptions? Find { get => GetPropertyValue<EditorFindOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Display overflow widgets as `fixed`.
        /// Defaults to `false`.
        /// </summary>
        [JsonProperty("fixedOverflowWidgets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FixedOverflowWidgets { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable code folding
        /// Defaults to true.
        /// </summary>
        [JsonProperty("folding", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Folding { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable highlight for folded regions.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("foldingHighlight", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FoldingHighlight { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Selects the folding strategy. 'auto' uses the strategies contributed for the current
        /// document, 'indentation' uses the indentation based folding strategy.
        /// Defaults to 'auto'.
        /// </summary>
        [JsonProperty("foldingStrategy", NullValueHandling = NullValueHandling.Ignore)]
        public FoldingStrategy? FoldingStrategy { get => GetPropertyValue<FoldingStrategy?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The font family
        /// </summary>
        [JsonProperty("fontFamily", NullValueHandling = NullValueHandling.Ignore)]
        public string? FontFamily { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable font ligatures.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("fontLigatures", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FontLigatures { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The font size
        /// </summary>
        [JsonProperty("fontSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? FontSize { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The font weight
        /// </summary>
        [JsonProperty("fontWeight", NullValueHandling = NullValueHandling.Ignore)]
        public string? FontWeight { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable format on paste.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("formatOnPaste", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FormatOnPaste { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable format on type.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("formatOnType", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FormatOnType { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable the rendering of the glyph margin.
        /// Defaults to true in vscode and to false in monaco-editor.
        /// </summary>
        [JsonProperty("glyphMargin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? GlyphMargin { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Configuration options for go to location
        /// </summary>
        [JsonProperty("gotoLocation", NullValueHandling = NullValueHandling.Ignore)]
        public GoToLocationOptions? GotoLocation { get => GetPropertyValue<GoToLocationOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Should the cursor be hidden in the overview ruler.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("hideCursorInOverviewRuler", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HideCursorInOverviewRuler { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable highlighting of the active indent guide.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("highlightActiveIndentGuide", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HighlightActiveIndentGuide { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Configure the editor's hover.
        /// </summary>
        [JsonProperty("hover", NullValueHandling = NullValueHandling.Ignore)]
        public EditorHoverOptions? Hover { get => GetPropertyValue<EditorHoverOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// This editor is used inside a diff editor.
        /// </summary>
        [JsonProperty("inDiffEditor", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InDiffEditor { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The letter spacing
        /// </summary>
        [JsonProperty("letterSpacing", NullValueHandling = NullValueHandling.Ignore)]
        public int? LetterSpacing { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the behavior and rendering of the code action lightbulb.
        /// </summary>
        [JsonProperty("lightbulb", NullValueHandling = NullValueHandling.Ignore)]
        public EditorLightbulbOptions? Lightbulb { get => GetPropertyValue<EditorLightbulbOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The width reserved for line decorations (in px).
        /// Line decorations are placed between line numbers and the editor content.
        /// You can pass in a string in the format floating point followed by "ch". e.g. 1.3ch.
        /// Defaults to 10.
        /// </summary>
        [JsonProperty("lineDecorationsWidth", NullValueHandling = NullValueHandling.Ignore)]
        public uint? LineDecorationsWidth { get => GetPropertyValue<uint?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The line height
        /// </summary>
        [JsonProperty("lineHeight", NullValueHandling = NullValueHandling.Ignore)]
        public int? LineHeight { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the rendering of line numbers.
        /// If it is a function, it will be invoked when rendering a line number and the return value
        /// will be rendered.
        /// Otherwise, if it is a truey, line numbers will be rendered normally (equivalent of using
        /// an identity function).
        /// Otherwise, line numbers will not be rendered.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("lineNumbers", NullValueHandling = NullValueHandling.Ignore)]
        public LineNumbersType? LineNumbers { get => GetPropertyValue<LineNumbersType?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the width of line numbers, by reserving horizontal space for rendering at least
        /// an amount of digits.
        /// Defaults to 5.
        /// </summary>
        [JsonProperty("lineNumbersMinChars", NullValueHandling = NullValueHandling.Ignore)]
        public int? LineNumbersMinChars { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable detecting links and making them clickable.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Links { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable highlighting of matching brackets.
        /// Defaults to 'always'.
        /// </summary>
        [JsonProperty("matchBrackets", NullValueHandling = NullValueHandling.Ignore)]
        public MatchBrackets? MatchBrackets { get => GetPropertyValue<MatchBrackets?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the behavior and rendering of the minimap.
        /// </summary>
        [JsonProperty("minimap", NullValueHandling = NullValueHandling.Ignore)]
        public EditorMinimapOptions? Minimap { get => GetPropertyValue<EditorMinimapOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the mouse pointer style, either 'text' or 'default' or 'copy'
        /// Defaults to 'text'
        /// </summary>
        [JsonProperty("mouseStyle", NullValueHandling = NullValueHandling.Ignore)]
        public MouseStyle? MouseStyle { get => GetPropertyValue<MouseStyle?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// A multiplier to be used on the `deltaX` and `deltaY` of mouse wheel scroll events.
        /// Defaults to 1.
        /// </summary>
        [JsonProperty("mouseWheelScrollSensitivity", NullValueHandling = NullValueHandling.Ignore)]
        public int? MouseWheelScrollSensitivity { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Zoom the font in the editor when using the mouse wheel in combination with holding Ctrl.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("mouseWheelZoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MouseWheelZoom { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Merge overlapping selections.
        /// Defaults to true
        /// </summary>
        [JsonProperty("multiCursorMergeOverlapping", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MultiCursorMergeOverlapping { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The modifier to be used to add multiple cursors with the mouse.
        /// Defaults to 'alt'
        /// </summary>
        [JsonProperty("multiCursorModifier", NullValueHandling = NullValueHandling.Ignore)]
        public MultiCursorModifier? MultiCursorModifier { get => GetPropertyValue<MultiCursorModifier?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Configure the behaviour when pasting a text with the line count equal to the cursor
        /// count.
        /// Defaults to 'spread'.
        /// </summary>
        [JsonProperty("multiCursorPaste", NullValueHandling = NullValueHandling.Ignore)]
        public MultiCursorPaste? MultiCursorPaste { get => GetPropertyValue<MultiCursorPaste?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable semantic occurrences highlight.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("occurrencesHighlight", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OccurrencesHighlight { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls if a border should be drawn around the overview ruler.
        /// Defaults to `true`.
        /// </summary>
        [JsonProperty("overviewRulerBorder", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OverviewRulerBorder { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The number of vertical lanes the overview ruler should render.
        /// Defaults to 3.
        /// </summary>
        [JsonProperty("overviewRulerLanes", NullValueHandling = NullValueHandling.Ignore)]
        public int? OverviewRulerLanes { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Parameter hint options.
        /// </summary>
        [JsonProperty("parameterHints", NullValueHandling = NullValueHandling.Ignore)]
        public EditorParameterHintOptions? ParameterHints { get => GetPropertyValue<EditorParameterHintOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls whether to focus the inline editor in the peek widget by default.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("peekWidgetDefaultFocus", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PeekWidgetDefaultFocus { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable quick suggestions (shadow suggestions)
        /// Defaults to true.
        /// </summary>
        [JsonProperty("quickSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        public bool? QuickSuggestions { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Quick suggestions show delay (in ms)
        /// Defaults to 10 (ms)
        /// </summary>
        [JsonProperty("quickSuggestionsDelay", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuickSuggestionsDelay { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Should the editor be read only.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("readOnly", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReadOnly { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable rendering of control characters.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("renderControlCharacters", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RenderControlCharacters { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Render last line number when the file ends with a newline.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("renderFinalNewline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RenderFinalNewline { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable rendering of indent guides.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("renderIndentGuides", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RenderIndentGuides { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable rendering of current line highlight.
        /// Defaults to all.
        /// </summary>
        [JsonProperty("renderLineHighlight", NullValueHandling = NullValueHandling.Ignore)]
        public RenderLineHighlight? RenderLineHighlight { get => GetPropertyValue<RenderLineHighlight?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Should the editor render validation decorations.
        /// Defaults to editable.
        /// </summary>
        [JsonProperty("renderValidationDecorations", NullValueHandling = NullValueHandling.Ignore)]
        public string? RenderValidationDecorations { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable rendering of whitespace.
        /// Defaults to none.
        /// </summary>
        [JsonProperty("renderWhitespace", NullValueHandling = NullValueHandling.Ignore)]
        public RenderWhitespace? RenderWhitespace { get => GetPropertyValue<RenderWhitespace?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// When revealing the cursor, a virtual padding (px) is added to the cursor, turning it into
        /// a rectangle.
        /// This virtual padding ensures that the cursor gets revealed before hitting the edge of the
        /// viewport.
        /// Defaults to 30 (px).
        /// </summary>
        [JsonProperty("revealHorizontalRightPadding", NullValueHandling = NullValueHandling.Ignore)]
        public int? RevealHorizontalRightPadding { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Render the editor selection with rounded borders.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("roundedSelection", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RoundedSelection { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Render vertical lines at the specified columns.
        /// Defaults to empty array.
        /// </summary>
        [JsonProperty("rulers", NullValueHandling = NullValueHandling.Ignore)]
        public int[]? Rulers { get => GetPropertyValue<int[]>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the behavior and rendering of the scrollbars.
        /// </summary>
        [JsonProperty("scrollbar", NullValueHandling = NullValueHandling.Ignore)]
        public EditorScrollbarOptions? Scrollbar { get => GetPropertyValue<EditorScrollbarOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable that scrolling can go beyond the last column by a number of columns.
        /// Defaults to 5.
        /// </summary>
        [JsonProperty("scrollBeyondLastColumn", NullValueHandling = NullValueHandling.Ignore)]
        public int? ScrollBeyondLastColumn { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable that scrolling can go one screen size after the last line.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("scrollBeyondLastLine", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ScrollBeyondLastLine { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable Linux primary clipboard.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("selectionClipboard", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SelectionClipboard { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable selection highlight.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("selectionHighlight", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SelectionHighlight { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Should the corresponding line be selected when clicking on the line number?
        /// Defaults to true.
        /// </summary>
        [JsonProperty("selectOnLineNumbers", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SelectOnLineNumbers { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls whether the fold actions in the gutter stay always visible or hide unless the
        /// mouse is over the gutter.
        /// Defaults to 'mouseover'.
        /// </summary>
        [JsonProperty("showFoldingControls", NullValueHandling = NullValueHandling.Ignore)]
        public Show? ShowFoldingControls { get => GetPropertyValue<Show?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls fading out of unused variables.
        /// </summary>
        [JsonProperty("showUnused", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowUnused { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable that the editor animates scrolling to a position.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("smoothScrolling", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SmoothScrolling { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable snippet suggestions. Default to 'true'.
        /// </summary>
        [JsonProperty("snippetSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        public SnippetSuggestions? SnippetSuggestions { get => GetPropertyValue<SnippetSuggestions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Performance guard: Stop rendering a line after x characters.
        /// Defaults to 10000.
        /// Use -1 to never stop rendering
        /// </summary>
        [JsonProperty("stopRenderingLineAfter", NullValueHandling = NullValueHandling.Ignore)]
        public int? StopRenderingLineAfter { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Suggest options.
        /// </summary>
        [JsonProperty("suggest", NullValueHandling = NullValueHandling.Ignore)]
        public SuggestOptions? Suggest { get => GetPropertyValue<SuggestOptions?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The font size for the suggest widget.
        /// Defaults to the editor font size.
        /// </summary>
        [JsonProperty("suggestFontSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? SuggestFontSize { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The line height for the suggest widget.
        /// Defaults to the editor line height.
        /// </summary>
        [JsonProperty("suggestLineHeight", NullValueHandling = NullValueHandling.Ignore)]
        public int? SuggestLineHeight { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable the suggestion box to pop-up on trigger characters.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("suggestOnTriggerCharacters", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SuggestOnTriggerCharacters { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// The history mode for suggestions.
        /// </summary>
        [JsonProperty("suggestSelection", NullValueHandling = NullValueHandling.Ignore)]
        public SuggestSelection? SuggestSelection { get => GetPropertyValue<SuggestSelection?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Enable tab completion.
        /// </summary>
        [JsonProperty("tabCompletion", NullValueHandling = NullValueHandling.Ignore)]
        public TabCompletion? TabCompletion { get => GetPropertyValue<TabCompletion?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Inserting and deleting whitespace follows tab stops.
        /// </summary>
        [JsonProperty("useTabStops", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseTabStops { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// A string containing the word separators used when doing word navigation.
        /// Defaults to `~!@#$%^&amp;*()-=+[{]}\|;:'",.&lt;&gt;/?
        /// *
        /// </summary>
        [JsonProperty("wordSeparators", NullValueHandling = NullValueHandling.Ignore)]
        public string? WordSeparators { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the wrapping of the editor.
        /// When `wordWrap` = "off", the lines will never wrap.
        /// When `wordWrap` = "on", the lines will wrap at the viewport width.
        /// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
        /// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
        /// Defaults to "off".
        /// </summary>
        [JsonProperty("wordWrap", NullValueHandling = NullValueHandling.Ignore)]
        public WordWrap? WordWrap { get => GetPropertyValue<WordWrap?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Configure word wrapping characters. A break will be introduced after these characters.
        /// Defaults to ' \t})]?|&amp;,;'.
        /// </summary>
        [JsonProperty("wordWrapBreakAfterCharacters", NullValueHandling = NullValueHandling.Ignore)]
        public string? WordWrapBreakAfterCharacters { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Configure word wrapping characters. A break will be introduced before these characters.
        /// Defaults to '{([+'.
        /// </summary>
        [JsonProperty("wordWrapBreakBeforeCharacters", NullValueHandling = NullValueHandling.Ignore)]
        public string? WordWrapBreakBeforeCharacters { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control the wrapping of the editor.
        /// When `wordWrap` = "off", the lines will never wrap.
        /// When `wordWrap` = "on", the lines will wrap at the viewport width.
        /// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
        /// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
        /// Defaults to 80.
        /// </summary>
        [JsonProperty("wordWrapColumn", NullValueHandling = NullValueHandling.Ignore)]
        public int? WordWrapColumn { get => GetPropertyValue<int?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Force word wrapping when the text appears to be of a minified/generated file.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("wordWrapMinified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WordWrapMinified { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Control indentation of wrapped lines. Can be: 'none', 'same', 'indent' or 'deepIndent'.
        /// Defaults to 'same' in vscode and to 'none' in monaco-editor.
        /// </summary>
        [JsonProperty("wrappingIndent", NullValueHandling = NullValueHandling.Ignore)]
        public WrappingIndent? WrappingIndent { get => GetPropertyValue<WrappingIndent?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Controls the wrapping strategy to use.
        /// Defaults to 'simple'.
        /// </summary>
        [JsonProperty("wrappingStrategy", NullValueHandling = NullValueHandling.Ignore)]
        public string? WrappingStrategy { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        /// <summary>
        /// Initial theme to be used for rendering.
        /// The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black'.
        /// You can create custom themes via `monaco.editor.defineTheme`.
        /// To switch a theme, use `monaco.editor.setTheme`
        /// </summary>
        [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
        public string? Theme { get => GetPropertyValue<string?>(); set => SetPropertyValue(value); }

        [JsonProperty("enableSplitViewResizing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnableSplitViewResizing { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        [JsonProperty("ignoreTrimWhitespace", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IgnoreTrimWhitespace { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        [JsonProperty("maxComputationTime", NullValueHandling = NullValueHandling.Ignore)]
        public uint? MaxComputationTime { get => GetPropertyValue<uint?>(); set => SetPropertyValue(value); }

        [JsonProperty("originalEditable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OriginalEditable { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        [JsonProperty("renderIndicators", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RenderIndicators { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }

        [JsonProperty("renderSideBySide", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RenderSideBySide { get => GetPropertyValue<bool?>(); set => SetPropertyValue(value); }
    }

}
