﻿using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Url;

[Export(typeof(ICommandLineTool))]
[Name("UrlEncoderDecoder")]
[CommandName(
    Name = "url",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Url.UrlEncoderDecoder",
    DescriptionResourceName = nameof(UrlEncoderDecoder.Description))]
internal sealed class UrlEncoderDecoderCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(UrlEncoderDecoder.InputOptionDescription))]
    private string? Input { get; set; }

    [CommandLineOption(
        Name = "conversion",
        Alias = "c",
        DescriptionResourceName = nameof(UrlEncoderDecoder.ConversionOptionDescription))]
    private EncodingConversion EncodingConversionMode { get; set; } = EncodingConversion.Encode;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        string output;
        switch (EncodingConversionMode)
        {
            case EncodingConversion.Encode:
                output = UrlHelper.EncodeUrlData(Input, logger, cancellationToken);
                break;

            case EncodingConversion.Decode:
                output = UrlHelper.EncodeUrlData(Input, logger, cancellationToken);
                break;

            default:
                throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine(output);

        return new ValueTask<int>(0);
    }
}
