namespace DevToys.Api;

/// <summary>
/// A component that represents a drop down list where the user can select one item in it.
/// </summary>
public interface IUISelectDropDownList : IUITitledElement
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
    event EventHandler? ItemsChanged;

    /// <summary>
    /// Raised when <see cref="SelectedItem"/> is changed.
    /// </summary>
    event EventHandler? SelectedItemChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal sealed class UISelectDropDownList : UITitledElement, IUISelectDropDownList
{
    private IUIDropDownListItem[]? _items;
    private IUIDropDownListItem? _selectedItem;

    internal UISelectDropDownList(string? id)
        : base(id)
    {
    }

    public IUIDropDownListItem[]? Items
    {
        get => _items;
        internal set => SetPropertyValue(ref _items, value, ItemsChanged);
    }

    public IUIDropDownListItem? SelectedItem
    {
        get => _selectedItem;
        internal set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnItemSelectedAction?.Invoke(_selectedItem);
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
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
    public static IUISelectDropDownList SelectDropDownList()
    {
        return SelectDropDownList(null);
    }

    /// <summary>
    /// A component that represents a drop down list where the user can select one item in it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISelectDropDownList SelectDropDownList(string? id)
    {
        return new UISelectDropDownList(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISelectDropDownList.Items"/> of the drop down list.
    /// </summary>
    public static IUISelectDropDownList WithItems(this IUISelectDropDownList element, params IUIDropDownListItem[] items)
    {
        ((UISelectDropDownList)element).Items = items;
        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the drop down list.
    /// </summary>
    public static IUISelectDropDownList OnItemSelected(this IUISelectDropDownList element, Func<IUIDropDownListItem?, ValueTask>? onItemSelectedAction)
    {
        ((UISelectDropDownList)element).OnItemSelectedAction = onItemSelectedAction;
        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the drop down list.
    /// </summary>
    public static IUISelectDropDownList OnItemSelected(this IUISelectDropDownList element, Action<IUIDropDownListItem?>? onItemSelectedAction)
    {
        ((UISelectDropDownList)element).OnItemSelectedAction
            = (item) =>
            {
                onItemSelectedAction?.Invoke(item);
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownListItem"/> that should be selected in the drop down list.
    /// If <paramref name="item"/> is null or does not exist in the list, no item will be selected.
    /// </summary>
    public static IUISelectDropDownList Select(this IUISelectDropDownList element, IUIDropDownListItem? item)
    {
        var dropDownList = (UISelectDropDownList)element;
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
    public static IUISelectDropDownList Select(this IUISelectDropDownList element, int index)
    {
        var dropDownList = (UISelectDropDownList)element;

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
