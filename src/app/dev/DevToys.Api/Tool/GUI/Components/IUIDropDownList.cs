namespace DevToys.Api;

/// <summary>
/// A component that represents a drop down list where the user can select one item in it.
/// </summary>
public interface IUIDropDownList : IUITitledElement
{
    /// <summary>
    /// Gets the list of items displayed in the drop down list.
    /// </summary>
    IUIDropDownListItem[]? Items { get; }

    /// <summary>
    /// Gets the selected item in the drop down list.
    /// </summary>
    IUIDropDownListItem? SelectedItem { get; }

    /// <summary>
    /// Gets the action to run when the user selects an item in the list.
    /// </summary>
    Func<IUIDropDownListItem?, ValueTask>? OnItemSelectedAction { get; }

    /// <summary>
    /// Raised when <see cref="Items"/> is changed.
    /// </summary>
    public event EventHandler? ItemsChanged;

    /// <summary>
    /// Raised when <see cref="SelectedItem"/> is changed.
    /// </summary>
    public event EventHandler? SelectedItemChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal sealed class UIDropDownList : UITitledElement, IUIDropDownList
{
    private IUIDropDownListItem[]? _items;
    private IUIDropDownListItem? _selectedItem;

    internal UIDropDownList(string? id)
        : base(id)
    {
    }

    public IUIDropDownListItem[]? Items
    {
        get => _items;
        internal set
        {
            _items = value;
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IUIDropDownListItem? SelectedItem
    {
        get => _selectedItem;
        internal set
        {
            _selectedItem = value;
            OnItemSelectedAction?.Invoke(_selectedItem);
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Func<IUIDropDownListItem?, ValueTask>? OnItemSelectedAction { get; internal set; }

    public event EventHandler? ItemsChanged;
    public event EventHandler? SelectedItemChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a drop down list where the user can select one item in it.
    /// </summary>
    public static IUIDropDownList DropDownList()
    {
        return DropDownList(null);
    }

    /// <summary>
    /// A component that represents a drop down list where the user can select one item in it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIDropDownList DropDownList(string? id)
    {
        return new UIDropDownList(id);
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownList.Items"/> of the drop down list.
    /// </summary>
    public static IUIDropDownList WithItems(this IUIDropDownList element, params IUIDropDownListItem[] items)
    {
        ((UIDropDownList)element).Items = items;
        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the drop down list.
    /// </summary>
    public static IUIDropDownList OnItemSelected(this IUIDropDownList element, Func<IUIDropDownListItem?, ValueTask>? onItemSelectedAction)
    {
        ((UIDropDownList)element).OnItemSelectedAction = onItemSelectedAction;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownListItem"/> that should be selected in the drop down list.
    /// If <paramref name="item"/> is null or does not exist in the list, no item will be selected.
    /// </summary>
    public static IUIDropDownList Select(this IUIDropDownList element, IUIDropDownListItem? item)
    {
        var dropDownList = (UIDropDownList)element;
        if (item is not null && dropDownList.Items is not null && !dropDownList.Items.Contains(item))
        {
            dropDownList.SelectedItem = null;
        }
        else
        {
            dropDownList.SelectedItem = item;
        }

        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownListItem"/> that should be selected in the drop down list, using its index in the list.
    /// If <paramref name="index"/> smaller or greater than the amount of items in the list, no item will be selected.
    /// </summary>
    public static IUIDropDownList Select(this IUIDropDownList element, int index)
    {
        var dropDownList = (UIDropDownList)element;

        if (dropDownList.Items is null || index < 0 || index > dropDownList.Items.Length - 1)
        {
            dropDownList.SelectedItem = null;
        }
        else
        {
            dropDownList.SelectedItem = dropDownList.Items[index];
        }

        return element;
    }
}
