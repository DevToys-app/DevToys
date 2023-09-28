using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Base64Image;

[Export(typeof(ICommandLineTool))]
[Name("Base64ImageEncoderDecoder")]
[CommandName(
    Name = "base64",
    Alias = "b64",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Base64Image.Base64ImageEncoderDecoder",
    DescriptionResourceName = nameof(Base64ImageEncoderDecoder.Description))]
internal sealed class Base64ImageEncoderDecoderCommandLineTool : ICommandLineTool
{
    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        return new ValueTask<int>(0);
    }
}
