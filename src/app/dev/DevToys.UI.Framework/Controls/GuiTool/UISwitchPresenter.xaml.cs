using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UISwitchPresenter : ContentControl
{
    private bool _toggledProgrammatically;

    public UISwitchPresenter()
    {
        this.InitializeComponent();

        Loaded += UISwitchPresenter_Loaded;
        Unloaded += UISwitchPresenter_Unloaded;
    }

    internal IUISwitch UISwitch => (IUISwitch)DataContext;

    private void UISwitchPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UISwitch.IsEnabledChanged += UISwitch_IsEnabledChanged;
        UISwitch.IsVisibleChanged += UISwitch_IsVisibleChanged;
        UISwitch.OnTextChanged += UISwitch_OnTextChanged;
        UISwitch.OffTextChanged += UISwitch_OffTextChanged;
        UISwitch.IsOnChanged += UISwitch_IsOnChanged;

        IsEnabled = UISwitch.IsEnabled;
        Visibility = UISwitch.IsVisible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        ToggleSwitch.OnContent = UISwitch.OnText;
        ToggleSwitch.OffContent = UISwitch.OffText;

        _toggledProgrammatically = true;
        ToggleSwitch.IsOn = UISwitch.IsOn;
        _toggledProgrammatically = false;
    }

    private void UISwitchPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UISwitch.IsEnabledChanged -= UISwitch_IsEnabledChanged;
        UISwitch.IsVisibleChanged -= UISwitch_IsVisibleChanged;
        UISwitch.OnTextChanged -= UISwitch_OnTextChanged;
        UISwitch.OffTextChanged -= UISwitch_OffTextChanged;
        UISwitch.IsOnChanged -= UISwitch_IsOnChanged;
        Loaded -= UISwitchPresenter_Loaded;
        Unloaded -= UISwitchPresenter_Unloaded;
    }

    private void UISwitch_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UISwitch.IsEnabled;
    }

    private void UISwitch_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UISwitch.IsVisible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void UISwitch_IsOnChanged(object? sender, EventArgs e)
    {
        _toggledProgrammatically = true;
        ToggleSwitch.IsOn = UISwitch.IsOn;
        _toggledProgrammatically = false;
    }

    private void UISwitch_OffTextChanged(object? sender, EventArgs e)
    {
        ToggleSwitch.OffContent = UISwitch.OffText;
    }

    private void UISwitch_OnTextChanged(object? sender, EventArgs e)
    {
        ToggleSwitch.OnContent = UISwitch.OnText;
    }

    private void ToggleSwitch_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!_toggledProgrammatically)
        {
            if (ToggleSwitch.IsOn)
            {
                UISwitch.On();
            }
            else
            {
                UISwitch.Off();
            }
        }
    }
}
