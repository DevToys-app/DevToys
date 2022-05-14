#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.Base64ImageEncoderDecoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.ViewModels.Tools.Base64ImageEncoderDecoder
{
    [Export(typeof(Base64ImageEncoderDecoderToolViewModel))]
    public class Base64ImageEncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel, IDisposable
    {
        /// <summary>
        /// Whether the tool should encode/decode in Unicode or ASCII.
        /// </summary>
        private static readonly SettingDefinition<string> Encoder
            = new(
                name: $"{nameof(Base64ImageEncoderDecoderToolViewModel)}.{nameof(Encoder)}",
                isRoaming: true,
                defaultValue: DefaultEncoding);

        private const string DefaultEncoding = "UTF-8";

        private readonly object _lockObject = new();
        private readonly List<string> _tempFileNames = new();
        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;

        private CancellationTokenSource? _cancellationTokenSource;
        private string? _base64Data;
        private StorageFile? _imageFile;
        private bool _ignoreBase64DataChange;

        public Type View { get; } = typeof(Base64ImageEncoderDecoderToolPage);

        internal Base64ImageEncoderDecoderStrings Strings => LanguageManager.Instance.Base64ImageEncoderDecoder;

        internal string? Base64Data
        {
            get => _base64Data;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (value != _base64Data)
                {
                    SetProperty(ref _base64Data, value);
                    if (!_ignoreBase64DataChange)
                    {
                        QueueNewConversionFromBase64ToImage(_base64Data);
                    }
                }
            }
        }

        internal StorageFile? ImageFile
        {
            get => _imageFile;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (value != _imageFile)
                {
                    SetProperty(ref _imageFile, value);
                }
            }
        }

        [ImportingConstructor]
        public Base64ImageEncoderDecoderToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;

            FilesSelectedCommand = new RelayCommand<StorageFile[]>(ExecuteFilesSelectedCommand);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            ClearTempFiles();
        }

        #region FilesSelectedCommand

        public IRelayCommand<StorageFile[]> FilesSelectedCommand { get; }

        private void ExecuteFilesSelectedCommand(StorageFile[]? files)
        {
            if (files is not null)
            {
                Debug.Assert(files.Length == 1);
                QueueNewConversionFromImageToBase64(files[0]);
            }
        }

        #endregion

        private void QueueNewConversionFromImageToBase64(StorageFile file)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            SetImageDataAsync(file)
                .ContinueWith(_ =>
                {
                    ConvertFromImageToBase64Async(file, cancellationToken).Forget();
                });
        }

        private void QueueNewConversionFromBase64ToImage(string? base64)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            SetImageDataAsync(null)
                .ContinueWith(_ =>
                {
                    ConvertFromBase64ToImageAsync(base64, cancellationToken).Forget();
                });
        }

        private async Task ConvertFromBase64ToImageAsync(string? base64, CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;

            string? trimmedData = base64?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedData))
            {
                return;
            }

            string fileType;
            if (trimmedData!.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".png";
            }
            else if (trimmedData!.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".jpeg";
            }
            else if (trimmedData!.StartsWith("data:image/bmp;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".bmp";
            }
            else if (trimmedData!.StartsWith("data:image/gif;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".gif";
            }
            else if (trimmedData!.StartsWith("data:image/x-icon;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".ico";
            }
            else if (trimmedData!.StartsWith("data:image/svg+xml;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".svg";
            }
            else if (trimmedData!.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase))
            {
                fileType = ".webp";
            }
            else
            {
                return;
            }

            base64 = trimmedData.Substring(trimmedData.IndexOf(',') + 1);
            byte[] bytes = Convert.FromBase64String(base64);

            StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
            StorageFile storageFile = await localCacheFolder.CreateFileAsync($"{Guid.NewGuid()}{fileType}", CreationCollisionOption.ReplaceExisting);

            _tempFileNames.Add(storageFile.Path);

            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await stream.WriteAsync(bytes.AsBuffer());
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await SetImageDataAsync(storageFile);
        }

        private async Task ConvertFromImageToBase64Async(StorageFile file, CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;

            using var memStream = new MemoryStream();
            using Stream stream = await file.OpenStreamForReadAsync();

            await stream.CopyToAsync(memStream);

            byte[] bytes = memStream.ToArray();
            string base64 = Convert.ToBase64String(bytes);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            string fileExtension = file.FileType;
            string output
                = fileExtension.ToLowerInvariant() switch
                {
                    ".png" => "data:image/png;base64," + base64,
                    ".jpg" or ".jpeg" => "data:image/jpeg;base64," + base64,
                    ".bmp" => "data:image/bmp;base64," + base64,
                    ".gif" => "data:image/gif;base64," + base64,
                    ".ico" => "data:image/x-icon;base64," + base64,
                    ".svg" => "data:image/svg+xml;base64," + base64,
                    ".webp" => "data:image/webp;base64," + base64,
                    _ => throw new NotSupportedException(),
                };

            await SetBase64DataAsync(output);
        }

        private async Task SetImageDataAsync(StorageFile? file)
        {
            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                ImageFile = file;
            });
        }

        private async Task SetBase64DataAsync(string base64)
        {
            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                _ignoreBase64DataChange = true;
                Base64Data = base64;
                _ignoreBase64DataChange = false;
            });
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
                    Logger.LogFault(nameof(Base64ImageEncoderDecoderToolViewModel), ex, "Unable to delete a temporary file.");
                }
            }
        }
    }
}
