using DevToys.Tools.Helpers;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DevToys.Tools.Tools.Graphic.ColorBlindnessSimulator;

[Export(typeof(IGuiTool))]
[Name("ColorBlindnessSimulator")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0101',
    GroupName = PredefinedCommonToolGroupNames.Graphic,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Graphic.ColorBlindnessSimulator.ColorBlindnessSimulator",
    ShortDisplayTitleResourceName = nameof(ColorBlindnessSimulator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(ColorBlindnessSimulator.LongDisplayTitle),
    DescriptionResourceName = nameof(ColorBlindnessSimulator.Description),
    SearchKeywordsResourceName = nameof(ColorBlindnessSimulator.SearchKeywords),
    AccessibleNameResourceName = nameof(ColorBlindnessSimulator.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Image)]
[AcceptedDataTypeName("StaticImageFile")]
internal sealed class ColorBlindnessSimulatorGuiTool : IGuiTool, IDisposable
{
    private enum GridRows
    {
        FileInput,
        FirstLineResult,
        SecondLineResult
    }

    private enum GridColumns
    {
        Left,
        Right
    }

    private readonly object _lock = new();
    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly IFileStorage _fileStorage;
    private readonly IUIFileSelector _fileSelector = FileSelector();
    private readonly IUIImageViewer _originalImageViewer = ImageViewer();
    private readonly IUIImageViewer _protanopiaImageViewer = ImageViewer();
    private readonly IUIImageViewer _tritanopiaImageViewer = ImageViewer();
    private readonly IUIImageViewer _deuteranopiaImageViewer = ImageViewer();
    private readonly IUIStack _progressStack = Stack();
    private readonly IUIProgressBar _progressBar = ProgressBar();

    private CancellationTokenSource? _cancellationTokenSource;
    private SandboxedFileReader? _selectedFile;

    [ImportingConstructor]
    public ColorBlindnessSimulatorGuiTool(IFileStorage fileStorage)
    {
        _logger = this.Log();
        _fileStorage = fileStorage;
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: false,
            Grid()
                .RowLargeSpacing()
                .ColumnLargeSpacing()

                .Rows(
                    (GridRows.FileInput, Auto),
                    (GridRows.FirstLineResult, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridRows.SecondLineResult, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Columns(
                    (GridColumns.Left, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridColumns.Right, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.FileInput,
                        GridRows.FileInput,
                        GridColumns.Left,
                        GridColumns.Right,

                        _fileSelector
                            .CanSelectOneFile()
                            .LimitFileTypesTo(StaticImageFileDataTypeDetector.SupportedFileTypes)
                            .OnFilesSelected(OnFilesSelected)),

                    Cell(
                        GridRows.FirstLineResult,
                        GridColumns.Left,

                        _originalImageViewer
                            .Title(ColorBlindnessSimulator.OriginalPicture)
                            .Hide()),

                    Cell(
                        GridRows.FirstLineResult,
                        GridColumns.Right,

                        _protanopiaImageViewer
                            .Title(ColorBlindnessSimulator.ProtanopiaSimulation)
                            .Hide()),

                    Cell(
                        GridRows.SecondLineResult,
                        GridColumns.Left,

                        _tritanopiaImageViewer
                            .Title(ColorBlindnessSimulator.TritanopiaSimulation)
                            .Hide()),

                    Cell(
                        GridRows.SecondLineResult,
                        GridColumns.Right,

                        _deuteranopiaImageViewer
                            .Title(ColorBlindnessSimulator.DeuteranopiaSimulation)
                            .Hide()),

                    Cell(GridRows.FirstLineResult,
                         GridRows.SecondLineResult,
                         GridColumns.Left,
                         GridColumns.Right,

                         _progressStack
                            .AlignHorizontally(UIHorizontalAlignment.Center)
                            .AlignVertically(UIVerticalAlignment.Center)
                            .Horizontal()
                            .SmallSpacing()
                            .Hide()

                            .WithChildren(
                                _progressBar,
                                Button()
                                    .Icon("FluentSystemIcons", '\uF75A')
                                    .OnClick(CancelSimulation)))));

    public async void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Image && parsedData is Image<Rgba32> image)
        {
            FileInfo temporaryFile = _fileStorage.CreateSelfDestroyingTempFile("png");

            using (image)
            {
                using Stream fileStream = _fileStorage.OpenWriteFile(temporaryFile.FullName, replaceIfExist: true);
                await image.SaveAsPngAsync(fileStream);
            }

            _fileSelector.WithFiles(SandboxedFileReader.FromFileInfo(temporaryFile)); // This will trigger a new simulation.
        }
        else if (dataTypeName == "StaticImageFile" && parsedData is FileInfo file)
        {
            _fileSelector.WithFiles(SandboxedFileReader.FromFileInfo(file)); // This will trigger a new simulation.
        }
    }

    public void Dispose()
    {
        _selectedFile?.Dispose();
        CancelSimulation();
        _semaphore.Dispose();
    }

    private void OnFilesSelected(SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);

        _selectedFile?.Dispose();
        _selectedFile = files[0];

        CancelSimulation();

        _cancellationTokenSource = new CancellationTokenSource();
        WorkTask = SimulateAsync(_selectedFile, _cancellationTokenSource.Token);
    }

    private void CancelSimulation()
    {
        lock (_lock)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch
            {
                // Ignore.
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
            }

            _originalImageViewer.Clear();
            _protanopiaImageViewer.Clear();
            _tritanopiaImageViewer.Clear();
            _deuteranopiaImageViewer.Clear();
            _originalImageViewer.Hide();
            _protanopiaImageViewer.Hide();
            _tritanopiaImageViewer.Hide();
            _deuteranopiaImageViewer.Hide();

            _progressBar.Progress(0);
            _progressStack.Hide();
        }
    }

    private async Task SimulateAsync(SandboxedFileReader file, CancellationToken cancellationToken)
    {
        _progressStack.Show();

        int protanopiaProgress = 0;
        int tritanopiaProgress = 0;
        int deuteranopiaProgress = 0;

        using Stream fileStream = await file.GetNewAccessToFileContentAsync(cancellationToken);
        Image<Rgba32> originalImage = await Image.LoadAsync<Rgba32>(fileStream, cancellationToken);

        var simulationTasks
            = new List<Task<Image<Rgba32>?>>
            {
                SimulateAsync(
                    originalImage,
                    ColorBlindnessMode.Protanopia,
                    (progress) =>
                    {
                        protanopiaProgress = progress;
                        UpdateProgress();
                    },
                    cancellationToken),

                SimulateAsync(
                    originalImage,
                    ColorBlindnessMode.Tritanopia,
                    (progress) =>
                    {
                        tritanopiaProgress = progress;
                        UpdateProgress();
                    },
                    cancellationToken),

                SimulateAsync(
                    originalImage,
                    ColorBlindnessMode.Deuteranopia,
                    (progress) =>
                    {
                        deuteranopiaProgress = progress;
                        UpdateProgress();
                    },
                    cancellationToken),
            };

        await Task.WhenAll(simulationTasks);

        lock (_lock)
        {
            _progressStack.Hide();

            if (cancellationToken.IsCancellationRequested)
            {
                if (simulationTasks[0].Result is not null)
                {
                    simulationTasks[0].Result!.Dispose();
                }
                if (simulationTasks[1].Result is not null)
                {
                    simulationTasks[1].Result!.Dispose();
                }
                if (simulationTasks[2].Result is not null)
                {
                    simulationTasks[2].Result!.Dispose();
                }

                return;
            }

            Guard.IsNotNull(simulationTasks[0].Result);
            Guard.IsNotNull(simulationTasks[1].Result);
            Guard.IsNotNull(simulationTasks[2].Result);
            Image<Rgba32> protanopiaImage = simulationTasks[0].Result!;
            Image<Rgba32> tritanopiaImage = simulationTasks[1].Result!;
            Image<Rgba32> deuteranopiaImage = simulationTasks[2].Result!;

            _originalImageViewer.WithImage(originalImage, disposeAutomatically: true).Show();
            _protanopiaImageViewer.WithImage(protanopiaImage, disposeAutomatically: true).Show();
            _tritanopiaImageViewer.WithImage(tritanopiaImage, disposeAutomatically: true).Show();
            _deuteranopiaImageViewer.WithImage(deuteranopiaImage, disposeAutomatically: true).Show();
        }

        void UpdateProgress()
        {
            _progressBar.Progress((protanopiaProgress + tritanopiaProgress + deuteranopiaProgress) / 3);
        }
    }

    private static async Task<Image<Rgba32>?> SimulateAsync(
        Image<Rgba32> originalImage,
        ColorBlindnessMode colorBlindnessMode,
        Action<int> progressReport,
        CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        return ColorBlindnessSimulatorHelper.SimulateColorBlindness(originalImage, colorBlindnessMode, progressReport, cancellationToken);
    }
}
