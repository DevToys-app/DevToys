using DevToys.Tools.Tools.EncodersDecoders.Base64Text;
using static DevToys.Tools.Tools.EncodersDecoders.Jwt.JwtEncoderDecoderGuiTool;

namespace DevToys.Tools.Tools.EncodersDecoders.Jwt;

internal sealed partial class JwtDecoderGuiTool
{
    private enum GridRows
    {
        Settings
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("base64-text-input-box");

    public IUIGridCell GridCell
        => Cell(
                JwtGridRows.SubContainer,
                GridColumns.Stretch,

                _inputText
                    .Title(Base64TextEncoderDecoder.InputTitle)
        );
}
