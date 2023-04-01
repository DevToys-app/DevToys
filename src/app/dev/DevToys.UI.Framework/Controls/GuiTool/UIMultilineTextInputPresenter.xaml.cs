using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UIMultilineTextInputPresenter : UserControl, IDetachable
{
    private static readonly MonacoEditorPool MonacoEditorPool = new();

    private readonly IMonacoEditor _monacoEditor;

    private bool _isDetached;
    private bool _textChangedByUser;

    public UIMultilineTextInputPresenter()
    {
        this.InitializeComponent();

        Loaded += UIMultilineTextInputPresenter_Loaded;
        Unloaded += UIMultilineTextInputPresenter_Unloaded;

        _monacoEditor = MonacoEditorPool.Get();
        _monacoEditor.UIHost.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
        _monacoEditor.UIHost.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
        _monacoEditor.TextChanged += MonacoEditor_TextChanged;
        _monacoEditor.SelectedSpanChanged += MonacoEditor_SelectedSpanChanged;
        UITextInputHeader.InnerEditor = _monacoEditor.UIHost;
    }

    internal IUIMultilineLineTextInput UIMultilineLineTextInput => (IUIMultilineLineTextInput)DataContext;

    public void Detach()
    {
        // Recycling the Monaco Editor when we don't need it anymore (i.e navigating to another tool) so it can
        // be reused in another tool without having to create a new editor instance (which is slow and consumes a lot of memory).
        _isDetached = true;
        _monacoEditor.TextChanged -= MonacoEditor_TextChanged;
        _monacoEditor.SelectedSpanChanged -= MonacoEditor_SelectedSpanChanged;
        UITextInputHeader.InnerEditor = null;
        MonacoEditorPool.Recycle(_monacoEditor);
    }

    private void UIMultilineTextInputPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UIMultilineLineTextInput.IsReadOnlyChanged += UIMultilineLineTextInput_IsReadOnlyChanged;
        UIMultilineLineTextInput.TextChanged += UIMultilineLineTextInput_TextChanged;
        UIMultilineLineTextInput.SelectionChanged += UIMultilineLineTextInput_SelectionChanged;
        UIMultilineLineTextInput.SyntaxColorizationLanguageNameChanged += UIMultilineLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultilineLineTextInput.HighlightedSpansChanged += UIMultilineLineTextInput_HighlightedSpansChanged;
        _monacoEditor.ReadOnly = UIMultilineLineTextInput.IsReadOnly;
        _monacoEditor.Text = UIMultilineLineTextInput.Text;
        _monacoEditor.CodeLanguage = UIMultilineLineTextInput.SyntaxColorizationLanguageName;
        _monacoEditor.HighlightSpansAsync(UIMultilineLineTextInput.HighlightedSpans).ForgetSafely();
        if (UIMultilineLineTextInput.Selection is not null)
        {
            _monacoEditor.SelectedSpan = UIMultilineLineTextInput.Selection;
        }
    }

    private void UIMultilineTextInputPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _monacoEditor.TextChanged -= MonacoEditor_TextChanged;
        _monacoEditor.SelectedSpanChanged -= MonacoEditor_SelectedSpanChanged;
        UIMultilineLineTextInput.IsReadOnlyChanged -= UIMultilineLineTextInput_IsReadOnlyChanged;
        UIMultilineLineTextInput.TextChanged -= UIMultilineLineTextInput_TextChanged;
        UIMultilineLineTextInput.SelectionChanged -= UIMultilineLineTextInput_SelectionChanged;
        UIMultilineLineTextInput.SyntaxColorizationLanguageNameChanged -= UIMultilineLineTextInput_SyntaxColorizationLanguageNameChanged;
        Loaded -= UIMultilineTextInputPresenter_Loaded;
        Unloaded -= UIMultilineTextInputPresenter_Unloaded;
    }

    private void UIMultilineLineTextInput_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isDetached)
        {
            return;
        }

        if (UIMultilineLineTextInput.Selection is not null)
        {
            if (_monacoEditor.SelectedSpan.StartPosition != UIMultilineLineTextInput.Selection.StartPosition
                && _monacoEditor.SelectedSpan.Length != UIMultilineLineTextInput.Selection.Length)
            {
                _monacoEditor.SelectedSpan = UIMultilineLineTextInput.Selection;
            }
        }
        else
        {
            _monacoEditor.SelectedSpan = new TextSpan(0, 0);
        }
    }

    private void UIMultilineLineTextInput_TextChanged(object? sender, EventArgs e)
    {
        if (!_isDetached)
        {
            if (!_textChangedByUser)
            {
                _monacoEditor.Text = UIMultilineLineTextInput.Text;
            }
        }
    }

    private void UIMultilineLineTextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        if (!_isDetached)
        {
            _monacoEditor.ReadOnly = UIMultilineLineTextInput.IsReadOnly;
        }
    }

    private void UIMultilineLineTextInput_SyntaxColorizationLanguageNameChanged(object? sender, EventArgs e)
    {
        if (!_isDetached)
        {
            _monacoEditor.CodeLanguage = UIMultilineLineTextInput.SyntaxColorizationLanguageName;
        }
    }

    private void UIMultilineLineTextInput_HighlightedSpansChanged(object? sender, EventArgs e)
    {
        if (!_isDetached)
        {
            _monacoEditor.HighlightSpansAsync(UIMultilineLineTextInput.HighlightedSpans).ForgetSafely();
        }
    }

    private void MonacoEditor_TextChanged(object? sender, EventArgs e)
    {
        _textChangedByUser = true;
        if (!_isDetached)
        {
            UIMultilineLineTextInput.Text(_monacoEditor.Text);
        }
        _textChangedByUser = false;
    }

    private void MonacoEditor_SelectedSpanChanged(object? sender, EventArgs e)
    {
        if (!_isDetached)
        {
            if (_monacoEditor.SelectedSpan.StartPosition > UIMultilineLineTextInput.Text.Length)
            {
                // Looks like the text isn't in sync.
                _textChangedByUser = true;
                UIMultilineLineTextInput.Text(_monacoEditor.Text);
                _textChangedByUser = false;
            }

            UIMultilineLineTextInput.Select(_monacoEditor.SelectedSpan);
        }
    }
}
