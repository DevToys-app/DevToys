#nullable enable

using System;
using System.Threading.Tasks;
using DevToys.Api.Core.OOP;
using DevToys.Core.Threading;
using DevToys.Shared.AppServiceMessages.PngJpgCompressor;
using DevToys.Shared.Core;
using DevToys.Shared.Core.OOP;
using DevToys.Shared.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace DevToys.ViewModels.Tools.PngJpgCompressor
{
    internal sealed class ImageCompressionWorkItem : ObservableRecipient, IProgress<AppServiceProgressMessage>
    {
        private int _progressionPercentage;
        private string _originalFileSize = string.Empty;
        private string _newFileSize = string.Empty;
        private string _compressionRatio = string.Empty;
        private bool _isDone;
        private bool _hasFailed;
        private bool _canCancel;
        private bool _canDelete;

        internal string FileName { get; }

        internal string FilePath { get; }

        internal string OriginalFileSize
        {
            get => _originalFileSize;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _originalFileSize, value);
            }
        }

        internal string NewFileSize
        {
            get => _newFileSize;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _newFileSize, value);
            }
        }

        internal string CompressionRatio
        {
            get => _compressionRatio;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _compressionRatio, value);
            }
        }

        internal int ProgressPercentage
        {
            get => _progressionPercentage;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _progressionPercentage, value);
            }
        }

        internal bool IsDone
        {
            get => _isDone;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _isDone, value);
            }
        }

        internal bool HasFailed
        {
            get => _hasFailed;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _hasFailed, value);
            }
        }

        internal bool CanCancel
        {
            get => _canCancel;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _canCancel, value);
            }
        }

        internal bool CanDelete
        {
            get => _canDelete;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _canDelete, value);
            }
        }

        internal event EventHandler? DeleteItemRequested;

        internal event EventHandler? SaveItemRequested;

        internal ImageCompressionWorkItem(IAppService appService, StorageFile file)
        {
            Arguments.NotNull(appService, nameof(appService));
            Arguments.NotNull(file, nameof(file));

            FileName = file.Name;
            FilePath = file.Path;
            CanCancel = true;

            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);

            ComputePropertiesAsync(file).Forget();
            ConvertAsync(appService).Forget();
        }

        #region DeleteCommand

        public IRelayCommand DeleteCommand { get; }

        private void ExecuteDeleteCommand()
        {
            DeleteItemRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region SaveCommand

        public IRelayCommand SaveCommand { get; }

        private void ExecuteSaveCommand()
        {
            SaveItemRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region CancelCommand

        public IRelayCommand CancelCommand { get; }

        private void ExecuteCancelCommand()
        {
        }

        #endregion

        private async Task ComputePropertiesAsync(StorageFile file)
        {
            BasicProperties fileProperties = await file.GetBasicPropertiesAsync();

            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            double fileSize = fileProperties.Size;
            int order = 0;
            while (fileSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fileSize /= 1024;
            }

            string fileSizeString = string.Format("{0:0.##} {1}", fileSize, sizes[order]);

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                OriginalFileSize = fileSizeString;
            });
        }

        private async Task ConvertAsync(IAppService appService)
        {
            await TaskScheduler.Default;

            var message = new PngJpgCompressorWorkMessage
            {
                FilePath = FilePath,
            };

            PngJpgCompressorWorkResultMessage result
                = await appService.SendMessageAndGetResponseAsync<PngJpgCompressorWorkResultMessage>(
                    message,
                    progress: this);

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                CanCancel = false;
                ProgressPercentage = 0;
                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    IsDone = true;
                    CompressionRatio = "## %";
                    NewFileSize = "#.## MB";
                }
                else
                {
                    HasFailed = true;
                }
                CanDelete = true;
            });
        }

        public void Report(AppServiceProgressMessage value)
        {
            ThreadHelper.RunOnUIThreadAsync(() =>
            {
                ProgressPercentage = value.ProgressPercentage;
            });
        }
    }
}
