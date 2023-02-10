using DevToys.Api.Core.Theme;
using DevToys.UI.Framework.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace DevToys.UI.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : BackdropPage
{
    private const string CompactOverlayStateName = "CompactOverlay";
    private const string NavigationViewExpandedStateName = "NavigationViewExpanded";
    private const string NavigationViewCompactStateName = "NavigationViewCompact";
    private const string NavigationViewMinimalStateName = "NavigationViewMinimal";

    public MainWindow(BackdropWindow backdropWindow, IThemeListener themeListener)
        : base(backdropWindow, themeListener)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Resize(1200, 800);
    }

    private void SearchBoxKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Focus(FocusState.Keyboard);
    }
}
