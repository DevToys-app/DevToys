#if WINDOWS_UWP
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DevToys.UI.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private const string CompactOverlayStateName = "CompactOverlay";
    private const string NavigationViewExpandedStateName = "NavigationViewExpanded";
    private const string NavigationViewCompactStateName = "NavigationViewCompact";
    private const string NavigationViewMinimalStateName = "NavigationViewMinimal";

    public MainPage()
    {
        this.InitializeComponent();

#if WINDOWS_UWP
        // Enable Mica effect.
        BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
#endif
    }
}
