using DevToys.Tools.Helpers;
using DevToys.Tools.SmartDetection;
using DevToys.Tools.Strings.GlobalStrings;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DevToys.Tools.Tools.Graphic.ColorBlindnessSimulator;

[Export(typeof(ICommandLineTool))]
[Name("ColorBlindnessSimulator")]
[CommandName(
    Name = "colorblindsimulator",
    Alias = "cbs",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Graphic.ColorBlindnessSimulator.ColorBlindnessSimulator",
    DescriptionResourceName = nameof(ColorBlindnessSimulator.Description))]
internal sealed class ColorBlindnessSimulatorCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(ColorBlindnessSimulator.InputOptionDescription))]
    internal FileInfo Input { get; set; } = null!;

    [CommandLineOption(
        Name = "outputDirectory",
        Alias = "o",
        DescriptionResourceName = nameof(ColorBlindnessSimulator.OutputDirectoryOptionDescription))]
    internal DirectoryInfo? OutputFile { get; set; }

    [CommandLineOption(
    Name = "silent",
    Alias = "s",
    DescriptionResourceName = nameof(ColorBlindnessSimulator.SilentOptionDescription))]
    internal bool Silent { get; set; } = false;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (Input is null)
        {
            return -1;
        }

        if (!Input.Exists)
        {
            Console.Error.WriteLine(string.Format(GlobalStrings.FileNotFound, Input.FullName));
            return -1;
        }

        string fileExtension = Input.Extension;
        bool isSupportedImageFile = StaticImageFileDataTypeDetector.SupportedFileTypes.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        if (!isSupportedImageFile)
        {
            Console.Error.WriteLine(
                string.Format(
                    GlobalStrings.FileTypeNotSupported,
                    Input.FullName,
                    string.Join(", ", StaticImageFileDataTypeDetector.SupportedFileTypes)));
            return -1;
        }

        await SimulateAsync(cancellationToken);

        return 0;
    }

    private async Task SimulateAsync(CancellationToken cancellationToken)
    {
        string protanopiaOutputFilePath = string.Empty;
        string tritanopiaOutputFilePath = string.Empty;
        string deuteranopiaOutputFilePath = string.Empty;

        ConsoleProgressBar? progressBar = Silent ? null : new ConsoleProgressBar();
        using (progressBar)
        {
            int protanopiaProgress = 0;
            int tritanopiaProgress = 0;
            int deuteranopiaProgress = 0;

            using Stream fileStream = Input.OpenRead();
            using Image<Rgba32> originalImage = await Image.LoadAsync<Rgba32>(fileStream, cancellationToken);

            var simulationTasks
                = new List<Task<Image<Rgba32>?>>
                {
                    Task.Run(() =>
                        ColorBlindnessSimulatorHelper.SimulateColorBlindness(
                            originalImage,
                            ColorBlindnessMode.Protanopia,
                            (progress) =>
                            {
                                protanopiaProgress = progress;
                                UpdateProgress();
                            },
                            cancellationToken)),

                    Task.Run(() =>
                        ColorBlindnessSimulatorHelper.SimulateColorBlindness(
                            originalImage,
                            ColorBlindnessMode.Tritanopia,
                            (progress) =>
                            {
                                tritanopiaProgress = progress;
                                UpdateProgress();
                            },
                            cancellationToken)),

                    Task.Run(() =>
                        ColorBlindnessSimulatorHelper.SimulateColorBlindness(
                            originalImage,
                            ColorBlindnessMode.Deuteranopia,
                            (progress) =>
                            {
                                deuteranopiaProgress = progress;
                                UpdateProgress();
                            },
                            cancellationToken)),
                };

            await Task.WhenAll(simulationTasks);

            using Image<Rgba32>? protanopiaImage = simulationTasks[0].Result;
            using Image<Rgba32>? tritanopiaImage = simulationTasks[1].Result;
            using Image<Rgba32>? deuteranopiaImage = simulationTasks[2].Result;

            string outputFileName = Path.GetFileNameWithoutExtension(Input.Name);
            string outputDirectoryPath = OutputFile is null ? Path.GetDirectoryName(Input.FullName)! : OutputFile.FullName;
            Directory.CreateDirectory(outputDirectoryPath);
            string outputExtension = Path.GetExtension(Input.Name);

            protanopiaOutputFilePath = Path.Combine(outputDirectoryPath, outputFileName + "-protanopia" + outputExtension);
            tritanopiaOutputFilePath = Path.Combine(outputDirectoryPath, outputFileName + "-tritanopia" + outputExtension);
            deuteranopiaOutputFilePath = Path.Combine(outputDirectoryPath, outputFileName + "-deuteranopia" + outputExtension);

            if (protanopiaImage is not null)
            {
                progressBar?.Report(98);
                await protanopiaImage.SaveAsync(protanopiaOutputFilePath, cancellationToken);
            }

            if (tritanopiaImage is not null)
            {
                progressBar?.Report(99);
                await tritanopiaImage.SaveAsync(tritanopiaOutputFilePath, cancellationToken);
            }

            if (deuteranopiaImage is not null)
            {
                progressBar?.Report(100);
                await deuteranopiaImage.SaveAsync(deuteranopiaOutputFilePath, cancellationToken);
            }

            void UpdateProgress()
            {
                progressBar?.Report(Math.Min((protanopiaProgress + tritanopiaProgress + deuteranopiaProgress) / 3, 97));
            }
        }

        Console.WriteLine(protanopiaOutputFilePath);
        Console.WriteLine(tritanopiaOutputFilePath);
        Console.WriteLine(deuteranopiaOutputFilePath);
    }
}
