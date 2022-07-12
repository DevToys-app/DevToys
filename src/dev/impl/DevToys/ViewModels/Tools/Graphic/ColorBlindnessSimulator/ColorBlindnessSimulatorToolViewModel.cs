#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.ColorBlindnessSimulator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DevToys.ViewModels.Tools.ColorBlindnessSimulator
{
    [Export(typeof(ColorBlindnessSimulatorToolViewModel))]
    public sealed class ColorBlindnessSimulatorToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        private readonly object _lockObject = new();
        private readonly List<string> _tempFileNames = new();
        private readonly IMarketingService _marketingService;

        private CancellationTokenSource? _cancellationTokenSource;
        private DateTime _timeSinceLastprogressUpdate;
        private bool _isResultGridVisible;
        private bool _isProgressGridVisible;
        private int _progress;
        private int _protanopiaProgress;
        private int _tritanopiaProgress;
        private int _deuteranopiaProgress;
        private StorageFile? _originalOutput;
        private StorageFile? _protanopiaOutput;
        private StorageFile? _tritanopiaOutput;
        private StorageFile? _deuteranopiaOutput;

        public Type View { get; } = typeof(ColorBlindnessSimulatorToolPage);

        internal ColorBlindnessSimulatorStrings Strings => LanguageManager.Instance.ColorBlindnessSimulator;

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

        internal StorageFile? OriginalOutput
        {
            get => _originalOutput;
            set => SetProperty(ref _originalOutput, value);
        }

        internal StorageFile? ProtanopiaOutput
        {
            get => _protanopiaOutput;
            set => SetProperty(ref _protanopiaOutput, value);
        }

        internal StorageFile? TritanopiaOutput
        {
            get => _tritanopiaOutput;
            set => SetProperty(ref _tritanopiaOutput, value);
        }

        internal StorageFile? DeuteranopiaOutput
        {
            get => _deuteranopiaOutput;
            set => SetProperty(ref _deuteranopiaOutput, value);
        }

        [ImportingConstructor]
        public ColorBlindnessSimulatorToolViewModel(IMarketingService marketingService)
        {
            _marketingService = marketingService;

            FilesSelectedCommand = new RelayCommand<StorageFile[]>(ExecuteFilesSelectedCommand);
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

        #region FilesSelectedCommand

        public IRelayCommand<StorageFile[]> FilesSelectedCommand { get; }

        private void ExecuteFilesSelectedCommand(StorageFile[]? files)
        {
            if (files is not null)
            {
                Debug.Assert(files.Length == 1);
                QueueNewSimulation(files[0]);
            }
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

        private void QueueNewSimulation(StorageFile file)
        {
            lock (_lockObject)
            {
                Arguments.NotNull(file, nameof(file));

                ExecuteCancelCommand();
                IsProgressGridVisible = true;

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
                = new List<Task<StorageFile>>
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

                _marketingService.NotifyToolSuccessfullyWorked();
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

        private async Task<StorageFile> SaveImageToFileAsync(byte[] bgraPixels, uint width, uint height, string imageName, string disabilityName)
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

            return storageFile;
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
