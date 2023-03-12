using DevToys.Api;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace DevToys.UI.Framework.Controls.GuiTool;

[ContentProperty(Name = nameof(SettingActionableElement))]
public sealed partial class UISettingHeaderControl : UserControl
{
    public static readonly DependencyProperty SettingActionableElementProperty
       = DependencyProperty.Register(
           nameof(SettingActionableElement),
           typeof(IUIElement),
           typeof(UISettingHeaderControl),
           new PropertyMetadata(null));

    public IUIElement? SettingActionableElement
    {
        get => (IUIElement?)GetValue(SettingActionableElementProperty);
        set => SetValue(SettingActionableElementProperty, value);
    }

    public static readonly DependencyProperty TitleProperty
       = DependencyProperty.Register(
           nameof(Title),
           typeof(string),
           typeof(UISettingHeaderControl),
           new PropertyMetadata(
               string.Empty,
               (d, e) =>
               {
                   AutomationProperties.SetName(d, (string)e.NewValue);
               }));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty
        = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(UISettingHeaderControl),
            new PropertyMetadata(
                string.Empty,
                (d, e) =>
                {
                    AutomationProperties.SetHelpText(d, (string)e.NewValue);
                }));

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(IUIIcon),
            typeof(UISettingHeaderControl),
            new PropertyMetadata(null));

    public IUIIcon? Icon
    {
        get => (IUIIcon?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public UISettingHeaderControl()
    {
        InitializeComponent();
        VisualStateManager.GoToState(this, "NormalState", false);
    }

    private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width == e.PreviousSize.Width || ActionableElement == null)
        {
            return;
        }

        if (ActionableElement.ActualWidth > e.NewSize.Width / 3)
        {
            VisualStateManager.GoToState(this, "CompactState", false);
        }
        else
        {
            VisualStateManager.GoToState(this, "NormalState", false);
        }
    }
}
