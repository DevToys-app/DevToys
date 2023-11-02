using DevToys.Blazor.Core;
using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIFileSelectorPresenter : MefComponentBase
{
    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/UIElements/UIFileSelectorPresenter.razor.js";

    protected string ExtendedId => UIFileSelector.Id + "-" + Id;

    protected string InputFileId => UIFileSelector.Id + "-" + Id + "-InputFile";

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IClipboard _clipboard = default!;

    [Import]
    private IFileStorage _fileStorage = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    [Parameter]
    public IUIFileSelector UIFileSelector { get; set; } = default!;

    internal string DragDropInstructions { get; private set; } = string.Empty;

    internal string HasInvalidFilesSelectedIndication { get; private set; } = string.Empty;

    internal bool HasInvalidFilesSelected { get; private set; }

    internal bool HasFilesSelected { get; private set; }

    internal string SelectedFilesDescription { get; private set; } = string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UIFileSelector.AllowedFileExtensionsChanged += UIFileSelector_AllowedFileExtensionsChanged;
        UIFileSelector.CanSelectManyFilesChanged += UIFileSelector_CanSelectManyFilesChanged;

        UpdateInstructionStrings();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("registerDropZone", ExtendedId);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        UIFileSelector.AllowedFileExtensionsChanged -= UIFileSelector_AllowedFileExtensionsChanged;
        UIFileSelector.CanSelectManyFilesChanged -= UIFileSelector_CanSelectManyFilesChanged;
        await (await JSModule).InvokeVoidWithErrorHandlingAsync("dispose", ExtendedId);
        await base.DisposeAsync();
    }

    private void OnFileDropped(InputFileChangeEventArgs args)
    {
        if (!IsActuallyEnabled)
        {
            return;
        }

        IReadOnlyList<IBrowserFile> droppedFiles = args.GetMultipleFiles(maximumFileCount: int.MaxValue);

        if (droppedFiles.Count == 0)
        {
            return;
        }

        TreatSelectedFiles(droppedFiles.ToArray(), GetFileName, CreatePickedFile);
    }

    private async Task OnBrowseFilesButtonClickAsync()
    {
        string[] fileTypes = GetTreatedFileExtensions();

        SandboxedFileReader[] pickedFiles;
        if (UIFileSelector.CanSelectManyFiles)
        {
            pickedFiles = await _fileStorage.PickOpenFilesAsync(fileTypes);
        }
        else
        {
            SandboxedFileReader? pickedFile = await _fileStorage.PickOpenFileAsync(fileTypes);
            if (pickedFile != null)
            {
                pickedFiles = new[] { pickedFile };
            }
            else
            {
                pickedFiles = Array.Empty<SandboxedFileReader>();
            }
        }

        if (pickedFiles.Length > 0)
        {
            UIFileSelector.WithFiles(pickedFiles);
        }

        UpdateSelectedFilesDescription();
    }

    private async Task OnBrowseFoldersButtonClickAsync()
    {
        var files = new List<SandboxedFileReader>();
        string? selectedFolder = await _fileStorage.PickFolderAsync();

        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            if (UIFileSelector.AllowedFileExtensions is null || UIFileSelector.AllowedFileExtensions.Length == 0)
            {
                foreach (string filePath in Directory.GetFiles(selectedFolder, "*", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(filePath);
                    files.Add(new BlazorSandboxedFileReader(info, _fileStorage));
                }
            }
            else
            {
                string[] fileTypes = GetTreatedFileExtensions();
                foreach (string fileType in fileTypes)
                {
                    foreach (string filePath in Directory.GetFiles(selectedFolder, "*." + fileType, SearchOption.AllDirectories))
                    {
                        var info = new FileInfo(filePath);
                        files.Add(new BlazorSandboxedFileReader(info, _fileStorage));
                    }
                }
            }
        }

        if (files.Count > 0)
        {
            UIFileSelector.WithFiles(files.ToArray());
        }

        UpdateSelectedFilesDescription();
    }

    private async Task OnPasteButtonClickAsync()
    {
        FileInfo[]? files = await _clipboard.GetClipboardFilesAsync();
        if (files is not null && files.Length > 0)
        {
            TreatSelectedFiles(files, GetFileName, CreatePickedFile);
            return;
        }

        string[] fileTypes = GetTreatedFileExtensions();
        if (fileTypes.Contains("png", StringComparer.OrdinalIgnoreCase))
        {
            Image<SixLabors.ImageSharp.PixelFormats.Rgba32>? image = await _clipboard.GetClipboardImageAsync();
            if (image is not null)
            {
                FileInfo temporaryFile = _fileStorage.CreateSelfDestroyingTempFile("png");

                using (image)
                {
                    using Stream fileStream = _fileStorage.OpenWriteFile(temporaryFile.FullName, replaceIfExist: true);
                    await image.SaveAsPngAsync(fileStream);
                }

                var pickedFile = new BlazorSandboxedFileReader(temporaryFile, _fileStorage);

                UIFileSelector.WithFiles(pickedFile);
            }
        }

        UpdateSelectedFilesDescription();
    }

    private string[] GetTreatedFileExtensions()
    {
        if (UIFileSelector.AllowedFileExtensions is null)
        {
            return Array.Empty<string>();
        }

        return UIFileSelector.AllowedFileExtensions.Select(fileType => fileType.Trim('*').Trim('.').ToLower()).ToArray();
    }

    private void UpdateInstructionStrings()
    {
        string[] allowedFileExtensions = GetTreatedFileExtensions();

        if (allowedFileExtensions is null
            || allowedFileExtensions.Length == 0
            || (allowedFileExtensions.Length == 1 && string.IsNullOrEmpty(allowedFileExtensions[0])))
        {
            if (UIFileSelector.CanSelectManyFiles)
            {
                DragDropInstructions = DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorDragDropAnyFiles;
            }
            else
            {
                DragDropInstructions = DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorDragDropAnyFile;
            }
        }
        else if (allowedFileExtensions.Length == 1)
        {
            string extensionsString
                = allowedFileExtensions[0]
                .Replace(".", string.Empty)
                .ToUpperInvariant();

            if (UIFileSelector.CanSelectManyFiles)
            {
                DragDropInstructions
                    = string.Format(
                        DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorDragDropAnySpecificFiles,
                        extensionsString);
            }
            else
            {
                DragDropInstructions
                    = string.Format(
                        DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorDragDropAnySpecificFile,
                        extensionsString);
            }

            HasInvalidFilesSelectedIndication
                    = string.Format(
                        DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorInvalidSelectedFiles,
                        extensionsString);
        }
        else
        {
            string extensionsString
                = string.Join(", ", allowedFileExtensions.Order())
                .Replace(".", string.Empty)
                .ToUpperInvariant();

            if (UIFileSelector.CanSelectManyFiles)
            {
                DragDropInstructions
                    = string.Format(
                        DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorDragDropAnySpecificFiles,
                        extensionsString);
            }
            else
            {
                DragDropInstructions
                    = string.Format(
                        DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorDragDropAnySpecificFile,
                        extensionsString);
            }

            HasInvalidFilesSelectedIndication
                    = string.Format(
                        DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorInvalidSelectedFiles,
                        extensionsString);
        }
    }

    private void UpdateSelectedFilesDescription()
    {
        if (UIFileSelector.SelectedFiles is null || UIFileSelector.SelectedFiles.Length == 0)
        {
            SelectedFilesDescription = string.Empty;
            HasFilesSelected = false;
        }
        else if (UIFileSelector.SelectedFiles.Length == 1)
        {
            SelectedFilesDescription = UIFileSelector.SelectedFiles[0].FileName;
            HasFilesSelected = true;
        }
        else
        {
            SelectedFilesDescription
                = string.Format(
                    DevToys.Localization.Strings.UIFileSelectorPresenter.UIFileSelectorPresenter.FileSelectorMultipleFilesSelected,
                    UIFileSelector.SelectedFiles.Length);
            HasFilesSelected = true;
        }
    }

    private void UIFileSelector_CanSelectManyFilesChanged(object? sender, EventArgs e)
    {
        UpdateInstructionStrings();
    }

    private void UIFileSelector_AllowedFileExtensionsChanged(object? sender, EventArgs e)
    {
        UpdateInstructionStrings();
    }

    private void TreatSelectedFiles<T>(T[] files, Func<T, string> fileNameGetter, Func<T, SandboxedFileReader> pickedFileCreator)
    {
        string[] fileTypes = GetTreatedFileExtensions();
        if (UIFileSelector.CanSelectManyFiles)
        {
            var pickedFiles = new List<SandboxedFileReader>();

            for (int i = 0; i < files.Length; i++)
            {
                T file = files[i];
                string fileName = fileNameGetter(file);
                if (!string.IsNullOrEmpty(fileName))
                {
                    string fileExtension = Path.GetExtension(fileName);
                    string fileExtensionTrimmed = fileExtension.Trim('.');
                    if (fileTypes.Length == 0
                        || fileTypes.Any(fileType
                            => string.Equals(fileType, fileExtension, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(fileType, fileExtensionTrimmed, StringComparison.OrdinalIgnoreCase)))
                    {
                        pickedFiles.Add(pickedFileCreator(file));
                    }
                    else
                    {
                        HasInvalidFilesSelected = true;
                    }
                }
                else
                {
                    HasInvalidFilesSelected = true;
                }
            }

            if (pickedFiles.Count > 0)
            {
                UIFileSelector.WithFiles(pickedFiles.ToArray());
            }
            else
            {
                HasInvalidFilesSelected = true;
            }
        }
        else
        {
            if (files.Length == 1 && !string.IsNullOrEmpty(fileNameGetter(files[0])))
            {
                string fileExtension = Path.GetExtension(fileNameGetter(files[0]));
                string fileExtensionTrimmed = fileExtension.Trim('.');
                if (fileTypes.Length == 0
                        || fileTypes.Any(fileType
                            => string.Equals(fileType, fileExtension, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(fileType, fileExtensionTrimmed, StringComparison.OrdinalIgnoreCase)))
                {
                    UIFileSelector.WithFiles(pickedFileCreator(files[0]));
                    return;
                }
            }

            HasInvalidFilesSelected = true;
            return;
        }

        UpdateSelectedFilesDescription();
    }

    private string GetFileName(IBrowserFile browserFile)
    {
        Guard.IsNotNull(browserFile);
        return browserFile.Name;
    }

    private string GetFileName(FileInfo fileInfo)
    {
        Guard.IsNotNull(fileInfo);
        return fileInfo.Name;
    }

    private SandboxedFileReader CreatePickedFile(IBrowserFile browserFile)
    {
        Guard.IsNotNull(browserFile);
        return new BlazorSandboxedFileReader(browserFile);
    }

    private SandboxedFileReader CreatePickedFile(FileInfo fileInfo)
    {
        Guard.IsNotNull(fileInfo);
        return new BlazorSandboxedFileReader(fileInfo, _fileStorage);
    }
}
