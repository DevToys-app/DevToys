using DevToys.Api;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UISettingPresenter : UserControl
{
    public UISettingPresenter()
    {
        this.InitializeComponent();

        CornerRadius = new CornerRadius(4); // Default corner radius.

        Loaded += UISettingPresenter_Loaded;
        Unloaded += UISettingPresenter_Unloaded;
    }

    internal IUISetting UISetting => (IUISetting)DataContext;

    internal void ClearBackground()
    {
        NonExpanderGrid.Background = null;
    }

    private void UISettingPresenter_Loaded(object sender, RoutedEventArgs e)
    {
        UISetting.IsEnabledChanged += UISetting_IsEnabledChanged;
        UISetting.IsVisibleChanged += UISetting_IsVisibleChanged;
        UISetting.TitleChanged += UISetting_TitleChanged;
        UISetting.DescriptionChanged += UISetting_DescriptionChanged;
        UISetting.IconChanged += UISetting_IconChanged;
        UISetting.InteractiveElementChanged += UISetting_InteractiveElementChanged;

        IsEnabled = UISetting.IsEnabled;
        Visibility = UISetting.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        UISettingHeaderControl.Title = UISetting.Title;
        UISettingHeaderControl.Description = UISetting.Description;
        UISettingHeaderControl.SettingActionableElement = UISetting.InteractiveElement;
        UISettingHeaderControl.Icon = UISetting.Icon;
        if (UISetting.Icon != null)
        {
            UISetting.Icon.Size(20);
        }
    }

    private void UISettingPresenter_Unloaded(object sender, RoutedEventArgs e)
    {
        UISetting.IsEnabledChanged -= UISetting_IsEnabledChanged;
        UISetting.IsVisibleChanged -= UISetting_IsVisibleChanged;
        UISetting.TitleChanged -= UISetting_TitleChanged;
        UISetting.DescriptionChanged -= UISetting_DescriptionChanged;
        UISetting.IconChanged -= UISetting_IconChanged;
        UISetting.InteractiveElementChanged -= UISetting_InteractiveElementChanged;
        Loaded -= UISettingPresenter_Loaded;
        Unloaded -= UISettingPresenter_Unloaded;
    }

    private void UISetting_TitleChanged(object? sender, EventArgs e)
    {
        UISettingHeaderControl.Title = UISetting.Title;
    }

    private void UISetting_DescriptionChanged(object? sender, EventArgs e)
    {
        UISettingHeaderControl.Description = UISetting.Description;
    }

    private void UISetting_IconChanged(object? sender, EventArgs e)
    {
        if (UISetting.Icon != null)
        {
            UISetting.Icon.Size(20);
        }
        UISettingHeaderControl.Icon = UISetting.Icon;
    }

    private void UISetting_InteractiveElementChanged(object? sender, EventArgs e)
    {
        UISettingHeaderControl.SettingActionableElement = UISetting.InteractiveElement;
    }

    private void UISetting_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UISetting.IsEnabled;
    }

    private void UISetting_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UISetting.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }
}
