using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UIDropDownListPresenter : UserControl
{
    public UIDropDownListPresenter()
    {
        this.InitializeComponent();

        Loaded += UIDropDownListPresenter_Loaded;
        Unloaded += UIDropDownListPresenter_Unloaded;
    }

    internal IUIDropDownList UIDropDownList => (IUIDropDownList)DataContext;

    private void UIDropDownListPresenter_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UIDropDownList.IsEnabledChanged += UIDropDownList_IsEnabledChanged;
        UIDropDownList.IsVisibleChanged += UIDropDownList_IsVisibleChanged;
        UIDropDownList.TitleChanged += UIDropDownList_TitleChanged;
        UIDropDownList.ItemsChanged += UIDropDownList_ItemsChanged;
        UIDropDownList.SelectedItemChanged += UIDropDownList_SelectedItemChanged;

        IsEnabled = UIDropDownList.IsEnabled;
        Visibility = UIDropDownList.IsVisible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        ComboBox.Header = UIDropDownList.Title!;
        SetItems();
        if (UIDropDownList.Items is null)
        {
            ComboBox.SelectedIndex = -1;
        }
        else
        {
            ComboBox.SelectedIndex = Array.IndexOf(UIDropDownList.Items, UIDropDownList.SelectedItem);
        }

        ComboBox.SelectionChanged += UIDropDownListPresenter_SelectionChanged;
    }

    private void UIDropDownListPresenter_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ComboBox.SelectionChanged -= UIDropDownListPresenter_SelectionChanged;
        UIDropDownList.IsEnabledChanged -= UIDropDownList_IsEnabledChanged;
        UIDropDownList.IsVisibleChanged -= UIDropDownList_IsVisibleChanged;
        UIDropDownList.TitleChanged -= UIDropDownList_TitleChanged;
        UIDropDownList.ItemsChanged -= UIDropDownList_ItemsChanged;
        UIDropDownList.SelectedItemChanged -= UIDropDownList_SelectedItemChanged;
        Loaded -= UIDropDownListPresenter_Loaded;
        Unloaded -= UIDropDownListPresenter_Unloaded;
    }

    private void UIDropDownList_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UIDropDownList.IsEnabled;
    }

    private void UIDropDownList_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UIDropDownList.IsVisible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void UIDropDownList_SelectedItemChanged(object? sender, EventArgs e)
    {
        if (UIDropDownList.Items is null)
        {
            ComboBox.SelectedIndex = -1;
        }
        else
        {
            ComboBox.SelectedIndex = Array.IndexOf(UIDropDownList.Items, UIDropDownList.SelectedItem);
        }
    }

    private void UIDropDownList_ItemsChanged(object? sender, EventArgs e)
    {
        SetItems();
    }

    private void UIDropDownList_TitleChanged(object? sender, EventArgs e)
    {
        ComboBox.Header = UIDropDownList.Title!;
    }

    private void UIDropDownListPresenter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UIDropDownList.Select(ComboBox.SelectedIndex);
    }

    private void SetItems()
    {
        ComboBox.Items.Clear();
        if (UIDropDownList.Items is not null)
        {
            for (int i = 0; i < UIDropDownList.Items.Length; i++)
            {
                ComboBox.Items.Add(new ComboBoxItem()
                {
                    Tag = UIDropDownList.Items[i],
                    Content = UIDropDownList.Items[i].Text
                });
            }
        }
    }
}
