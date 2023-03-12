using DevToys.Api;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls.GuiTool;

/// <summary>
/// The template selector used to display <see cref="IUIElement"/>
/// </summary>
internal sealed class UIElementTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// The data template used to display a <see cref="IUIButton"/>.
    /// </summary>
    public DataTemplate UIButtonTemplate { get; set; } = null!;

    /// <summary>
    /// The data template used to display a <see cref="IUIStack"/>.
    /// </summary>
    public DataTemplate UIStackTemplate { get; set; } = null!;

    /// <summary>
    /// The data template used to display a <see cref="IUIIcon"/>.
    /// </summary>
    public DataTemplate UIIconTemplate { get; set; } = null!;

    /// <summary>
    /// The data template used to display a <see cref="IUISetting"/>.
    /// </summary>
    public DataTemplate UISettingTemplate { get; set; } = null!;

    /// <summary>
    /// The data template used to display a <see cref="IUISettingGroup"/>.
    /// </summary>
    public DataTemplate UISettingGroupTemplate { get; set; } = null!;

    /// <summary>
    /// The data template used to display a <see cref="IUISwitch"/>.
    /// </summary>
    public DataTemplate UISwitchTemplate { get; set; } = null!;

    /// <summary>
    /// The data template used to display a <see cref="IUIDropDownList"/>.
    /// </summary>
    public DataTemplate UIDropDownListTemplate { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is null)
        {
            return null!;
        }

        return item switch
        {
            IUIButton => UIButtonTemplate,
            IUIStack => UIStackTemplate,
            IUIIcon => UIIconTemplate,
            IUISettingGroup => UISettingGroupTemplate,
            IUISetting => UISettingTemplate,
            IUISwitch => UISwitchTemplate,
            IUIDropDownList => UIDropDownListTemplate,

            _ => throw new NotSupportedException($"Gui Tool component of type '{item.GetType().FullName}' isn't supported.")
        };
    }
}
