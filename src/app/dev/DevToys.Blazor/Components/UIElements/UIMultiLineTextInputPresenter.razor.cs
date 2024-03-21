using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;
using Microsoft.VisualBasic;
using Range = DevToys.Blazor.Components.Monaco.Range;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIMultiLineTextInputPresenter : JSStyledComponentBase
{
    private readonly TaskCompletionSource _monacoInitializationAwaiter = new();
    private readonly DisposableSemaphore _semaphore = new();

    private MonacoEditor _monacoEditor = default!;
    private UITextInputWrapper _textInputWrapper = default!;
    private bool _ignoreChangeComingFromUIMultiLineTextInput;
    private bool _ignoreSelectionComingFromUIMultiLineTextInput;
    private string _modelName = string.Empty;

    [Parameter]
    public IUIMultiLineTextInput UIMultiLineTextInput { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _modelName = Id + "-text-model";
        UIMultiLineTextInput.SyntaxColorizationLanguageNameChanged += UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultiLineTextInput.TextChanged += UIMultiLineTextInput_TextChanged;
        UIMultiLineTextInput.IsReadOnlyChanged += UIMultiLineTextInput_IsReadOnlyChanged;
        UIMultiLineTextInput.SelectionChanged += UIMultiLineTextInput_SelectionChanged;
        UIMultiLineTextInput.HighlightedSpansChanged += UIMultiLineTextInput_HighlightedSpansChanged;
        UIMultiLineTextInput.HoverTooltipChanged += UIMultiLineTextInput_HoverTooltipChanged;
    }

    public override ValueTask DisposeAsync()
    {
        UIMultiLineTextInput.SyntaxColorizationLanguageNameChanged -= UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultiLineTextInput.TextChanged -= UIMultiLineTextInput_TextChanged;
        UIMultiLineTextInput.IsReadOnlyChanged -= UIMultiLineTextInput_IsReadOnlyChanged;
        UIMultiLineTextInput.SelectionChanged -= UIMultiLineTextInput_SelectionChanged;
        UIMultiLineTextInput.HighlightedSpansChanged -= UIMultiLineTextInput_HighlightedSpansChanged;
        UIMultiLineTextInput.HoverTooltipChanged -= UIMultiLineTextInput_HoverTooltipChanged;

        return base.DisposeAsync();
    }

    private async void UIMultiLineTextInput_TextChanged(object? sender, EventArgs e)
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            if (!_ignoreChangeComingFromUIMultiLineTextInput)
            {
                await _monacoInitializationAwaiter.Task;
                await _monacoEditor.SetValueAsync(UIMultiLineTextInput.Text);
            }
        }
    }

    private async void UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged(object? sender, EventArgs e)
    {
        await _monacoInitializationAwaiter.Task;
        await _monacoEditor.SetLanguageAsync(UIMultiLineTextInput.SyntaxColorizationLanguageName);
    }

    private async void UIMultiLineTextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        await _monacoInitializationAwaiter.Task;
        await _monacoEditor.UpdateOptionsAsync(new EditorUpdateOptions
        {
            ReadOnly = UIMultiLineTextInput.IsReadOnly
        });
    }

    private async void UIMultiLineTextInput_SelectionChanged(object? sender, EventArgs e)
    {
        if (!_ignoreSelectionComingFromUIMultiLineTextInput)
        {
            await _monacoInitializationAwaiter.Task;

            using (await _semaphore.WaitAsync(CancellationToken.None))
            {
                TextModel model = await _monacoEditor.GetModelAsync();

                if (model is not null)
                {
                    Position selectionStartPosition = await model.GetPositionAtAsync(JSRuntime, UIMultiLineTextInput.Selection.StartPosition);
                    Position selectionEndPosition = await model.GetPositionAtAsync(JSRuntime, UIMultiLineTextInput.Selection.EndPosition);

                    await _monacoEditor.SetSelectionAsync(new Monaco.Range
                    {
                        StartLineNumber = selectionStartPosition.LineNumber,
                        StartColumn = selectionStartPosition.Column,
                        EndLineNumber = selectionEndPosition.LineNumber,
                        EndColumn = selectionEndPosition.Column
                    });
                }
            }
        }
    }

    private async void UIMultiLineTextInput_HighlightedSpansChanged(object? sender, EventArgs e)
    {
        await _monacoInitializationAwaiter.Task;

        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            TextModel model = await _monacoEditor.GetModelAsync();

            if (model is not null)
            {
                var decorations = new ModelDeltaDecoration[UIMultiLineTextInput.HighlightedSpans.Count];

                for (int i = 0; i < decorations.Length; i++)
                {
                    UIHighlightedTextSpan highlightedSpan = UIMultiLineTextInput.HighlightedSpans[i];
                    Position selectionStartPosition = await model.GetPositionAtAsync(JSRuntime, highlightedSpan.StartPosition);
                    Position selectionEndPosition = await model.GetPositionAtAsync(JSRuntime, highlightedSpan.EndPosition);

                    decorations[i]
                        = new ModelDeltaDecoration
                        {
                            Range = new Monaco.Range
                            {
                                StartLineNumber = selectionStartPosition.LineNumber,
                                StartColumn = selectionStartPosition.Column,
                                EndLineNumber = selectionEndPosition.LineNumber,
                                EndColumn = selectionEndPosition.Column
                            },
                            Options = new ModelDecorationOptions
                            {
                                InlineClassName = GetClassNameForHighlightedTextSpan(highlightedSpan)
                            }
                        };
                }

                await _monacoEditor.ReplaceAllDecorationsByAsync(decorations);
            }
        }
    }

    private async void UIMultiLineTextInput_HoverTooltipChanged(object? sender, EventArgs e)
    {
        await _monacoInitializationAwaiter.Task;

        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            TextModel model = await _monacoEditor.GetModelAsync();

            if (model is not null)
            {
                var decorations = new ModelDeltaDecoration[UIMultiLineTextInput.HoverTooltips.Count];

                for (int i = 0; i < decorations.Length; i++)
                {
                    UIHoverTooltip hoverTooltip = UIMultiLineTextInput.HoverTooltips[i];
                    Position selectionStartPosition = await model.GetPositionAtAsync(JSRuntime, hoverTooltip.Span.StartPosition);
                    Position selectionEndPosition = await model.GetPositionAtAsync(JSRuntime, hoverTooltip.Span.EndPosition);

                    decorations[i]
                        = new ModelDeltaDecoration
                        {
                            Range = new Monaco.Range
                            {
                                StartLineNumber = selectionStartPosition.LineNumber,
                                StartColumn = selectionStartPosition.Column,
                                EndLineNumber = selectionEndPosition.LineNumber,
                                EndColumn = selectionEndPosition.Column
                            },
                            Options = new ModelDecorationOptions
                            {
                                HoverMessage = new[]
                                {
                                    new MarkdownString()
                                    {
                                        Value = hoverTooltip.Description
                                    }
                                }
                            }
                        };
                }

                await _monacoEditor.ReplaceAllDecorationsByAsync(decorations);
            }
        }
    }

    private async Task OnMonacoEditorTextChangedAsync(ModelContentChangedEvent ev)
    {
        if (ev.Changes is not null)
        {
            _ignoreChangeComingFromUIMultiLineTextInput = true;

            string documentText = await _monacoEditor.GetValueAsync(preserveBOM: null, lineEnding: null);

            UIMultiLineTextInput.Text(documentText);

            _ignoreChangeComingFromUIMultiLineTextInput = false;
        }
    }

    private async Task OnMonacoTextModelInitializationRequestedAsync()
    {
        // Get or create the text model
        TextModel textModel = await MonacoEditorHelper.GetModelAsync(JSRuntime, uri: _modelName);
        if (textModel == null)
        {
            textModel
                = await MonacoEditorHelper.CreateModelAsync(
                    JSRuntime,
                    UIMultiLineTextInput.Text,
                    language: UIMultiLineTextInput.SyntaxColorizationLanguageName,
                    uri: _modelName);

            // Set the editor model
            await _monacoEditor.SetModelAsync(textModel);
        }

        // Set the text of model
        await textModel.SetValueAsync(JSRuntime, UIMultiLineTextInput.Text);
    }

    private Task OnMonacoEditorInitializedAsync()
    {
        _monacoInitializationAwaiter.TrySetResult();

        UIMultiLineTextInput_HighlightedSpansChanged(UIMultiLineTextInput, EventArgs.Empty);
        UIMultiLineTextInput_SelectionChanged(UIMultiLineTextInput, EventArgs.Empty);

        return Task.CompletedTask;
    }

    private async Task OnMonacoEditorSelectionChanged(CursorSelectionChangedEvent newSelection)
    {
        await _monacoInitializationAwaiter.Task;
        TextModel model = await _monacoEditor.GetModelAsync();

        if (model is not null && newSelection.Selection is not null)
        {
            var selectionStartPosition
                = new Position
                {
                    LineNumber = newSelection.Selection.StartLineNumber,
                    Column = newSelection.Selection.StartColumn
                };
            var selectionEndPosition
                = new Position
                {
                    LineNumber = newSelection.Selection.EndLineNumber,
                    Column = newSelection.Selection.EndColumn
                };

            int selectionStartIndex = await model.GetOffsetAtAsync(JSRuntime, selectionStartPosition);
            int selectionEndIndex = await model.GetOffsetAtAsync(JSRuntime, selectionEndPosition);

            _ignoreSelectionComingFromUIMultiLineTextInput = true;
            try
            {
                UIMultiLineTextInput.Select(new TextSpan(selectionStartIndex, selectionEndIndex - selectionStartIndex));
            }
            finally
            {
                _ignoreSelectionComingFromUIMultiLineTextInput = false;
            }
        }
    }

    private StandaloneEditorConstructionOptions OnMonacoConstructionOptions(MonacoEditor monacoEditor)
    {
        // TODO: if language is plain text, then should we hide line numbers and few other things?
        return new StandaloneEditorConstructionOptions
        {
            ReadOnly = UIMultiLineTextInput.IsReadOnly,
            Language = UIMultiLineTextInput.SyntaxColorizationLanguageName,
            Value = UIMultiLineTextInput.Text
        };
    }

    private static string GetClassNameForHighlightedTextSpan(UIHighlightedTextSpan highlightedTextSpan)
    {
        return highlightedTextSpan.Color switch
        {
            UIHighlightedTextSpanColor.Default => "ui-multiline-text-input-highlighted-text-span-default",
            UIHighlightedTextSpanColor.Blue => "ui-multiline-text-input-highlighted-text-span-blue",
            UIHighlightedTextSpanColor.Green => "ui-multiline-text-input-highlighted-text-span-green",
            UIHighlightedTextSpanColor.Red => "ui-multiline-text-input-highlighted-text-span-red",
            UIHighlightedTextSpanColor.Yellow => "ui-multiline-text-input-highlighted-text-span-yellow",
            UIHighlightedTextSpanColor.Purple => "ui-multiline-text-input-highlighted-text-span-purple",
            UIHighlightedTextSpanColor.Teal => "ui-multiline-text-input-highlighted-text-span-teal",
            _ => throw new NotImplementedException()
        };
    }
}
