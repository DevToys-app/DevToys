#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Core;
using DevToys.Views.Tools.ImageConverter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.ImageConverter
{
    [Export(typeof(ImageConverterToolViewModel))]
    public sealed class ImageConverterToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        private static readonly string[] SupportedFileExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };

        private bool _isSelectFilesAreaHighlithed;
        private bool _hasInvalidFilesSelected;
        private bool _isInfoBarOpen;

        public Type View { get; } = typeof(ImageConverterToolPage);

        public string ConvertedFormat { get; set; }

        internal ObservableCollection<ImageConversionWorkItem> ConversionWorkQueue { get; } = new();

        internal ImageConverterStrings Strings => LanguageManager.Instance.ImageConverter;

        internal string InfoBarMessage => Strings.ErrorMessage;

        internal bool IsSelectFilesAreaHighlithed
        {
            get => _isSelectFilesAreaHighlithed;
            set => SetProperty(ref _isSelectFilesAreaHighlithed, value);
        }

        internal bool HasInvalidFilesSelected
        {
            get => _hasInvalidFilesSelected;
            set => SetProperty(ref _hasInvalidFilesSelected, value);
        }

        internal bool IsInfoBarOpen
        {
            get => _isInfoBarOpen;
            set => SetProperty(ref _isInfoBarOpen, value);
        }

        [ImportingConstructor]
        public ImageConverterToolViewModel()
        {
            SelectFilesAreaDragOverCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragOverCommand);
            SelectFilesAreaDragLeaveCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragLeaveCommand);
            SelectFilesAreaDragDropCommand = new AsyncRelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragDropCommandAsync);
            SelectFilesBrowseCommand = new AsyncRelayCommand(ExecuteSelectFilesBrowseCommandAsync);
            DeleteAllCommand = new RelayCommand(ExecuteDeleteAllCommand);
            SaveAllCommand = new AsyncRelayCommand(ExecuteSaveAllCommandAsync);
            CloseInfoBarButtonCommand = new RelayCommand(ExecuteCloseInfoBarButtonCommand);

            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            ConvertedFormat = "PNG";
        }

        public void Dispose()
        {
            DeleteAllCommand.Execute(null);
        }

        #region SelectFilesAreaDragOverCommand

        public IRelayCommand<DragEventArgs> SelectFilesAreaDragOverCommand { get; }

        private void ExecuteSelectFilesAreaDragOverCommand(DragEventArgs? parameters)
        {
            Arguments.NotNull(parameters, nameof(parameters));
            if (parameters!.DataView.Contains(StandardDataFormats.StorageItems))
            {
                parameters.AcceptedOperation = DataPackageOperation.Copy;
                parameters.Handled = false;
            }

            IsSelectFilesAreaHighlithed = true;
            HasInvalidFilesSelected = false;
        }

        #endregion

        #region SelectFilesAreaDragLeaveCommand

        public IRelayCommand<DragEventArgs> SelectFilesAreaDragLeaveCommand { get; }

        private void ExecuteSelectFilesAreaDragLeaveCommand(DragEventArgs? parameters)
        {
            IsSelectFilesAreaHighlithed = false;
            HasInvalidFilesSelected = false;
        }

        #endregion

        #region SelectFilesAreaDragDropCommand

        public IAsyncRelayCommand<DragEventArgs> SelectFilesAreaDragDropCommand { get; }

        private async Task ExecuteSelectFilesAreaDragDropCommandAsync(DragEventArgs? parameters)
        {
            try
            {
                Arguments.NotNull(parameters, nameof(parameters));

                await ThreadHelper.RunOnUIThreadAsync(async () =>
                {
                    IsSelectFilesAreaHighlithed = false;
                    if (!parameters!.DataView.Contains(StandardDataFormats.StorageItems))
                    {
                        return;
                    }

                    IReadOnlyList<IStorageItem>? storageItems = await parameters.DataView.GetStorageItemsAsync();
                    if (storageItems is null || storageItems.Count == 0)
                    {
                        return;
                    }

                    foreach (IStorageItem storageItem in storageItems)
                    {
                        if (storageItem is StorageFile file)
                        {
                            if (SupportedFileExtensions.Any(ext => string.Equals(ext, file.FileType, StringComparison.OrdinalIgnoreCase)))
                            {
                                QueueNewConversionWorkItem(file);
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
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(ImageConverterToolViewModel), ex, "Unable to recognize files.");
                IsInfoBarOpen = true;
            }
        }

        #endregion

        #region SelectFilesBrowseCommand

        public IAsyncRelayCommand SelectFilesBrowseCommand { get; }

        private async Task ExecuteSelectFilesBrowseCommandAsync()
        {
            try
            {
                await ThreadHelper.RunOnUIThreadAsync(async () =>
                {
                    HasInvalidFilesSelected = false;

                    var filePicker = new FileOpenPicker
                    {
                        ViewMode = PickerViewMode.List,
                        SuggestedStartLocation = PickerLocationId.ComputerFolder
                    };

                    for (int i = 0; i < SupportedFileExtensions.Length; i++)
                    {
                        filePicker.FileTypeFilter.Add(SupportedFileExtensions[i]);
                    }

                    IReadOnlyList<StorageFile> files = await filePicker.PickMultipleFilesAsync();
                    for (int i = 0; i < files.Count; i++)
                    {
                        QueueNewConversionWorkItem(files[i]);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(ImageConverterToolViewModel), ex, "Unable to browse file(s).");
                IsInfoBarOpen = true;
            }
        }

        #endregion

        #region DeleteAllCommand

        public IRelayCommand DeleteAllCommand { get; }

        private void ExecuteDeleteAllCommand()
        {
            ConversionWorkQueue.Clear();
        }

        #endregion

        #region SaveAllCommand

        public IAsyncRelayCommand SaveAllCommand { get; }

        private async Task ExecuteSaveAllCommandAsync()
        {
            try
            {
                var works = ConversionWorkQueue.ToList();

                var folderPicker = new FolderPicker
                {
                    ViewMode = PickerViewMode.List
                };

                StorageFolder? selectedFolder = await folderPicker.PickSingleFolderAsync();

                if (selectedFolder is not null)
                {
                    ICollection<Task> conversionTasks = new List<Task>();

                    foreach (ImageConversionWorkItem work in works)
                    {
                        conversionTasks.Add(SaveConversionWorkItem(selectedFolder, work));
                    }

                    await Task.WhenAll(conversionTasks);
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(ImageConverterToolViewModel), ex, "Unable to save all files.");
                IsInfoBarOpen = true;
            }
        }

        #endregion

        #region CloseInfoBarButtonCommand

        public IRelayCommand CloseInfoBarButtonCommand { get; }

        private void ExecuteCloseInfoBarButtonCommand()
        {
            IsInfoBarOpen = false;
        }

        #endregion

        private async Task SaveConversionWorkItem(StorageFolder selectedFolder, ImageConversionWorkItem work)
        {
            StorageFile newFile = await selectedFolder.CreateFileAsync(string.Concat(work.File.DisplayName, ImageHelper.GetExtension(ConvertedFormat)), CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream outputStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder? encoder = await ImageHelper.GetEncoderAsync(ConvertedFormat, outputStream);
                var bitmap = await work.DecodeImageAsync();
                encoder.SetSoftwareBitmap(bitmap);
                await encoder.FlushAsync();
            }

            work.DeleteCommand.Execute(null);
        }

        private void QueueNewConversionWorkItem(StorageFile file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!ConversionWorkQueue.Any(item => string.Equals(item.File.Path, file.Path, StringComparison.OrdinalIgnoreCase)))
            {
                var workItem = new ImageConversionWorkItem(file);
                workItem.DeleteItemRequested += WorkItem_DeleteItemRequested;
                ConversionWorkQueue.Insert(0, workItem);
            }
        }

        private void WorkItem_DeleteItemRequested(object sender, EventArgs e)
        {
            var workItem = (ImageConversionWorkItem)sender;
            workItem.DeleteItemRequested -= WorkItem_DeleteItemRequested;
            ConversionWorkQueue.Remove(workItem);
        }
    }
}
