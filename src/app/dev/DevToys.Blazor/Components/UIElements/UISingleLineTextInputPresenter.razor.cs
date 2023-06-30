namespace DevToys.Blazor.Components.UIElements;

public partial class UISingleLineTextInputPresenter : ComponentBase
{
    [Parameter]
    public IUISingleLineTextInput UISingleLineTextInput { get; set; } = default!;

    private void OnTextChanged(string newText)
    {
        UISingleLineTextInput.Text(newText);
    }
}
