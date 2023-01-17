#if __MAC__

//using DevToys.HighlightRBindingLibrary;
using System.Reflection;
using Windows.Foundation;
using DevToys.Core.Debugger;
using DevToys.MonacoEditor.HighlightJs;
using DevToys.UI.Framework.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using DispatcherQueue = Windows.UI.Core.CoreDispatcher;

namespace DevToys.MonacoEditor;

/// <summary>
/// Provides a WebView that displays the Monaco Editor.
/// </summary>
public sealed partial class CodeEditorPresenter : UserControl, ICodeEditorPresenter
{
    private readonly CodeAttributedString _textStorage;
    private readonly TextBox _textBox = new();

    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationStartingEventArgs>? NavigationStarting;
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2DOMContentLoadedEventArgs>? DOMContentLoaded;
    public event TypedEventHandler<ICodeEditorPresenter, CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
    public event AsyncTypedEventHandler<ICodeEditorPresenter, EventArgs>? DotNetObjectInjectionRequested;
    public event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? GotFocus;
    public event TypedEventHandler<ICodeEditorPresenter, RoutedEventArgs>? LostFocus;

    public CodeEditorPresenter()
    {
        _textStorage = new CodeAttributedString(this.DispatcherQueue);

        _textBox.IsSpellCheckEnabled = false;
        _textBox.AcceptsReturn = false;
        _textBox.TextWrapping = TextWrapping.WrapWholeWords;
        _textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        _textBox.VerticalAlignment = VerticalAlignment.Stretch;
        _textBox.Loaded += TextBox_Loaded;
        this.Content = _textBox;
    }

    public Task LaunchAsync()
    {
        return Task.CompletedTask;
    }

    public Task InjectDotNetObjectToWebPageAsync<T>(string name, T pObject)
    {
        return Task.CompletedTask;
    }

    public Task<string> InvokeScriptAsync(string script)
    {
        return Task.FromResult(string.Empty);
    }

    private void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        var textBoxView
            = _textBox
                .GetType()
                .GetField("_textBoxView", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(_textBox) as ITextBoxView;
        Guard.IsNotNull(textBoxView);

        textBoxView.AutocapitalizationType = UITextAutocapitalizationType.None;
        textBoxView.AutocorrectionType = UITextAutocorrectionType.No;

        _textStorage.Language = "json";

        if (textBoxView is MultilineTextBoxView multilineTextBoxView)
        {
            multilineTextBoxView.FindInteractionEnabled = true;
            multilineTextBoxView.SpellCheckingType = UITextSpellCheckingType.No;
            multilineTextBoxView.DataDetectorTypes = UIDataDetectorType.None;
            multilineTextBoxView.SmartQuotesType = UITextSmartQuotesType.No;
            multilineTextBoxView.SmartDashesType = UITextSmartDashesType.No;
            multilineTextBoxView.SmartInsertDeleteType = UITextSmartInsertDeleteType.No;
            _textStorage.AddLayoutManager(multilineTextBoxView.LayoutManager);
        }
    }
}

#endif
