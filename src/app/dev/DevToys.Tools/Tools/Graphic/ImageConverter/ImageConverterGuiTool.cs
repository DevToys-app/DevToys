using System.Collections.Specialized;
using DevToys.Tools.Models;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.Tools.Tools.Graphic.ImageConverter;

[Export(typeof(IGuiTool))]
[Name("ImageConverter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0127',
    GroupName = PredefinedCommonToolGroupNames.Graphic,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Graphic.ImageConverter.ImageConverter",
    ShortDisplayTitleResourceName = nameof(ImageConverter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(ImageConverter.LongDisplayTitle),
    DescriptionResourceName = nameof(ImageConverter.Description),
    AccessibleNameResourceName = nameof(ImageConverter.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Image)]
[AcceptedDataTypeName("StaticImageFile")]
[AcceptedDataTypeName("StaticImageFiles")]
internal sealed class ImageConverterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// The image format to convert into.
    /// </summary>
    private static readonly SettingDefinition<ImageConverterSupportedFormat> targetImageFormat
        = new(
            name: $"{nameof(ImageConverterGuiTool)}.{nameof(targetImageFormat)}",
            defaultValue: ImageConverterSupportedFormat.PNG);

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IFileStorage _fileStorage;
    private readonly IUIList _itemList = List("image-converter-task-list");
    private readonly IUIButton _saveAllButton = Button("image-converter-save-all-button");
    private readonly IUIButton _deleteAllButton = Button("image-converter-delete-all-button");

    [ImportingConstructor]
    public ImageConverterGuiTool(ISettingsProvider settingsProvider, IFileStorage fileStorage)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        _fileStorage = fileStorage;

        _itemList.Items.CollectionChanged += OnItemListItemsChanged;
    }

    internal ImageConverterSupportedFormat UserSelectedImageFormat => _settingsProvider.GetSetting(targetImageFormat);

    internal IUIList ItemList => _itemList;

    public UIToolView View
        => new(
            isScrollable: true,
            Stack()
                .Vertical()
                .LargeSpacing()

                .WithChildren(
                    Stack()
                        .Vertical()
                        .SmallSpacing()

                        .WithChildren(
                            Label()
                                .Text(ImageConverter.ConfigurationTitle),

                            Setting("image-converter-target-format-type-setting")
                                .Icon("FluentSystemIcons", '\uF18D')
                                .Title(ImageConverter.ConversionTitle)
                                .Description(ImageConverter.ConvertedFormatDescription)

                                .Handle(
                                    _settingsProvider,
                                    targetImageFormat,
                                    onOptionSelected: null,
                                    Item(ImageConverterSupportedFormat.BMP),
                                    Item(ImageConverterSupportedFormat.JPEG),
                                    Item(ImageConverterSupportedFormat.PBM),
                                    Item(ImageConverterSupportedFormat.PNG),
                                    Item(ImageConverterSupportedFormat.TGA),
                                    Item(ImageConverterSupportedFormat.TIFF),
                                    Item(ImageConverterSupportedFormat.WEBP))),

                    FileSelector()
                        .CanSelectManyFiles()
                        .LimitFileTypesTo(StaticImageFileDataTypeDetector.SupportedFileTypes)
                        .OnFilesSelected(OnFilesSelected),

                    Stack()
                        .Horizontal()
                        .AlignHorizontally(UIHorizontalAlignment.Right)
                        .MediumSpacing()

                        .WithChildren(
                            _saveAllButton
                                .Icon("FluentSystemIcons", '\uF67F')
                                .Text(ImageConverter.SaveAll)
                                .AccentAppearance()
                                .Disable()
                                .OnClick(OnSaveAllAsync),

                            _deleteAllButton
                                .Icon("FluentSystemIcons", '\uF34C')
                                .Text(ImageConverter.DeleteAll)
                                .Disable()
                                .OnClick(OnDeleteAllAsync)),

                    _itemList
                        .ForbidSelectItem()));

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

            _itemList.Items.Insert(0, new ImageConverterTaskItemGui(this, SandboxedFileReader.FromFileInfo(temporaryFile), _fileStorage));
        }
        else if (dataTypeName == "StaticImageFile" && parsedData is FileInfo file)
        {
            _itemList.Items.Insert(0, new ImageConverterTaskItemGui(this, SandboxedFileReader.FromFileInfo(file), _fileStorage));
        }
        else if (dataTypeName == "StaticImageFiles" && parsedData is FileInfo[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                _itemList.Items.Insert(0, new ImageConverterTaskItemGui(this, SandboxedFileReader.FromFileInfo(files[i]), _fileStorage));
            }
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < _itemList.Items.Count; i++)
        {
            if (_itemList.Items[i] is IDisposable disposableItem)
            {
                disposableItem.Dispose();
            }
        }

        _itemList.Items.CollectionChanged -= OnItemListItemsChanged;
    }

    private void OnFilesSelected(SandboxedFileReader[] files)
    {
        for (int i = 0; i < files.Length; i++)
        {
            _itemList.Items.Insert(
                0,
                new ImageConverterTaskItemGui(this, files[i], _fileStorage));
        }
    }

    private async ValueTask OnSaveAllAsync()
    {
        string? folderPath = await _fileStorage.PickFolderAsync();

        if (!string.IsNullOrEmpty(folderPath))
        {
            ImageConverterSupportedFormat targetFormat = UserSelectedImageFormat;
            for (int i = 0; i < _itemList.Items.Count; i++)
            {
                if (_itemList.Items[i] is ImageConverterTaskItemGui item)
                {
                    string newFileName = Path.GetFileNameWithoutExtension(item.InputFile.FileName) + "." + targetFormat.ToString().ToLower();
                    string newFilePath = Path.Combine(folderPath, newFileName);
                    await item.SaveAsync(newFilePath, targetFormat);
                }
            }
        }
    }

    private ValueTask OnDeleteAllAsync()
    {
        for (int i = 0; i < _itemList.Items.Count; i++)
        {
            if (_itemList.Items[i] is IDisposable disposableItem)
            {
                disposableItem.Dispose();
            }
        }

        _itemList.Items.Clear();

        return ValueTask.CompletedTask;
    }

    private void OnItemListItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        bool hasItems = _itemList.Items.Count > 0;
        if (hasItems)
        {
            _saveAllButton.Enable();
            _deleteAllButton.Enable();
        }
        else
        {
            _saveAllButton.Disable();
            _deleteAllButton.Disable();
        }
    }
}
