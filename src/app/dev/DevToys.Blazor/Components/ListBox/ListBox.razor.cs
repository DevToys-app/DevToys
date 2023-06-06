using System.Collections.Specialized;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class ListBox<TElement> : JSStyledComponentBase where TElement : class
{
    private int _oldSelectedIndex;

    [Parameter]
    public ICollection<TElement>? Items { get; set; }

    [Parameter]
    public RenderFragment<TElement> ItemTemplate { get; set; } = default!;

    [Parameter]
    public int SelectedIndex { get; set; } = -1;

    [Parameter]
    public EventCallback<int> OnSelectedIndexChanged { get; set; }

    [Parameter]
    public string Role { get; set; } = "listbox";

    [Parameter]
    public bool OverrideDefaultItemTemplate { get; set; }

    [Parameter]
    public bool Virtualize { get; set; } = true;

    [Parameter]
    public bool RaiseSelectionEventOnKeyboardNavigation { get; set; } = true;

    internal TElement? SelectedItem { get; private set; }

    internal ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }
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

        if (raiseEvent)
        {
            RaiseOnSelectedIndexChanged();
        }

        StateHasChanged();
    }

    private void SetSelectedItem(TElement? item, bool raiseEvent)
    {
        Guard.IsNotNull(Items);
        if (item == SelectedItem)
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

    private void RaiseOnSelectedIndexChanged()
    {
        if (OnSelectedIndexChanged.HasDelegate)
        {
            OnSelectedIndexChanged.InvokeAsync(SelectedIndex);
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetSelectedItem(SelectedItem, raiseEvent: false);
    }

    private void OnItemClick(object? selectedItem)
    {
        SetSelectedItem(selectedItem as TElement, raiseEvent: true);
    }

    private void OnKeyDown(KeyboardEventArgs ev)
    {
        if (Items is not null && Items.Count > 0)
        {
            if (string.Equals(ev.Key, "Enter", StringComparison.OrdinalIgnoreCase))
            {
                RaiseOnSelectedIndexChanged();
            }
            else if (string.Equals(ev.Key, "ArrowDown", StringComparison.OrdinalIgnoreCase))
            {
                SelectNextItem();
            }
            else if (string.Equals(ev.Key, "ArrowUp", StringComparison.OrdinalIgnoreCase))
            {
                SelectPreviousItem();
            }
            else if (string.Equals(ev.Key, "Home", StringComparison.OrdinalIgnoreCase))
            {
                SetSelectedIndex(Math.Min(0, Items.Count - 1));
            }
            else if (string.Equals(ev.Key, "End", StringComparison.OrdinalIgnoreCase))
            {
                SetSelectedIndex(Items.Count - 1);
            }
        }
    }
}
