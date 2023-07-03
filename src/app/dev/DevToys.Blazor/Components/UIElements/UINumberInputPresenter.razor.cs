namespace DevToys.Blazor.Components.UIElements;

public partial class UINumberInputPresenter : StyledComponentBase
{
    private UITextInputWrapper _textInputWrapper = default!;

    [Parameter]
    public IUINumberInput UINumberInput { get; set; } = default!;

    private void OnTextChanged(string newText)
    {
        UINumberInput.Text(newText);
    }
}
