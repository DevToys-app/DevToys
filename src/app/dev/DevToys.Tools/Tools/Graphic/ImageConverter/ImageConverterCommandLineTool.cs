using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;
using OneOf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.Tools.Tools.Graphic.ImageConverter;

[Export(typeof(ICommandLineTool))]
[Name("ImageConverter")]
[CommandName(
    Name = "imageconverter",
    Alias = "imgconv",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Graphic.ImageConverter.ImageConverter",
    DescriptionResourceName = nameof(ImageConverter.Description))]
internal sealed class ImageConverterCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(ImageConverter.InputOptionDescription))]
    internal OneOf<DirectoryInfo, FileInfo>[] Input { get; set; } = null!;

    [CommandLineOption(
        Name = "type",
        Alias = "t",
        IsRequired = true,
        DescriptionResourceName = nameof(ImageConverter.FormatOptionDescription))]
    internal ImageConverterSupportedFormat Format { get; set; }

    [CommandLineOption(
        Name = "output",
        Alias = "o",
        DescriptionResourceName = nameof(ImageConverter.OutputDirectoryOptionDescription))]
    internal OneOf<DirectoryInfo, FileInfo>? Output { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            if (Input is null || Input.Length == 0)
            {
                return -1;
            }

            if (Input.Length >= 1)
            {
                if (Output.HasValue && Output.Value.IsT1)
                {
                    Console.Error.WriteLine("Output must be a directory when passing more than one file as input.");
                    return -1;
                }
            }

            for (int i = 0; i < Input.Length; i++)
            {
                await TreatDirectoryOrFileAsync(Input[i], logger, cancellationToken);
            }

            return 0;
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            return -1;
        }
        catch
        {
            // TODO: Log exception.
            return -1;
        }
    }

    private async Task TreatDirectoryOrFileAsync(OneOf<DirectoryInfo, FileInfo> directoryOrFile, ILogger logger, CancellationToken cancellationToken)
    {
        if (directoryOrFile.IsT0)
        {
            foreach (FileInfo fileInDirectory in directoryOrFile.AsT0.EnumerateFiles())
            {
                await TreatDirectoryOrFileAsync(fileInDirectory, logger, cancellationToken);
            }
        }
        else if (directoryOrFile.IsT1)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (StaticImageFileDataTypeDetector.SupportedFileTypes.Contains(directoryOrFile.AsT1.Extension, StringComparer.OrdinalIgnoreCase))
            {
                await ConvertAsync(directoryOrFile.AsT1, cancellationToken);
            }
        }
    }

    private async Task ConvertAsync(FileInfo fileToConvert, CancellationToken cancellationToken)
    {
        FileInfo outputFile;
        if (!Output.HasValue || Output.Value.IsT0)
        {
            string outputDirectoryPath;
            if (Output.HasValue)
            {
                outputDirectoryPath = Output.Value.AsT0.FullName;
            }
            else
            {
                outputDirectoryPath = Path.GetDirectoryName(fileToConvert.FullName)!;
            }

            string outputFileName = Path.GetFileNameWithoutExtension(fileToConvert.FullName) + "." + Format.ToString().ToLower();
            string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);
            outputFile = new FileInfo(outputFilePath);
        }
        else
        {
            outputFile = Output.Value.AsT1;
        }

        // Load the image first, in case if the user will overwrite it.
        using Image<Rgba32> originalImage = await ImageHelper.LoadImageFromFileAsync(fileToConvert, cancellationToken);

        using FileStream outputFileStream = File.Create(outputFile.FullName);
        await ImageHelper.SaveAsync(outputFileStream, originalImage, Format, cancellationToken);
    }
}
