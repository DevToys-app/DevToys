#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.GZipEncoderDecoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.GZipEncoderDecoder
{
    [Export(typeof(GZipEncoderDecoderToolViewModel))]
    public class GZipEncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the tool should encode or decode Html.
        /// </summary>
        private static readonly SettingDefinition<bool> CompressMode
            = new(
                name: $"{nameof(GZipEncoderDecoderToolViewModel)}.{nameof(CompressMode)}",
                isRoaming: true,
                defaultValue: true);

        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly Queue<string> _conversionQueue = new();

        private string? _inputValue;
        private string? _outputValue;
        private bool _conversionInProgress;
        private bool _setPropertyInProgress;
        private bool _toolSuccessfullyWorked;

        public Type View { get; } = typeof(GZipEncoderDecoderToolPage);

        internal GZipEncoderDecoderStrings Strings => LanguageManager.Instance.GZipEncoderDecoder;

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

        /// <summary>
        /// Gets or sets the compress mode.
        /// </summary>
        internal bool IsCompressMode
        {
            get => _settingsProvider.GetSetting(CompressMode);
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (_settingsProvider.GetSetting(CompressMode) != value)
                    {
                        _settingsProvider.SetSetting(CompressMode, value);
                        OnPropertyChanged();
                    }
                    InputValue = OutputValue;
                    _setPropertyInProgress = false;
                }
            }
        }

        [ImportingConstructor]
        public GZipEncoderDecoderToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueConversionCalculation()
        {
            _conversionQueue.Enqueue(InputValue ?? string.Empty);
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

            while (_conversionQueue.TryDequeue(out string? text))
            {
                string conversionResult;
                if (IsCompressMode)
                {
                    conversionResult = await Compress(text).ConfigureAwait(false);
                }
                else
                {
                    conversionResult = await Decompress(text).ConfigureAwait(false);
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
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

        private async Task<string> Compress(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            string? compressed;
            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(data);
                using var outputStream = new MemoryStream();
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);
                }

                compressed = Convert.ToBase64String(outputStream.ToArray());
            }
            catch (Exception ex)
            {
                Logger.LogFault("GZip - Compress", ex);
                return ex.Message;
            }

            return compressed;
        }

        private async Task<string> Decompress(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;
            string? decompressed = string.Empty;

            try
            {
                byte[] inputBytes = Convert.FromBase64String(data);
                using var inputStream = new MemoryStream(inputBytes);
                using var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress);
                using var streamReader = new StreamReader(gZipStream);
                decompressed = streamReader.ReadToEnd();
            }
            catch (FormatException)
            {
                // ignore;
            }
            catch (Exception ex)
            {
                Logger.LogFault("GZip - Decompress", ex);
                return ex.Message;
            }

            return decompressed;
        }
    }
}
