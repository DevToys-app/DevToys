#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Core.OOP;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.AppServiceMessages.PngJpgCompressor;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace DevToys.ViewModels.Tools.PngJpgCompressor
{
    internal sealed class ImageCompressionWorkItem : ObservableRecipient, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private int _progressionPercentage;
        private string _originalFileSize = string.Empty;
        private string _newFileSize = string.Empty;
        private string _compressionRatio = string.Empty;
        private string? _tempCompressedFilePath;
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

        public void Dispose()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_tempCompressedFilePath) && File.Exists(_tempCompressedFilePath))
                {
                    File.Delete(_tempCompressedFilePath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault(nameof(ImageCompressionWorkItem), ex, "Unable to dispose the the work item");
            }
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
            _cancellationTokenSource.Cancel();
        }

        #endregion

        private async Task ConvertAsync(IAppService appService)
        {
            await TaskScheduler.Default;

            var message = new PngJpgCompressorWorkMessage
            {
                FilePath = FilePath,
            };

            try
            {
                PngJpgCompressorWorkResultMessage result
                    = await appService.SendMessageAndGetResponseAsync<PngJpgCompressorWorkResultMessage>(
                        message,
                        _cancellationTokenSource.Token);

                _tempCompressedFilePath = result.TempCompressedFilePath;

                await ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    CanCancel = false;
                    ProgressPercentage = 0;
                    if (string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        IsDone = true;
                        CompressionRatio = result.PercentageSaved.ToString("P");
                        NewFileSize = HumanizeFileSize(result.NewFileSize);
                    }
                    else
                    {
                        HasFailed = true;
                        Logger.Log("PNG/JPG Compressor", result.ErrorMessage ?? string.Empty);
                    }
                    CanDelete = true;
                });
            }
            catch (OperationCanceledException)
            {
                await ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    CanCancel = false;
                    IsDone = true;
                    CanDelete = true;
                    DeleteCommand.Execute(null);
                });
            }
        }

        private async Task ComputePropertiesAsync(StorageFile file)
        {
            BasicProperties fileProperties = await file.GetBasicPropertiesAsync();

            string fileSize = HumanizeFileSize(fileProperties.Size);

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                OriginalFileSize = fileSize;
            });
        }

        private string HumanizeFileSize(double fileSize)
        {
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (fileSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fileSize /= 1024;
            }

            string fileSizeString = string.Format("{0:0.##} {1}", fileSize, sizes[order]);
            return fileSizeString;
        }
    }
}
