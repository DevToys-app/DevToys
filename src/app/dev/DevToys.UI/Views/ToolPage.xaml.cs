using DevToys.UI.Framework.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Views;

/// <summary>
/// Page displaying a tool.
/// </summary>
public sealed partial class ToolPage : Page, IVisualStateListener
{
    public ToolPage()
    {
        this.InitializeComponent();
    }

    public void SetVisualState(string visualStateName)
    {
        Guard.IsNotNullOrWhiteSpace(visualStateName);
        VisualStateManager.GoToState(this, visualStateName, useTransitions: true);
    }
}
