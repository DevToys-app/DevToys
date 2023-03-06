using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UIButtonPresenter : Button
{
    public UIButtonPresenter()
    {
        this.InitializeComponent();

        Loaded += UIButtonPresenter_Loaded;
        Unloaded += UIButtonPresenter_Unloaded;
    }

    internal IUIButton UIButton => (IUIButton)DataContext;

    private void UIButtonPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UIButton.IsEnabledChanged += UIButton_IsEnabledChanged;
        UIButton.IsVisibleChanged += UIButton_IsVisibleChanged;
        UIButton.DisplayTextChanged += UIButton_DisplayTextChanged;

        IsEnabled = UIButton.IsEnabled;
        Visibility = UIButton.IsVisible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Content = UIButton.DisplayText;
    }

    private void UIButtonPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UIButton.IsEnabledChanged -= UIButton_IsEnabledChanged;
        UIButton.IsVisibleChanged -= UIButton_IsVisibleChanged;
        UIButton.DisplayTextChanged -= UIButton_DisplayTextChanged;
        Loaded -= UIButtonPresenter_Loaded;
    }

    private void UIButton_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UIButton.IsEnabled;
    }

    private void UIButton_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UIButton.IsVisible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void UIButton_DisplayTextChanged(object? sender, EventArgs e)
    {
        Content = UIButton.DisplayText;
    }

    private void OnButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UIButton.OnClickAction?.Invoke(); // TODO: await? Also, Try Catch and log?
    }
}
