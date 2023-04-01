using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UIDiffTextInputPresenter : UserControl
{
    public UIDiffTextInputPresenter()
    {
        this.InitializeComponent();

        Loaded += UIDiffTextInputPresenter_Loaded;
        Unloaded += UIDiffTextInputPresenter_Unloaded;
    }

    internal IUIDiffTextInput UIDiffTextInput => (IUIDiffTextInput)DataContext;

    private void UIDiffTextInputPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
    }

    private void UIDiffTextInputPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= UIDiffTextInputPresenter_Loaded;
        Unloaded -= UIDiffTextInputPresenter_Unloaded;
    }
}
