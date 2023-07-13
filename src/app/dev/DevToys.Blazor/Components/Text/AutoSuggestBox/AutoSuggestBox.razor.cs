using System.Collections.Specialized;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class AutoSuggestBox<TElement> : StyledComponentBase, IFocusable, IDisposable where TElement : class
{
    private TextBox? _textBox = default!;
    private ListBox<TElement>? _resultListBox = default!;
    private bool _showDropDown;

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public EventCallback<string> QueryChanged { get; set; }

    [Parameter]
    public EventCallback<TElement?> QuerySubmitted { get; set; }

    [Parameter]
    public ICollection<TElement>? Items { get; set; }

    [Parameter]
    public RenderFragment<TElement> ItemTemplate { get; set; } = default!;

    public string? Query => _textBox?.Text;

    public ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_textBox);
        return _textBox.FocusAsync();
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

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Guard.IsNotNull(Items);
        _showDropDown = IsActuallyEnabled && !IsReadOnly && Items.Count > 0;
    }

    private Task OnTextBoxTextChangedAsync(string text)
    {
        return QueryChanged.InvokeAsync(text);
    }

    private void OnSelectedIndexChanged(int index)
    {
        SubmitQuery();
    }

    private void OnTextBoxKeyPress(KeyboardEventArgs ev)
    {
        if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            SubmitQuery();
        }
        else if (string.Equals(ev.Code, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            Guard.IsNotNull(_textBox);
            _textBox.SetTextAsync(string.Empty).Forget();
        }
        else if (_showDropDown && _resultListBox is not null && Items is not null)
        {
            if (string.Equals(ev.Code, "ArrowDown", StringComparison.OrdinalIgnoreCase))
            {
                _resultListBox.SelectNextItem();
            }
            else if (string.Equals(ev.Code, "ArrowUp", StringComparison.OrdinalIgnoreCase))
            {
                _resultListBox.SelectPreviousItem();
            }
        }
    }

    private void OnTextBoxFocusLost()
    {
        // Bug workaround: user may click on an item in the drop down list. Text box will lose focus
        // before the item click event propagate. So we introduce a small delay here, to let time to propagate the event
        // before closing the drop down.
        // NOTE: This approach can be an issue if the user do a long-press on an item. Alternative possibility: track whether the focus
        // is in the list box?
        Task.Delay(150).ContinueWith(_ =>
        {
            InvokeAsync(() =>
            {
                _showDropDown = false;
                StateHasChanged();

                Guard.IsNotNull(_textBox);
                _textBox.SetTextAsync(string.Empty).Forget();
            });
        });
    }

    private void SubmitQuery()
    {
        if (QuerySubmitted.HasDelegate)
        {
            TElement? selectedItem = null;
            if (_resultListBox is not null)
            {
                selectedItem = _resultListBox.SelectedItem;
                if (selectedItem == null && _resultListBox.Items?.Count > 0)
                {
                    selectedItem = _resultListBox.Items.First();
                }
            }
            QuerySubmitted.InvokeAsync(selectedItem);
            _showDropDown = false;
        }
    }

    public void Dispose()
    {
        if (Items is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= OnItemsChanged;
        }

        GC.SuppressFinalize(this);
    }
}
