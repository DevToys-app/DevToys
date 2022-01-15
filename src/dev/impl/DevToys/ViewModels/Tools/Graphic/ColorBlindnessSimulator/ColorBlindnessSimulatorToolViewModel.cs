#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.ColorBlindnessSimulator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.ViewModels.Tools.ColorBlindnessSimulator
{
    [Export(typeof(ColorBlindnessSimulatorToolViewModel))]
    public sealed class ColorBlindnessSimulatorToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        private static readonly string[] SupportedFileExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };

        private readonly object _lockObject = new();
        private readonly List<string> _tempFileNames = new();

        private CancellationTokenSource? _cancellationTokenSource;
        private DateTime _timeSinceLastprogressUpdate;
        private bool _isSelectFilesAreaHighlithed;
        private bool _hasInvalidFilesSelected;
        private bool _isResultGridVisible;
        private bool _isProgressGridVisible;
        private int _progress;
        private int _protanopiaProgress;
        private int _tritanopiaProgress;
        private int _deuteranopiaProgress;
        private Uri? _originalOutput;
        private Uri? _protanopiaOutput;
        private Uri? _tritanopiaOutput;
        private Uri? _deuteranopiaOutput;

        public Type View { get; } = typeof(ColorBlindnessSimulatorToolPage);

        internal ColorBlindnessSimulatorStrings Strings => LanguageManager.Instance.ColorBlindnessSimulator;

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

        internal bool IsResultGridVisible
        {
            get => _isResultGridVisible;
            set => SetProperty(ref _isResultGridVisible, value);
        }

        internal bool IsProgressGridVisible
        {
            get => _isProgressGridVisible;
            set => SetProperty(ref _isProgressGridVisible, value);
        }

        internal int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        internal Uri? OriginalOutput
        {
            get => _originalOutput;
            set => SetProperty(ref _originalOutput, value);
        }

        internal Uri? ProtanopiaOutput
        {
            get => _protanopiaOutput;
            set => SetProperty(ref _protanopiaOutput, value);
        }

        internal Uri? TritanopiaOutput
        {
            get => _tritanopiaOutput;
            set => SetProperty(ref _tritanopiaOutput, value);
        }

        internal Uri? DeuteranopiaOutput
        {
            get => _deuteranopiaOutput;
            set => SetProperty(ref _deuteranopiaOutput, value);
        }

        [ImportingConstructor]
        public ColorBlindnessSimulatorToolViewModel()
        {
            SelectFilesAreaDragOverCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragOverCommand);
            SelectFilesAreaDragLeaveCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragLeaveCommand);
            SelectFilesAreaDragDropCommand = new AsyncRelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragDropCommandAsync);
            SelectFilesBrowseCommand = new AsyncRelayCommand(ExecuteSelectFilesBrowseCommandAsync);
            SelectFilesPasteCommand = new AsyncRelayCommand(ExecuteSelectFilesPasteCommandAsync);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            ClearTempFiles();
        }

        public void UpdateProgress(bool force = false)
        {
            if (force || DateTime.Now - _timeSinceLastprogressUpdate > TimeSpan.FromMilliseconds(1))
            {
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    Progress = (_protanopiaProgress + _tritanopiaProgress + _deuteranopiaProgress) / 3;
                    _timeSinceLastprogressUpdate = DateTime.Now;
                }).Forget();
            }
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
                if (storageItems is null || storageItems.Count != 1)
                {
                    return;
                }

                IStorageItem storageItem = storageItems[0];
                if (storageItem is StorageFile file)
                {
                    string fileExtension = Path.GetExtension(file.Name);

                    if (SupportedFileExtensions.Any(ext => string.Equals(ext, fileExtension, StringComparison.OrdinalIgnoreCase)))
                    {
                        QueueNewSimulation(file, addFileToListOfTempFilesToDelete: false);
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

                StorageFile file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    QueueNewSimulation(file, addFileToListOfTempFilesToDelete: false);
                }
            });
        }

        #endregion

        #region SelectFilesPasteCommand

        public IAsyncRelayCommand SelectFilesPasteCommand { get; }

        private async Task ExecuteSelectFilesPasteCommandAsync()
        {
            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                HasInvalidFilesSelected = false;

                DataPackageView? dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (dataPackageView is not null && dataPackageView.Contains(StandardDataFormats.Bitmap))
                {
                    IRandomAccessStreamReference? imageReceived = await dataPackageView.GetBitmapAsync();
                    if (imageReceived is not null)
                    {
                        using (IRandomAccessStreamWithContentType imageStream = await imageReceived.OpenReadAsync())
                        {
                            StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
                            StorageFile storageFile = await localCacheFolder.CreateFileAsync($"{Guid.NewGuid()}.jpeg", CreationCollisionOption.ReplaceExisting);

                            using (IRandomAccessStream? stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                await imageStream.AsStreamForRead().CopyToAsync(stream.AsStreamForWrite());
                            }

                            QueueNewSimulation(storageFile, addFileToListOfTempFilesToDelete: true);
                        }
                    }
                }
            });
        }

        #endregion

        #region CancelCommand

        public IRelayCommand CancelCommand { get; }

        private void ExecuteCancelCommand()
        {
            lock (_lockObject)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                _cancellationTokenSource = new CancellationTokenSource();

                IsResultGridVisible = false;
                IsProgressGridVisible = false;
                _protanopiaProgress = 0;
                _tritanopiaProgress = 0;
                _deuteranopiaProgress = 0;
                UpdateProgress(force: true);
                ClearTempFiles();
            }
        }

        #endregion

        private void QueueNewSimulation(StorageFile file, bool addFileToListOfTempFilesToDelete)
        {
            lock (_lockObject)
            {
                Arguments.NotNull(file, nameof(file));

                ExecuteCancelCommand();
                IsProgressGridVisible = true;

                if (addFileToListOfTempFilesToDelete)
                {
                    _tempFileNames.Add(file.Path);
                }

                Assumes.NotNull(_cancellationTokenSource, nameof(_cancellationTokenSource));
                SimulateColorBlindnessAsync(file, _cancellationTokenSource!.Token).Forget();
            }
        }

        private async Task SimulateColorBlindnessAsync(StorageFile file, CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;

            (byte[] bgra8SourcePixels, uint width, uint height) = await GetBgra8PixelsFromFileAsync(file);

            cancellationToken.ThrowIfCancellationRequested();

            string randomFileName = Guid.NewGuid().ToString();

            var workTasks
                = new List<Task<Uri>>
                {
                        Task.Run(async () =>
                        {
                            return await SaveImageToFileAsync(bgra8SourcePixels, width, height, Path.GetFileNameWithoutExtension(randomFileName), "Original");
                        }),
                        Task.Run(async () =>
                        {
                            byte[] protanopiaBgraPixels
                                = ColorBlindnessSimulatorHelper.SimulateProtanopia(
                                    bgra8SourcePixels,
                                    (p) => { _protanopiaProgress = p; UpdateProgress(); },
                                    cancellationToken);
                            UpdateProgress(force: true);
                            return await SaveImageToFileAsync(protanopiaBgraPixels, width, height, Path.GetFileNameWithoutExtension(randomFileName), "Protanopia");
                        }),
                        Task.Run(async () =>
                        {
                            byte[] tritanopiaBgraPixels
                                = ColorBlindnessSimulatorHelper.SimulateTritanopia(
                                    bgra8SourcePixels,
                                    (p) => { _tritanopiaProgress = p; UpdateProgress(); },
                                    cancellationToken);
                            UpdateProgress(force: true);
                            return await SaveImageToFileAsync(tritanopiaBgraPixels, width, height, Path.GetFileNameWithoutExtension(randomFileName), "Tritanopia");
                        }),
                        Task.Run(async () =>
                        {
                            byte[] deuteranopiaBgraPixels
                                = ColorBlindnessSimulatorHelper.SimulateDeuteranopia(
                                    bgra8SourcePixels,
                                    (p) => { _deuteranopiaProgress = p; UpdateProgress(); },
                                    cancellationToken);
                            UpdateProgress(force: true);
                            return await SaveImageToFileAsync(deuteranopiaBgraPixels, width, height, Path.GetFileNameWithoutExtension(randomFileName), "Deuteranopia");
                        })
                };

            await Task.WhenAll(workTasks);

            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                UpdateProgress(force: true);

                OriginalOutput = workTasks[0].Result;
                ProtanopiaOutput = workTasks[1].Result;
                TritanopiaOutput = workTasks[2].Result;
                DeuteranopiaOutput = workTasks[3].Result;
                OnPropertyChanged(nameof(OriginalOutput)); // Do this in case if the new file path is the same than before but with a different image.
                OnPropertyChanged(nameof(ProtanopiaOutput));
                OnPropertyChanged(nameof(TritanopiaOutput));
                OnPropertyChanged(nameof(DeuteranopiaOutput));

                IsProgressGridVisible = false;
                IsResultGridVisible = true;
            });
        }

        private async Task<(byte[] bgra8SourcePixels, uint width, uint height)> GetBgra8PixelsFromFileAsync(StorageFile file)
        {
            await TaskScheduler.Default;
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform()
                {
                    ScaledWidth = Convert.ToUInt32(decoder.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(decoder.PixelHeight)
                };

                PixelDataProvider pixelData
                    = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
                        BitmapAlphaMode.Ignore,
                        transform,
                        ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
                        ColorManagementMode.DoNotColorManage
                    );

                // An array containing the decoded image data, which could be modified before being displayed 
                return (pixelData.DetachPixelData(), decoder.PixelWidth, decoder.PixelHeight);
            }
        }

        private async Task<Uri> SaveImageToFileAsync(byte[] bgraPixels, uint width, uint height, string imageName, string disabilityName)
        {
            await TaskScheduler.Default;
            StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
            StorageFile storageFile = await localCacheFolder.CreateFileAsync($"{imageName}-{disabilityName}.png", CreationCollisionOption.ReplaceExisting);

            _tempFileNames.Add(storageFile.Path);

            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    width,
                    height,
                    96.0,
                    96.0,
                    bgraPixels);

                await encoder.FlushAsync();
            }

            return new Uri(storageFile.Path);
        }

        private void ClearTempFiles()
        {
            for (int i = 0; i < _tempFileNames.Count; i++)
            {
                string tempFile = _tempFileNames[i];
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogFault(nameof(ColorBlindnessSimulatorToolViewModel), ex, "Unable to delete a temporary file.");
                }
            }
        }
    }
}
