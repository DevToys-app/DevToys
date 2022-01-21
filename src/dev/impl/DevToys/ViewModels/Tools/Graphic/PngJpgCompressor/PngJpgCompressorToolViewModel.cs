﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Core.OOP;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using DevToys.Views.Tools.PngJpgCompressor;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.PngJpgCompressor
{
    [Export(typeof(PngJpgCompressorToolViewModel))]
    public sealed class PngJpgCompressorToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        private static readonly string[] SupportedFileExtensions = new[] { ".png", ".jpg", ".jpeg" };

        private readonly IAppService _appService;

        private bool _isSelectFilesAreaHighlithed;
        private bool _hasInvalidFilesSelected;

        public Type View { get; } = typeof(PngJpgCompressorToolPage);

        internal PngJpgCompressorStrings Strings => LanguageManager.Instance.PngJpgCompressor;

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

        internal ObservableCollection<ImageCompressionWorkItem> CompressionWorkQueue { get; } = new();

        [ImportingConstructor]
        public PngJpgCompressorToolViewModel(IAppService appService)
        {
            _appService = appService;

            SelectFilesAreaDragOverCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragOverCommand);
            SelectFilesAreaDragLeaveCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragLeaveCommand);
            SelectFilesAreaDragDropCommand = new AsyncRelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragDropCommandAsync);
            SelectFilesBrowseCommand = new AsyncRelayCommand(ExecuteSelectFilesBrowseCommandAsync);
            DeleteAllCommand = new RelayCommand(ExecuteDeleteAllCommand);
            SaveAllCommand = new AsyncRelayCommand(ExecuteSaveAllCommandAsync);
        }

        public void Dispose()
        {
            // Cancel all compression work in progress.
            var works = CompressionWorkQueue.ToList();
            foreach (ImageCompressionWorkItem work in works)
            {
                work.CancelCommand.Execute(null);
            }

            // Delete all completed compression work.
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

                for (int i = 0; i < storageItems.Count; i++)
                {
                    IStorageItem storageItem = storageItems[i];
                    if (storageItem is StorageFile file)
                    {
                        if (SupportedFileExtensions.Any(ext => string.Equals(ext, file.FileType, StringComparison.OrdinalIgnoreCase)))
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
            int i = 0;
            while (i < CompressionWorkQueue.Count)
            {
                ImageCompressionWorkItem workItem = CompressionWorkQueue[i];
                if (workItem.CanDelete)
                {
                    CompressionWorkQueue.Remove(workItem);
                    workItem.Dispose();
                }
                else
                {
                    i++;
                }
            }
        }

        #endregion

        #region SaveAllCommand

        public IAsyncRelayCommand SaveAllCommand { get; }

        private async Task ExecuteSaveAllCommandAsync()
        {
            var works = CompressionWorkQueue.ToList();

            if (works.Any(work => work.IsDone))
            {
                var folderPicker = new FolderPicker
                {
                    ViewMode = PickerViewMode.List
                };
                StorageFolder? selectedFolder = await folderPicker.PickSingleFolderAsync();
                if (selectedFolder is not null)
                {
                    foreach (ImageCompressionWorkItem work in works)
                    {
                        if (work.IsDone)
                        {
                            StorageFile newFile = await selectedFolder.CreateFileAsync(work.FileName, CreationCollisionOption.ReplaceExisting);
                            StorageFile tempCompressedFile = await StorageFile.GetFileFromPathAsync(work.TempCompressedFilePath);
                            await tempCompressedFile.CopyAndReplaceAsync(newFile);

                            work.DeleteCommand.Execute(null);
                        }
                    }
                }
            }
        }

        #endregion

        private void QueueNewConversion(StorageFile file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!CompressionWorkQueue.Any(item => string.Equals(item.FilePath, file.Path, StringComparison.OrdinalIgnoreCase)))
            {
                var workItem = new ImageCompressionWorkItem(_appService, file);
                workItem.DeleteItemRequested += WorkItem_DeleteItemRequested;
                CompressionWorkQueue.Insert(0, workItem);
            }
        }

        private void WorkItem_DeleteItemRequested(object sender, EventArgs e)
        {
            var workItem = (ImageCompressionWorkItem)sender;
            workItem.DeleteItemRequested -= WorkItem_DeleteItemRequested;
            CompressionWorkQueue.Remove(workItem);
            workItem.Dispose();
        }
    }
}
