using System.Collections.ObjectModel;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIDropDownButtonPresenter : ComponentBase, IDisposable
{
    [Parameter]
    public IUIDropDownButton UIDropDownButton { get; set; } = default!;

    internal ObservableCollection<UIDropDownMenuItemDropDownListItem> MenuItemComponents { get; } = new();

    public void Dispose()
    {
        UIDropDownButton.MenuItemsChanged -= UIDropDownButton_MenuItemsChanged;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UIDropDownButton.MenuItemsChanged += UIDropDownButton_MenuItemsChanged;
        UIDropDownButton_MenuItemsChanged(this, EventArgs.Empty);
    }

    private void UIDropDownButton_MenuItemsChanged(object? sender, EventArgs e)
    {
        MenuItemComponents.Clear();

        if (UIDropDownButton.MenuItems is not null)
        {
            EventCallback<DropDownListItem> onClickEventCallback = EventCallback.Factory.Create<DropDownListItem>(this, MenuItem_Click);

            foreach (IUIDropDownMenuItem item in UIDropDownButton.MenuItems)
            {
                MenuItemComponents.Add(new UIDropDownMenuItemDropDownListItem
                {
                    UIDropDownMenuItem = item,
                    IconFontFamily = item.IconFontName ?? string.Empty,
                    IconGlyph = item.IconGlyph,
                    Text = item.Text,
                    IsEnabled = item.IsEnabled,
                    OnClick = onClickEventCallback,
                });
            }
        }

        StateHasChanged();
    }

    private void MenuItem_Click(DropDownListItem menuItem)
    {
        if (menuItem is UIDropDownMenuItemDropDownListItem dropDownMenuItem)
        {
            dropDownMenuItem.UIDropDownMenuItem.OnClickAction?.Invoke();
        }
    }

    public class UIDropDownMenuItemDropDownListItem : DropDownListItem
    {
        internal IUIDropDownMenuItem UIDropDownMenuItem { get; set; } = default!;
    }
}
