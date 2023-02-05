using DevToys.UI.Framework.Controls;

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

    public MainWindow(BackdropWindow backdropWindow)
        : base(backdropWindow)
    {
        this.InitializeComponent();
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        this.Resize(1200, 800);
    }
}
