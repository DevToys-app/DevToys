#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.Base64EncoderDecoder;

namespace DevToys.ViewModels.Tools.Base64EncoderDecoder
{
    [Export(typeof(Base64EncoderDecoderToolViewModel))]
    public class Base64EncoderDecoderToolViewModel : QueueWorkerViewModelBase<string>, IToolViewModel
    {
        /// <summary>
        /// Whether the tool should encode or decode Base64.
        /// </summary>
        private static readonly SettingDefinition<bool> EncodeMode
            = new(
                name: $"{nameof(Base64EncoderDecoderToolViewModel)}.{nameof(EncodeMode)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether the tool should encode/decode in Unicode or ASCII.
        /// </summary>
        private static readonly SettingDefinition<string> Encoder
            = new(
                name: $"{nameof(Base64EncoderDecoderToolViewModel)}.{nameof(Encoder)}",
                isRoaming: true,
                defaultValue: DefaultEncoding);


        private static readonly NewlineSeparatorDisplayPair DefaultNewlineSeparator = 
            NewlineSeparatorDisplayPair.Values.SingleOrDefault(v => Environment.NewLine.Equals(v.EscapeSequence)) ?? NewlineSeparatorDisplayPair.LF;
        
        /// <summary>
        /// Whether the tool should encode using CRLF (Windows) or LF (Unix) for newlines.
        /// Defaults to the current platforms default newline separator.
        /// </summary>
        private static readonly SettingDefinition<NewlineSeparator> NewlineSeparator
            = new(
                name: $"{nameof(Base64EncoderDecoderToolViewModel)}.{nameof(NewlineSeparator)}",
                isRoaming: true,
                defaultValue: DefaultNewlineSeparator.Value);

        private const string DefaultEncoding = "UTF-8";

        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly Queue<string> _conversionQueue = new();

        private string? _inputValue;
        private string? _outputValue;
        private bool _setPropertyInProgress;
        private bool _toolSuccessfullyWorked;

        public Type View { get; } = typeof(Base64EncoderDecoderToolPage);

        internal Base64EncoderDecoderStrings Strings => LanguageManager.Instance.Base64EncoderDecoder;

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

        /// <summary>
        /// Gets or sets the encoding mode.
        /// </summary>
        internal string EncodingMode
        {
            get => _settingsProvider.GetSetting(Encoder);
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (!string.Equals(_settingsProvider.GetSetting(Encoder), value, StringComparison.Ordinal))
                {
                    _settingsProvider.SetSetting(Encoder, value);
                    OnPropertyChanged();
                    QueueConversionCalculation();
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired newline separator.
        /// </summary>
        internal NewlineSeparatorDisplayPair NewlineSeparatorMode
        {
            get
            {
                NewlineSeparator settingsValue = _settingsProvider.GetSetting(NewlineSeparator);
                NewlineSeparatorDisplayPair? newlineSeparator = NewlineSeparators.FirstOrDefault(x => x.Value == settingsValue);
                return newlineSeparator ?? DefaultNewlineSeparator;
            }
            set
            {
                if (NewlineSeparatorMode != value)
                {
                    _settingsProvider.SetSetting(NewlineSeparator, value.Value);
                    OnPropertyChanged();
                    QueueConversionCalculation();
                }
            }
        }

        /// <summary>
        /// Get a list of supported newline separators
        /// </summary>
        internal IReadOnlyList<NewlineSeparatorDisplayPair> NewlineSeparators = new ObservableCollection<NewlineSeparatorDisplayPair>(NewlineSeparatorDisplayPair.Values);

        [ImportingConstructor]
        public Base64EncoderDecoderToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueConversionCalculation()
        {
            EnqueueComputation(InputValue ?? string.Empty);
        }

        protected override async Task TreatComputationQueueAsync(string value)
        {
            string conversionResult;
            if (IsEncodeMode)
            {
                conversionResult = await EncodeBase64DataAsync(value).ConfigureAwait(false);
            }
            else
            {
                conversionResult = await DecodeBase64DataAsync(value).ConfigureAwait(false);
            }

            await ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
            {
                OutputValue = conversionResult;

                if (!_toolSuccessfullyWorked)
                {
                    _toolSuccessfullyWorked = true;
                    _marketingService.NotifyToolSuccessfullyWorked();
                }
            });
        }

        private async Task<string> EncodeBase64DataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            string? encoded;
            try
            {
                data = ReplaceNewlinesWithSelectedNewlines(data);

                Encoding encoder = GetEncoder();
                byte[]? dataBytes = encoder.GetBytes(data);
                encoded = Convert.ToBase64String(dataBytes);
            }
            catch (Exception ex)
            {
                Logger.LogFault("Base 64 - Encoder", ex, $"Encoding mode: {EncodingMode}");
                return ex.Message;
            }

            return encoded;
        }

        private string ReplaceNewlinesWithSelectedNewlines(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            return Regex.Replace(data, @"\r\n?|\n", NewlineSeparatorMode.EscapeSequence);
        }

        private async Task<string> DecodeBase64DataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            int remainder = data!.Length % 4;
            if (remainder > 0)
            {
                int padding = 4 - remainder;
                data = data.PadRight(data.Length + padding, '=');
            }

            await TaskScheduler.Default;
            string? decoded = string.Empty;

            try
            {
                byte[]? decodedData = Convert.FromBase64String(data);
                Encoding encoder = GetEncoder();
                if (encoder is UTF8Encoding && decodedData != null)
                {
                    byte[] preamble = encoder.GetPreamble();
                    if (decodedData.Take(preamble.Length).SequenceEqual(preamble))
                    {
                        // need to keep it this way to have the dom char
                        decoded += Encoding.Unicode.GetString(preamble, 0, 1);
                    }
                }
                decoded += encoder.GetString(decodedData);
            }
            catch (FormatException)
            {
                // ignore;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Base 64 - Decoder", ex, $"Encoding mode: {EncodingMode}");
                return ex.Message;
            }

            return decoded;
        }

        private Encoding GetEncoder()
        {
            if (string.Equals(EncodingMode, DefaultEncoding, StringComparison.Ordinal))
            {
                return new UTF8Encoding(true);
            }
            return Encoding.ASCII;
        }
    }
}
