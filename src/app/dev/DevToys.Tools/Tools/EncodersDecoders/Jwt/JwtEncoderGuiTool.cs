using DevToys.Tools.Tools.EncodersDecoders.Base64Text;
using static DevToys.Tools.Tools.EncodersDecoders.Jwt.JwtEncoderDecoderGuiTool;

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

    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("base64-text-output-box");

    public IUIGridCell GridCell
        => Cell(
                JwtGridRows.SubContainer,
                GridColumns.Stretch,

                _outputText
                    .Title(Base64TextEncoderDecoder.OutputTitle)
        );
}
