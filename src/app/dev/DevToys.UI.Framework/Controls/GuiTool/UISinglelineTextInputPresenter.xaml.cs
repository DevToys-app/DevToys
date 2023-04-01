using DevToys.Api;
using DevToys.UI.Framework.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UISinglelineTextInputPresenter : UserControl
{
    private bool _textChangedByUser;

    public UISinglelineTextInputPresenter()
    {
        this.InitializeComponent();

        Loaded += UISinglelineTextInputPresenter_Loaded;
        Unloaded += UISinglelineTextInputPresenter_Unloaded;
    }

    internal IUISinglelineTextInput UISinglelineTextInput => (IUISinglelineTextInput)DataContext;

    private void UISinglelineTextInputPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UISinglelineTextInput.IsReadOnlyChanged += UISinglelineTextInput_IsReadOnlyChanged;
        UISinglelineTextInput.TextChanged += UISinglelineTextInput_TextChanged;
        UISinglelineTextInput.SelectionChanged += UISinglelineTextInput_SelectionChanged;
        TextBox.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Parts.SettingsProvider.GetSetting(PredefinedSettings.TextEditorFont));
        TextBox.IsReadOnly = UISinglelineTextInput.IsReadOnly;
        TextBox.Text = UISinglelineTextInput.Text;
        if (UISinglelineTextInput.Selection is not null)
        {
            TextBox.Select(UISinglelineTextInput.Selection.StartPosition, UISinglelineTextInput.Selection.Length);
        }

        Parts.SettingsProvider.SettingChanged += SettingsProvider_SettingChanged;
    }

    private void UISinglelineTextInputPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UISinglelineTextInput.IsReadOnlyChanged -= UISinglelineTextInput_IsReadOnlyChanged;
        UISinglelineTextInput.TextChanged -= UISinglelineTextInput_TextChanged;
        UISinglelineTextInput.SelectionChanged -= UISinglelineTextInput_SelectionChanged;
        Loaded -= UISinglelineTextInputPresenter_Loaded;
        Unloaded -= UISinglelineTextInputPresenter_Unloaded;

        Parts.SettingsProvider.SettingChanged -= SettingsProvider_SettingChanged;
    }

    private void UISinglelineTextInput_SelectionChanged(object? sender, EventArgs e)
    {
        if (UISinglelineTextInput.Selection is not null)
        {
            if (TextBox.SelectionStart != UISinglelineTextInput.Selection.StartPosition
                && TextBox.SelectionLength != UISinglelineTextInput.Selection.Length)
            {
                TextBox.Select(UISinglelineTextInput.Selection.StartPosition, UISinglelineTextInput.Selection.Length);
            }
        }
        else
        {
            TextBox.Select(0, 0);
        }
    }

    private void UISinglelineTextInput_TextChanged(object? sender, EventArgs e)
    {
        if (!_textChangedByUser)
        {
            TextBox.Text = UISinglelineTextInput.Text;
        }
    }

    private void UISinglelineTextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        TextBox.IsReadOnly = UISinglelineTextInput.IsReadOnly;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _textChangedByUser = true;
        UISinglelineTextInput.Text(TextBox.Text);
        _textChangedByUser = false;
    }

    private void TextBox_SelectionChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (TextBox.SelectionStart > UISinglelineTextInput.Text.Length)
        {
            // Looks like the text isn't in sync.
            _textChangedByUser = true;
            UISinglelineTextInput.Text(TextBox.Text);
            _textChangedByUser = false;
        }

        UISinglelineTextInput.Select(TextBox.SelectionStart, TextBox.SelectionLength);
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (string.Equals(e.SettingName, PredefinedSettings.TextEditorFont.Name, StringComparison.Ordinal))
        {
            TextBox.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(e.NewValue as string);
        }
    }
}
