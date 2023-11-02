using System.Collections.ObjectModel;

namespace DevToys.Api;

/// <summary>
/// A component that represents a list of items.
/// </summary>
public interface IUIList : IUIElementWithChildren
{
    /// <summary>
    /// Gets the list of items displayed in the list.
    /// </summary>
    ObservableCollection<IUIListItem> Items { get; }

    /// <summary>
    /// Gets whether items are selectable in the list. Default is true.
    /// </summary>
    bool CanSelectItem { get; }

    /// <summary>
    /// Gets the selected item in the list.
    /// </summary>
    IUIListItem? SelectedItem { get; }

    /// <summary>
    /// Gets the action to run when the user selects an item in the list.
    /// </summary>
    Func<IUIListItem?, ValueTask>? OnItemSelectedAction { get; }

    /// <summary>
    /// Raised when <see cref="CanSelectItem"/> is changed.
    /// </summary>
    event EventHandler? CanSelectItemChanged;

    /// <summary>
    /// Raised when <see cref="SelectedItem"/> is changed.
    /// </summary>
    event EventHandler? SelectedItemChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, SelectedItem = {{{nameof(SelectedItem)}}}")]
internal sealed class UIList : UIElementWithChildren, IUIList, IDisposable
{
    private readonly ObservableCollection<IUIListItem> _items = new();
    private IUIListItem? _selectedItem;
    private bool _canSelectItem = true;

    internal UIList(string? id)
        : base(id)
    {
        _items.CollectionChanged += Items_CollectionChanged;
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        if (_items is not null)
        {
            foreach (IUIListItem item in _items)
            {
                if (item.UIElement is not null)
                {
                    yield return item.UIElement;
                }
            }
        }
    }

    public ObservableCollection<IUIListItem> Items => _items;

    public bool CanSelectItem
    {
        get => _canSelectItem;
        internal set
        {
            SetPropertyValue(ref _canSelectItem, value, CanSelectItemChanged);
            if (!value)
            {
                SelectedItem = null;
            }
        }
    }

    public IUIListItem? SelectedItem
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

    public Func<IUIListItem?, ValueTask>? OnItemSelectedAction { get; internal set; }

    public event EventHandler? CanSelectItemChanged;
    public event EventHandler? SelectedItemChanged;

    public void Dispose()
    {
        _items.CollectionChanged -= Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Try to preserve the selection.
        this.Select(SelectedItem);
    }
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a list of items.
    /// </summary>
    public static IUIList List()
    {
        return List(null);
    }

    /// <summary>
    /// A component that represents a list of items.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIList List(string? id)
    {
        return new UIList(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISelectDropDownList.Items"/> of the list.
    /// </summary>
    public static IUIList WithItems(this IUIList element, params IUIListItem[] items)
    {
        var list = (UIList)element;
        list.Items.Clear();
        list.Items.AddRange(items);

        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the list.
    /// </summary>
    public static IUIList OnItemSelected(this IUIList element, Func<IUIListItem?, ValueTask>? onItemSelectedAction)
    {
        ((UIList)element).OnItemSelectedAction = onItemSelectedAction;
        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the list.
    /// </summary>
    public static IUIList OnItemSelected(this IUIList element, Action<IUIListItem?>? onItemSelectedAction)
    {
        ((UIList)element).OnItemSelectedAction
            = (item) =>
            {
                onItemSelectedAction?.Invoke(item);
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIList"/> that should be selected in the list.
    /// If <paramref name="item"/> is null or does not exist in the list, no item will be selected.
    /// </summary>
    public static IUIList Select(this IUIList element, IUIListItem? item)
    {
        var list = (UIList)element;
        if ((item is not null
            && list.Items is not null
            && !list.Items.Contains(item))
            || !list.CanSelectItem)
        {
            list.SelectedItem = null;
        }
        else
        {
            list.SelectedItem = item;
        }

        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIList"/> that should be selected in the list, using its index in the list.
    /// If <paramref name="index"/> smaller or greater than the amount of items in the list, no item will be selected.
    /// </summary>
    public static IUIList Select(this IUIList element, int index)
    {
        var list = (UIList)element;

        if (list.Items is null
            || index < 0
            || index > list.Items.Count - 1
            || !list.CanSelectItem)
        {
            list.SelectedItem = null;
        }
        else
        {
            list.SelectedItem = list.Items[index];
        }

        return element;
    }

    /// <summary>
    /// Allows the user to select an item in the list.
    /// </summary>
    public static IUIList AllowSelectItem(this IUIList element)
    {
        ((UIList)element).CanSelectItem = true;
        return element;
    }

    /// <summary>
    /// Prevents the user from selecting an item in the list.
    /// </summary>
    public static IUIList ForbidSelectItem(this IUIList element)
    {
        ((UIList)element).CanSelectItem = false;
        return element;
    }

    /// <summary>
    /// Removes the first occurrence of an <see cref="IUIListItem"/> where <see cref="IUIListItem.Value"/> match the given <paramref name="value"/>.
    /// </summary>
    public static void RemoveValue(this ObservableCollection<IUIListItem> listItems, object? value)
    {
        IUIListItem? item = listItems.FirstOrDefault(item => item.Value == value);
        if (item is not null)
        {
            listItems.Remove(item);
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
