namespace DevToys.Blazor.Components.UIElements;

public partial class UIPasswordInputPresenter : StyledComponentBase
{
    private UITextInputWrapper _textInputWrapper = default!;

    [Parameter]
    public IUIPasswordInput UIPasswordInput { get; set; } = default!;

    private void OnTextChanged(string newText)
    {
        UIPasswordInput.Text(newText);
    }
}
