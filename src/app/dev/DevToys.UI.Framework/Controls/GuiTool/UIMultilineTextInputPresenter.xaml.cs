using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UIMultilineTextInputPresenter : UserControl
{
    public UIMultilineTextInputPresenter()
    {
        this.InitializeComponent();

        Loaded += UIMultilineTextInputPresenter_Loaded;
        Unloaded += UIMultilineTextInputPresenter_Unloaded;
    }

    internal IUIMultilineLineTextInput UIMultilineLineTextInput => (IUIMultilineLineTextInput)DataContext;

    private void UIMultilineTextInputPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
    }

    private void UIMultilineTextInputPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= UIMultilineTextInputPresenter_Loaded;
        Unloaded -= UIMultilineTextInputPresenter_Unloaded;
    }
}
