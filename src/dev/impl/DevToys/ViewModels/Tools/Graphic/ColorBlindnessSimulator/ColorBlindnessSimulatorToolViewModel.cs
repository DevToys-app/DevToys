#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
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

namespace DevToys.ViewModels.Tools.ColorBlindnessSimulator
{
    [Export(typeof(ColorBlindnessSimulatorToolViewModel))]
    public sealed class ColorBlindnessSimulatorToolViewModel : ObservableRecipient, IToolViewModel
    {
        private static readonly string[] SupportedFileExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };

        private readonly object _lockObject = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isSelectFilesAreaHighlithed;
        private bool _hasInvalidFilesSelected;

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

        [ImportingConstructor]
        public ColorBlindnessSimulatorToolViewModel()
        {
            SelectFilesAreaDragOverCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragOverCommand);
            SelectFilesAreaDragLeaveCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragLeaveCommand);
            SelectFilesAreaDragDropCommand = new AsyncRelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragDropCommandAsync);
            SelectFilesBrowseCommand = new AsyncRelayCommand(ExecuteSelectFilesBrowseCommandAsync);
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
                        QueueNewSimulation(file);
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
                    QueueNewSimulation(file);
                }
            });
        }

        #endregion

        private void QueueNewSimulation(StorageFile file)
        {
            lock (_lockObject)
            {
                Arguments.NotNull(file, nameof(file));

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                _cancellationTokenSource = new CancellationTokenSource();
                SimulateColorBlindnessAsync(file, _cancellationTokenSource.Token).Forget();
            }
        }

        private async Task SimulateColorBlindnessAsync(StorageFile file, CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;

            byte[] bgra8SourcePixels = await GetBgra8PixelsFromFileAsync(file);

            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task<byte[]> GetBgra8PixelsFromFileAsync(StorageFile file)
        {
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform()
                {
                    ScaledWidth = Convert.ToUInt32(decoder.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(decoder.PixelHeight)
                };
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
                    BitmapAlphaMode.Ignore,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
                    ColorManagementMode.DoNotColorManage
                );

                // An array containing the decoded image data, which could be modified before being displayed 
                return pixelData.DetachPixelData();
            }
        }
    }
}
