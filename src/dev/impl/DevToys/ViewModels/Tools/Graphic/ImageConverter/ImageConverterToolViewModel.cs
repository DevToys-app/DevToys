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
using DevToys.Views.Tools.ImageConverter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace DevToys.ViewModels.Tools.ImageConverter
{
    [Export(typeof(ImageConverterToolViewModel))]
    public sealed class ImageConverterToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        private bool _isInfoBarOpen;

        public Type View { get; } = typeof(ImageConverterToolPage);

        public string ConvertedFormat { get; set; }

        internal ObservableCollection<ImageConversionWorkItem> ConversionWorkQueue { get; } = new();

        internal ImageConverterStrings Strings => LanguageManager.Instance.ImageConverter;

        internal string InfoBarMessage => Strings.ErrorMessage;

        internal bool IsInfoBarOpen
        {
            get => _isInfoBarOpen;
            set => SetProperty(ref _isInfoBarOpen, value);
        }

        [ImportingConstructor]
        public ImageConverterToolViewModel()
        {
            FilesSelectedCommand = new RelayCommand<StorageFile[]>(ExecuteFilesSelectedCommand);
            DeleteAllCommand = new RelayCommand(ExecuteDeleteAllCommand);
            SaveAllCommand = new AsyncRelayCommand(ExecuteSaveAllCommandAsync);
            CloseInfoBarButtonCommand = new RelayCommand(ExecuteCloseInfoBarButtonCommand);

            // Initialize ComboBox
            ConvertedFormat = "PNG";
        }

        public void Dispose()
        {
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
                    QueueNewConversionWorkItem(file);
                }
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
            await work.Process(this, newFile);

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
