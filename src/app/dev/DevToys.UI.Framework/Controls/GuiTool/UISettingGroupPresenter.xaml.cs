using DevToys.Api;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UISettingGroupPresenter : Expander
{
    public UISettingGroupPresenter()
    {
        this.InitializeComponent();

        CornerRadius = new CornerRadius(4); // Default corner radius.

        Loaded += UISettingGroupPresenter_Loaded;
        Unloaded += UISettingGroupPresenter_Unloaded;
    }

    internal IUISettingGroup UISettingGroup => (IUISettingGroup)DataContext;

    private void UISettingGroupPresenter_Loaded(object sender, RoutedEventArgs e)
    {
        UISettingGroup.IsEnabledChanged += UISettingGroup_IsEnabledChanged;
        UISettingGroup.IsVisibleChanged += UISettingGroup_IsVisibleChanged;
        UISettingGroup.TitleChanged += UISettingGroup_TitleChanged;
        UISettingGroup.DescriptionChanged += UISettingGroup_DescriptionChanged;
        UISettingGroup.IconChanged += UISettingGroup_IconChanged;
        UISettingGroup.InteractiveElementChanged += UISettingGroup_InteractiveElementChanged;
        UISettingGroup.ChildrenChanged += UISettingGroup_ChildrenChanged;

        IsEnabled = UISettingGroup.IsEnabled;
        Visibility = UISettingGroup.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        UISettingHeaderControl.Title = UISettingGroup.Title;
        UISettingHeaderControl.Description = UISettingGroup.Description;
        UISettingHeaderControl.SettingActionableElement = UISettingGroup.InteractiveElement;
        UISettingHeaderControl.Icon = UISettingGroup.Icon;
        if (UISettingGroup.Icon != null)
        {
            UISettingGroup.Icon.Size(20);
        }

        SetChildren();

        AutomationProperties.SetName(this, UISettingGroup.Title);
        AutomationProperties.SetHelpText(this, UISettingGroup.Description);
    }

    private void UISettingGroupPresenter_Unloaded(object sender, RoutedEventArgs e)
    {
        UISettingGroup.IsEnabledChanged -= UISettingGroup_IsEnabledChanged;
        UISettingGroup.IsVisibleChanged -= UISettingGroup_IsVisibleChanged;
        UISettingGroup.TitleChanged -= UISettingGroup_TitleChanged;
        UISettingGroup.DescriptionChanged -= UISettingGroup_DescriptionChanged;
        UISettingGroup.IconChanged -= UISettingGroup_IconChanged;
        UISettingGroup.InteractiveElementChanged -= UISettingGroup_InteractiveElementChanged;
        UISettingGroup.ChildrenChanged -= UISettingGroup_ChildrenChanged;
        Loaded -= UISettingGroupPresenter_Loaded;
        Unloaded -= UISettingGroupPresenter_Unloaded;
    }

    private void UISettingGroup_TitleChanged(object? sender, EventArgs e)
    {
        UISettingHeaderControl.Title = UISettingGroup.Title;
    }

    private void UISettingGroup_DescriptionChanged(object? sender, EventArgs e)
    {
        UISettingHeaderControl.Description = UISettingGroup.Description;
    }

    private void UISettingGroup_IconChanged(object? sender, EventArgs e)
    {
        if (UISettingGroup.Icon != null)
        {
            UISettingGroup.Icon.Size(20);
        }
        UISettingHeaderControl.Icon = UISettingGroup.Icon;
    }

    private void UISettingGroup_InteractiveElementChanged(object? sender, EventArgs e)
    {
        UISettingHeaderControl.SettingActionableElement = UISettingGroup.InteractiveElement;
    }

    private void UISettingGroup_ChildrenChanged(object? sender, EventArgs e)
    {
        SetChildren();
    }

    private void UISettingGroup_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UISettingGroup.IsEnabled;
    }

    private void UISettingGroup_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UISettingGroup.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetChildren()
    {
        ContentStackPanel.Children.Clear();
        if (UISettingGroup.Children is not null)
        {
            for (int i = 0; i < UISettingGroup.Children.Length; i++)
            {
                IUIElement element = UISettingGroup.Children[i];
                if (UISettingGroup.ChildrenAreAllSettings)
                {
                    CornerRadius cornerRadius;
                    if (i == UISettingGroup.Children.Length - 1)
                    {
                        cornerRadius = new CornerRadius(0, 0, 4, 4);
                    }
                    else
                    {
                        cornerRadius = new CornerRadius(0);
                    }

                    var settingPresenter
                        = new UISettingPresenter()
                        {
                            DataContext = element,
                            CornerRadius = cornerRadius
                        };

                    ContentStackPanel.Children.Add(settingPresenter);
                }
                else
                {
                    ContentStackPanel.Children.Add(UIElementPresenter.Create(element));
                }
            }

            // Adjust the spacing inside of the group.
            if (UISettingGroup.ChildrenAreAllSettings)
            {
                ContentStackPanel.Spacing = 0;
                ContentStackPanel.Padding = default;
                ContentStackPanel.Margin = default;
            }
            else
            {
                ContentStackPanel.Spacing = 4;
                ContentStackPanel.Padding = new Thickness(12);
                ContentStackPanel.Margin = new Thickness(42, 0, 6, 0);
            }
        }
    }
}
