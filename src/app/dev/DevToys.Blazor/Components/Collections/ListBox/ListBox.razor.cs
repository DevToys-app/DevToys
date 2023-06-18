namespace DevToys.Blazor.Components;

public partial class ListBox<TElement> : SelectBase<TElement> where TElement : class
{
    [Parameter]
    public string Role { get; set; } = "listbox";

    [Parameter]
    public bool UseNativeScrollBar { get; set; }

    internal ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }
}
