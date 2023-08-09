using Microsoft.AspNetCore.Components.Web;
using System.Collections.Specialized;

namespace DevToys.Blazor.Components;

public abstract class SelectBase<TElement> : JSStyledComponentBase where TElement : class
{
    private int _oldSelectedIndex = int.MinValue;
    private TElement? _oldSelectedItem = null;

    [Parameter]
    public ICollection<TElement>? Items { get; set; }

    [Parameter]
    public RenderFragment<TElement> ItemTemplate { get; set; } = default!;

    [Parameter]
    public TElement? SelectedItem { get; set; }

    [Parameter]
    public int SelectedIndex { get; set; } = -1;

    [Parameter]
    public EventCallback<int> SelectedIndexChanged { get; set; }

    [Parameter]
    public EventCallback<TElement> SelectedItemChanged { get; set; }

    [Parameter]
    public bool CanSelectItem { get; set; } = true;

    [Parameter]
    public bool OverrideDefaultItemTemplate { get; set; }

    [Parameter]
    public bool Virtualize { get; set; } = true;

    [Parameter]
    public virtual bool RaiseSelectionEventOnKeyboardNavigation { get; set; } = true;

    internal void SelectNextItem()
    {
        if (Items is not null)
        {
            if (SelectedIndex == -1 || SelectedIndex == Items.Count - 1)
            {
                SetSelectedIndex(0, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
            else
            {
                SetSelectedIndex(SelectedIndex + 1, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
        }
    }

    internal void SelectPreviousItem()
    {
        if (Items is not null)
        {
            if (SelectedIndex == -1 || SelectedIndex == 0)
            {
                SetSelectedIndex(Items.Count - 1, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
            else
            {
                SetSelectedIndex(SelectedIndex - 1, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
        }
    }

    internal void SetSelectedIndex(int index)
    {
        SetSelectedIndex(index, raiseEvent: false);
    }

    internal void SetSelectedItem(TElement? item)
    {
        SetSelectedItem(item, raiseEvent: false);
    }

    public override ValueTask DisposeAsync()
    {
        if (Items is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= OnItemsChanged;
        }
        return base.DisposeAsync();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_oldSelectedIndex != SelectedIndex)
        {
            SetSelectedIndex(SelectedIndex, raiseEvent: false);
        }
        else if (_oldSelectedItem != SelectedItem)
        {
            SetSelectedItem(SelectedItem, raiseEvent: false);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
        {
            if (Items is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += OnItemsChanged;
            }
        }
    }

    private void SetSelectedIndex(int index, bool raiseEvent)
    {
        if (!CanSelectItem)
        {
            return;
        }

        if (index >= Items?.Count)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
        }

        SelectedIndex = index;
        _oldSelectedIndex = index;
        if (index > -1)
        {
            Guard.IsNotNull(Items);
            SelectedItem = Items.ElementAt(index);
        }
        else
        {
            SelectedItem = default;
        }
        _oldSelectedItem = SelectedItem;

        if (raiseEvent)
        {
            RaiseOnSelectionChanged();
        }

        StateHasChanged();
    }

    private void SetSelectedItem(TElement? item, bool raiseEvent)
    {
        Guard.IsNotNull(Items);

        if (!CanSelectItem)
        {
            return;
        }
        else if (item == SelectedItem && !raiseEvent)
        {
            return;
        }
        else if (item is null)
        {
            SetSelectedIndex(-1, raiseEvent);
        }
        else if (Items is IList<TElement> itemsList)
        {
            int itemIndex = itemsList.IndexOf(item);
            SetSelectedIndex(itemIndex, raiseEvent);
        }
        else
        {
            int i = 0;
            foreach (TElement childItem in Items)
            {
                if (object.ReferenceEquals(childItem, item))
                {
                    SetSelectedIndex(i, raiseEvent);
                    return;
                }
                i++;
            }

            SetSelectedIndex(-1, raiseEvent);
        }
    }

    private void RaiseOnSelectionChanged()
    {
        if (CanSelectItem)
        {
            OnItemSelected();

            if (SelectedIndexChanged.HasDelegate)
            {
                SelectedIndexChanged.InvokeAsync(SelectedIndex);
            }

            if (SelectedItemChanged.HasDelegate)
            {
                SelectedItemChanged.InvokeAsync(SelectedItem);
            }
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetSelectedItem(SelectedItem, raiseEvent: false);
        StateHasChanged();
    }

    protected void OnItemClick(object? selectedItem)
    {
        SetSelectedItem(selectedItem as TElement, raiseEvent: true);
    }

    protected void OnKeyDown(KeyboardEventArgs ev)
    {
        if (Items is not null && Items.Count > 0)
        {
            if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ev.Code, "Space", StringComparison.OrdinalIgnoreCase))
            {
                RaiseOnSelectionChanged();
            }
            else if (string.Equals(ev.Code, "ArrowDown", StringComparison.OrdinalIgnoreCase))
            {
                SelectNextItem();
            }
            else if (string.Equals(ev.Code, "ArrowUp", StringComparison.OrdinalIgnoreCase))
            {
                SelectPreviousItem();
            }
            else if (string.Equals(ev.Code, "Home", StringComparison.OrdinalIgnoreCase))
            {
                SetSelectedIndex(Math.Min(0, Items.Count - 1));
            }
            else if (string.Equals(ev.Code, "End", StringComparison.OrdinalIgnoreCase))
            {
                SetSelectedIndex(Items.Count - 1);
            }
        }
    }

    protected virtual void OnItemSelected()
    {
    }
}
