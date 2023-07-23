#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.CertificateEncoderDecoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Storage;

namespace DevToys.ViewModels.Tools.CertificateEncoderDecoder
{
    [Export(typeof(CertificateEncoderDecoderToolViewModel))]
    public class CertificateEncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly IMarketingService _marketingService;
        private readonly Queue<(string? cert, string? password)> _conversionQueue = new();
        private readonly ImmutableHashSet<string> _allowedFileExtensions = new HashSet<string>()
        {
            ".cer",
            ".crt",
            ".pfx",
            ".pem"
        }.ToImmutableHashSet();

        private string? _passwordValue;
        private string? _inputValue;
        private string? _outputValue;
        private bool _conversionInProgress;
        private bool _toolSuccessfullyWorked;

        private CancellationTokenSource? _cancellationTokenSource;
        private StorageFile? _certificateFile;

        public Type View { get; } = typeof(CertificateEncoderDecoderToolPage);

        internal CertificateEncoderDecoderStrings Strings => LanguageManager.Instance.CertificateEncoderDecoder;

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? PasswordValue
        {
            get => _passwordValue;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _passwordValue, value);
                QueueConversionCalculation();
            }
        }

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? InputValue
        {
            get => _inputValue;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _inputValue, value);
                QueueConversionCalculation();
            }
        }

        /// <summary>
        /// Gets or sets the output text.
        /// </summary>
        internal string? OutputValue
        {
            get => _outputValue;
            private set => SetProperty(ref _outputValue, value);
        }

        internal StorageFile? CertificateFile
        {
            get => _certificateFile;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (value != _certificateFile)
                {
                    SetProperty(ref _certificateFile, value);
                }
            }
        }

        internal string AllowedFileExtensions => string.Join(';', _allowedFileExtensions.Select(x => x));

        [ImportingConstructor]
        public CertificateEncoderDecoderToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _marketingService = marketingService;

            FilesSelectedCommand = new RelayCommand<StorageFile[]>(ExecuteFilesSelectedCommand);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
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

            CertificateFile = file;
            ConvertFromCertificateToStringAsync(file, cancellationToken).Forget();
        }

        private async Task ConvertFromCertificateToStringAsync(StorageFile file, CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;

            using var memStream = new MemoryStream();
            using Stream stream = await file.OpenStreamForReadAsync();

            await stream.CopyToAsync(memStream);

            byte[] bytes = memStream.ToArray();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            string fileExtension = file.FileType;
            string output = "";

            if (_allowedFileExtensions.Contains(fileExtension.ToLowerInvariant()))
            {
                output = CertificateHelper.GetRawCertificateString(bytes);
            }
            else
            {
                throw new NotSupportedException();
            }

            await SetInputValueAsync(output);
        }

        private void QueueConversionCalculation()
        {
            _conversionQueue.Enqueue((InputValue ?? string.Empty, PasswordValue));
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_conversionInProgress)
            {
                return;
            }

            _conversionInProgress = true;

            await TaskScheduler.Default;

            while (_conversionQueue.TryDequeue(out (string? cert, string? password) certAndPw))
            {
                string conversionResult = string.IsNullOrEmpty(certAndPw.cert)
                    ? ""
                    : await DecodeCertificateDataAsync(certAndPw.cert!, certAndPw.password).ConfigureAwait(false);

                ThreadHelper.RunOnUIThreadAsync(Core.Threading.ThreadPriority.Low, () =>
                {
                    OutputValue = conversionResult;

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private async Task SetInputValueAsync(string value)
        {
            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                InputValue = value;
            });
        }

        private async Task<string> EncodeHtmlDataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            string? encoded;
            try
            {
                encoded = HttpUtility.HtmlEncode(data);
            }
            catch (Exception ex)
            {
                Logger.LogFault("Html - Encoder", ex);
                return ex.Message;
            }

            return encoded;
        }

        private async Task<string> DecodeCertificateDataAsync(string data, string? password)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;
            string? decoded = string.Empty;
            if (CertificateHelper.TryDecodeCertificate(data, password, out string? decodedValue))
            {
                decoded = decodedValue!;
            }

            return decoded;
        }
    }
}
