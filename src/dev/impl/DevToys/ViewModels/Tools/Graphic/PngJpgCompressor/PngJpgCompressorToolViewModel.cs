#nullable enable

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
        private readonly IAppService _appService;

        public Type View { get; } = typeof(PngJpgCompressorToolPage);

        internal PngJpgCompressorStrings Strings => LanguageManager.Instance.PngJpgCompressor;

        internal ObservableCollection<ImageCompressionWorkItem> CompressionWorkQueue { get; } = new();

        [ImportingConstructor]
        public PngJpgCompressorToolViewModel(IAppService appService)
        {
            _appService = appService;

            FilesSelectedCommand = new RelayCommand<StorageFile[]>(ExecuteFilesSelectedCommand);
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

        #region FilesSelectedCommand

        public IRelayCommand<StorageFile[]> FilesSelectedCommand { get; }

        private void ExecuteFilesSelectedCommand(StorageFile[]? files)
        {
            if (files is not null)
            {
                foreach (StorageFile file in files)
                {
                    QueueNewConversion(file);
                }
            }
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
