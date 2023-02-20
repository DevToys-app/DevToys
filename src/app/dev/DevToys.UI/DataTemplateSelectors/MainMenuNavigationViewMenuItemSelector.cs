using DevToys.Core.Tools.ViewItems;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.DataTemplateSelectors;

public sealed class MainMenuNavigationViewMenuItemSelector : DataTemplateSelector
{
    public DataTemplate GuiToolViewItem { get; set; } = null!;

    public DataTemplate GroupViewItem { get; set; } = null!;

    public DataTemplate Separator { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is GuiToolViewItem)
        {
            return GuiToolViewItem;
        }
        else if (item is GroupViewItem)
        {
            return GroupViewItem;
        }
        else if (item is SeparatorViewItem)
        {
            return Separator;
        }

        throw new NotSupportedException();
    }
}
