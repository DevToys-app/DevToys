﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using DevToys.Views.Tools.ImageConverter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.ImageConverter
{
    [Export(typeof(ImageConverterToolViewModel))]
    public sealed class ImageConverterToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        private static readonly string[] SupportedFileExtensions = new[] { ".png", ".jpg", ".jpeg" };

        private bool _isSelectFilesAreaHighlithed;
        private bool _hasInvalidFilesSelected;

        public Type View { get; } = typeof(ImageConverterToolPage);

        public string ConvertedFormat;

        internal ImageConverterStrings Strings => LanguageManager.Instance.ImageConverter;

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

        [ImportingConstructor]
        public ImageConverterToolViewModel()
        {
            SelectFilesAreaDragOverCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragOverCommand);
            SelectFilesAreaDragLeaveCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragLeaveCommand);
            SelectFilesAreaDragDropCommand = new AsyncRelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragDropCommandAsync);
            SelectFilesBrowseCommand = new AsyncRelayCommand(ExecuteSelectFilesBrowseCommandAsync);
            DeleteAllCommand = new RelayCommand(ExecuteDeleteAllCommand);
            //SaveAllCommand = new AsyncRelayCommand(ExecuteSaveAllCommandAsync);

            ConvertedFormat = "PNG";
        }

        public void Dispose()
        {
            // Delete all completed compression work.
            DeleteAllCommand.Execute(null);
        }

        internal ObservableCollection<ImageConversionWorkItem> ConversionWorkQueue { get; } = new();

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
                        string fileExtension = Path.GetExtension(file.Name);

                        if (SupportedFileExtensions.Any(ext => string.Equals(ext, fileExtension, StringComparison.OrdinalIgnoreCase)))
                        {
                            QueueNewConversion(file);
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

        #endregion

        #region SelectFilesBrowseCommand

        public IAsyncRelayCommand SelectFilesBrowseCommand { get; }

        private async Task ExecuteSelectFilesBrowseCommandAsync()
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
                    QueueNewConversion(files[i]);
                }
            });
        }

        #endregion

        #region DeleteAllCommand

        public IRelayCommand DeleteAllCommand { get; }

        private void ExecuteDeleteAllCommand()
        {
            ConversionWorkQueue.Clear();
        }

        #endregion

        public IAsyncRelayCommand SaveAllCommand { get; }

        private void QueueNewConversion(StorageFile file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!ConversionWorkQueue.Any(item => string.Equals(item.FilePath, file.Path, StringComparison.OrdinalIgnoreCase)))
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
