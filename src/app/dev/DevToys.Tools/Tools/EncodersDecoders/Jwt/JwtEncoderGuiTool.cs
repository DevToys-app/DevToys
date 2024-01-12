namespace DevToys.Tools.Tools.EncodersDecoders.Jwt;

internal sealed partial class JwtEncoderGuiTool
{
    private enum GridRows
    {
        Settings
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly IUIGridCell _encodeCell = Cell(JwtGridRows.SubContainer, GridColumns.Stretch);

    private readonly IUIStack _viewStack = Stack("jwt-encode-view-stack");

    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("base64-text-output-box");

    public IUIStack ViewStack()
        => _viewStack
        .Vertical()
        .WithChildren(
            _outputText
            .Title(JwtEncoderDecoder.TokenInputTitle)
        );

    public void Show()
        => _viewStack.Show();

    public void Hide()
        => _viewStack.Hide();
}
