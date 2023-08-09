using Microsoft.AspNetCore.Components.Forms;

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

        if (!UIFileSelector.CanSelectManyFiles)
        {
            if (droppedFiles.Count == 1 && !string.IsNullOrEmpty(droppedFiles[0].Name))
            {
                string[] fileTypes = GetTreatedFileExtensions();
                if (fileTypes.Any(fileType => string.Equals(("." + fileType), Path.GetExtension(droppedFiles[0].Name), StringComparison.OrdinalIgnoreCase)))
                {
                    UIFileSelector.WithFiles(new PickedFile(droppedFiles[0].Name, droppedFiles[0].OpenReadStream(maxAllowedSize: long.MaxValue)));
                    return;
                }
            }

            HasInvalidFilesSelected = true;
            return;
        }
        else
        {
            var pickedFiles = new List<PickedFile>();
            string[] fileTypes = GetTreatedFileExtensions();

            for (int i = 0; i < droppedFiles.Count; i++)
            {
                IBrowserFile droppedFile = droppedFiles[i];
                if (!string.IsNullOrEmpty(droppedFile.Name))
                {
                    if (fileTypes.Any(fileType => string.Equals(("." + fileType), Path.GetExtension(droppedFile.Name), StringComparison.OrdinalIgnoreCase)))
                    {
                        pickedFiles.Add(new PickedFile(droppedFile.Name, droppedFile.OpenReadStream(maxAllowedSize: long.MaxValue)));
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
    }

    private async Task OnBrowseFilesButtonClickAsync()
    {
        string[] fileTypes = GetTreatedFileExtensions();

        PickedFile[] pickedFiles;
        if (UIFileSelector.CanSelectManyFiles)
        {
            pickedFiles = await _fileStorage.PickOpenFilesAsync(fileTypes);
        }
        else
        {
            PickedFile? pickedFile = await _fileStorage.PickOpenFileAsync(fileTypes);
            if (pickedFile != null)
            {
                pickedFiles = new[] { pickedFile };
            }
            else
            {
                pickedFiles = Array.Empty<PickedFile>();
            }
        }

        if (pickedFiles.Length > 0)
        {
            UIFileSelector.WithFiles(pickedFiles);
        }
    }

    private async Task OnBrowseFoldersButtonClickAsync()
    {
        var files = new List<PickedFile>();
        string? selectedFolder = await _fileStorage.PickFolderAsync();

        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            if (UIFileSelector.AllowedFileExtensions is null || UIFileSelector.AllowedFileExtensions.Length == 0)
            {
                foreach (string filePath in Directory.GetFiles(selectedFolder, "*", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(filePath);
                    files.Add(new(info.Name, info.OpenRead()));
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
                        files.Add(new(info.Name, info.OpenRead()));
                    }
                }
            }
        }

        if (files.Count > 0)
        {
            UIFileSelector.WithFiles(files.ToArray());
        }
    }

    private async Task OnPasteButtonClickAsync()
    {
        // TODO: Support image (format tbd) + file list.
        await _clipboard.GetClipboardDataAsync();
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

    private void UIFileSelector_CanSelectManyFilesChanged(object? sender, EventArgs e)
    {
        UpdateInstructionStrings();
    }

    private void UIFileSelector_AllowedFileExtensionsChanged(object? sender, EventArgs e)
    {
        UpdateInstructionStrings();
    }
}
