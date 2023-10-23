using DevToys.Tools.SmartDetection;
using DevToys.Tools.Strings.GlobalStrings;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.EncodersDecoders.Base64Image;

[Export(typeof(ICommandLineTool))]
[Name("Base64ImageEncoderDecoder")]
[CommandName(
    Name = "base64img",
    Alias = "b64i",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Base64Image.Base64ImageEncoderDecoder",
    DescriptionResourceName = nameof(Base64ImageEncoderDecoder.Description))]
internal sealed partial class Base64ImageEncoderDecoderCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(Base64ImageEncoderDecoder.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(Base64ImageEncoderDecoder.OutputFileOptionDescription))]
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

                string fileExtension = file.Extension.Trim('.');
                bool isImageFile = Base64ImageFileDataTypeDetector.SupportedFileTypes.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
                if (isImageFile)
                {
                    await EncodeAsync(file, cancellationToken);
                }
                else
                {
                    string base64 = await File.ReadAllTextAsync(file.FullName, cancellationToken);
                    return await DecodeAsync(base64, cancellationToken);
                }

                return 0;
            },
            async base64 => await DecodeAsync(base64, cancellationToken));
    }

    private async Task EncodeAsync(FileInfo file, CancellationToken cancellationToken)
    {
        using Stream fileStream = _fileStorage.OpenReadFile(file.FullName);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);

        byte[] bytes = memoryStream.ToArray();
        string base64 = Convert.ToBase64String(bytes);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        string output
            = file.Extension.ToLowerInvariant() switch
            {
                ".bmp" => "data:image/bmp;base64," + base64,
                ".gif" => "data:image/gif;base64," + base64,
                ".ico" => "data:image/x-icon;base64," + base64,
                ".jpg" or ".jpeg" => "data:image/jpeg;base64," + base64,
                ".png" => "data:image/png;base64," + base64,
                ".svg" => "data:image/svg+xml;base64," + base64,
                ".webp" => "data:image/webp;base64," + base64,
                _ => throw new NotSupportedException(),
            };

        if (OutputFile is null)
        {
            Console.WriteLine(output);
        }
        else
        {
            await File.WriteAllTextAsync(OutputFile.FullName, output, cancellationToken);
        }
    }

    private async ValueTask<int> DecodeAsync(string input, CancellationToken cancellationToken)
    {
        Stream? fileStream = null;
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return -1;
            }

            string trimmedData = input.Trim();

            input = trimmedData.Substring(trimmedData.IndexOf(',') + 1);
            byte[] bytes = Convert.FromBase64String(input);

            if (OutputFile is null)
            {
                fileStream = await _fileStorage.PickSaveFileAsync(Base64ImageFileDataTypeDetector.SupportedFileTypes);
                if (fileStream is null)
                {
                    return -1;
                }
            }
            else
            {
                if (OutputFile.Exists)
                {
                    File.Delete(OutputFile.FullName);
                }

                fileStream = OutputFile.OpenWrite();
            }

            await fileStream.WriteAsync(bytes, cancellationToken);
            return 0;
        }
        catch
        {
            return -1;
        }
        finally
        {
            fileStream?.Dispose();
        }
    }
}
