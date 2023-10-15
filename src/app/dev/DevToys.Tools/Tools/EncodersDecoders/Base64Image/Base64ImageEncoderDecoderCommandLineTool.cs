using DevToys.Tools.Strings.GlobalStrings;
using Microsoft.Extensions.Logging;

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
    private static readonly string[] supportedImageExtensions
        = new string[]
        {
            ".bmp",
            ".gif",
            ".ico",
            ".jpg",
            ".jpeg",
            ".png",
            ".svg",
            ".webp"
        };

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(Base64ImageEncoderDecoder.InputOptionDescription))]
    private AnyType<FileInfo, string> Input { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(Base64ImageEncoderDecoder.OutputFileOptionDescription))]
    private FileInfo? OutputFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (Input.TryGetFirst(out FileInfo? file))
        {
            if (!file.Exists)
            {
                Console.Error.WriteLine(string.Format(GlobalStrings.FileNotFound, file.FullName));
                return -1;
            }

            bool isImageFile = supportedImageExtensions.Any(ext => ext.Equals(file.Extension, StringComparison.OrdinalIgnoreCase));
            if (isImageFile)
            {
                await EncodeAsync(file, cancellationToken);
            }
            else
            {
                string base64 = await File.ReadAllTextAsync(file.FullName, cancellationToken);
                return await DecodeAsync(base64, cancellationToken);
            }
        }
        else if (Input.TryGetSecond(out string? base64))
        {
            return await DecodeAsync(base64, cancellationToken);
        }

        return 0;
    }

    public async Task EncodeAsync(FileInfo file, CancellationToken cancellationToken)
    {
        using var fileStream
            = new FileStream(
                file.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
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

    public async ValueTask<int> DecodeAsync(string input, CancellationToken cancellationToken)
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
                fileStream = await _fileStorage.PickSaveFileAsync(supportedImageExtensions);
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
