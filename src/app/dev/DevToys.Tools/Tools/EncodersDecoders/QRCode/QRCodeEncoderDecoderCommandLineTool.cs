using DevToys.Tools.Helpers;
using DevToys.Tools.SmartDetection;
using DevToys.Tools.Strings.GlobalStrings;
using Microsoft.Extensions.Logging;
using OneOf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.Tools.Tools.EncodersDecoders.QRCode;

[Export(typeof(ICommandLineTool))]
[Name("QRCodeEncoderDecoder")]
[CommandName(
    Name = "qrcode",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.QRCode.QRCodeEncoderDecoder",
    DescriptionResourceName = nameof(QRCodeEncoderDecoder.Description))]
internal class QRCodeEncoderDecoderCommandLineTool : ICommandLineTool
{
    private readonly Lazy<string[]> _supportedFileTypes
        = new(() =>
        {
            var extensions = new List<string>(StaticImageFileDataTypeDetector.SupportedFileTypes);
            extensions.Add(".svg");
            return extensions.ToArray();
        });

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(QRCodeEncoderDecoder.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(QRCodeEncoderDecoder.OutputFileOptionDescription))]
    internal FileInfo? OutputFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            return -1;
        }

        return await Input.Value.Match(
            async file =>
            {
                if (!file.Exists)
                {
                    Console.Error.WriteLine(string.Format(GlobalStrings.FileNotFound, file.FullName));
                    return -1;
                }

                string fileExtension = file.Extension;
                bool isImageFile = StaticImageFileDataTypeDetector.SupportedFileTypes.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
                if (isImageFile)
                {
                    await DecodeQRCodeAsync(file, cancellationToken);
                }
                else
                {
                    string text = await File.ReadAllTextAsync(file.FullName, cancellationToken);
                    return await EncodeQRCodeAsync(text, cancellationToken);
                }

                return 0;
            },
            async text => await EncodeQRCodeAsync(text, cancellationToken));
    }

    private async ValueTask<int> EncodeQRCodeAsync(string input, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return -1;
            }

            string filePath;
            if (OutputFile is null)
            {
                using FileStream? fileStream = await _fileStorage.PickSaveFileAsync(_supportedFileTypes.Value);
                if (fileStream is null)
                {
                    return -1;
                }

                filePath = fileStream.Name;
            }
            else
            {
                filePath = OutputFile.FullName;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return -1;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (Path.GetExtension(filePath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
            {
                string svg = QrCodeHelper.GenerateSvgQrCode(input);
                using var fileWriter = new StreamWriter(filePath);
                await fileWriter.WriteAsync(svg);
            }
            else
            {
                using Image<Rgba32> image = QrCodeHelper.GenerateQrCode(input);
                await image.SaveAsync(filePath, cancellationToken);
            }

            return 0;
        }
        catch
        {
            return -1;
        }
        finally
        {
        }
    }

    private async ValueTask<int> DecodeQRCodeAsync(FileInfo file, CancellationToken cancellationToken)
    {
        using Stream stream = _fileStorage.OpenReadFile(file.FullName);

        string output = await QrCodeHelper.ReadQrCodeAsync(stream, cancellationToken);

        if (string.IsNullOrEmpty(output))
        {
            Console.Error.WriteLine(QRCodeEncoderDecoder.NoQrCodeFound);
            return -1;
        }

        await FileHelper.WriteOutputAsync(output, OutputFile, cancellationToken);
        return 0;
    }
}
