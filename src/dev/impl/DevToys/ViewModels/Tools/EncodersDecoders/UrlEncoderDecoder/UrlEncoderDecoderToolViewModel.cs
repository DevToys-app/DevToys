#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.UrlEncoderDecoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.UrlEncoderDecoder
{
    [Export(typeof(UrlEncoderDecoderToolViewModel))]
    public class UrlEncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the tool should encode or decode Url.
        /// </summary>
        private static readonly SettingDefinition<bool> EncodeMode
            = new(
                name: $"{nameof(UrlEncoderDecoderToolViewModel)}.{nameof(EncodeMode)}",
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

        public Type View { get; } = typeof(UrlEncoderDecoderToolPage);

        internal UrlEncoderDecoderStrings Strings => LanguageManager.Instance.UrlEncoderDecoder;

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
        /// Gets or sets the conversion mode.
        /// </summary>
        internal bool IsEncodeMode
        {
            get => _settingsProvider.GetSetting(EncodeMode);
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (_settingsProvider.GetSetting(EncodeMode) != value)
                    {
                        _settingsProvider.SetSetting(EncodeMode, value);
                        OnPropertyChanged();
                    }
                    InputValue = OutputValue;
                    _setPropertyInProgress = false;
                }
            }
        }

        [ImportingConstructor]
        public UrlEncoderDecoderToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
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
                if (IsEncodeMode)
                {
                    conversionResult = await EncodeUrlDataAsync(text).ConfigureAwait(false);
                }
                else
                {
                    conversionResult = await DecodeUrlDataAsync(text).ConfigureAwait(false);
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

        private async Task<string> EncodeUrlDataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            string? encoded;
            try
            {
                encoded = Uri.EscapeDataString(data);
            }
            catch (Exception ex)
            {
                Logger.LogFault("Url - Encoder", ex);
                return ex.Message;
            }

            return encoded;
        }

        private async Task<string> DecodeUrlDataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;
            string? decoded = string.Empty;

            try
            {
                decoded = Uri.UnescapeDataString(data);
            }
            catch (FormatException)
            {
                // ignore;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Url - Decoder", ex);
                return ex.Message;
            }

            return decoded;
        }
    }
}
