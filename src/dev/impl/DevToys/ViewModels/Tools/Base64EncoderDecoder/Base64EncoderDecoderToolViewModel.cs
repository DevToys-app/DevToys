#nullable enable

using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Views.Tools.Base64EncoderDecoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.Base64EncoderDecoder
{
    [Export(typeof(Base64EncoderDecoderToolViewModel))]
    public class Base64EncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        private const string DefaultEncoding = "UTF-8";
        private const string DefaultConversion = "Encode";
        internal const string DecodeConversion = "Decode";

        private string? _inputValue;
        private string? _outputValue;
        private string _encodingMode = DefaultEncoding;
        private string _conversionMode = DefaultConversion;
        private bool _conversionInProgress;
        private bool _setPropertyInProgress;
        private readonly Queue<string> _conversionQueue = new();

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
        internal string ConversionMode
        {
            get => _conversionMode;
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;
                    ThreadHelper.ThrowIfNotOnUIThread();
                    SetProperty(ref _conversionMode, value);
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
            get => _encodingMode;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _encodingMode, value);
                QueueConversionCalculation();
            }
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

            while (_conversionQueue.TryDequeue(out string text))
            {
                string conversionResult;
                if (string.Equals(ConversionMode, DefaultConversion, StringComparison.Ordinal))
                {
                    conversionResult = await EncodeBase64DataAsync(text).ConfigureAwait(false);
                }
                else
                {
                    conversionResult = await DecodeBase64DataAsync(text).ConfigureAwait(false);
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OutputValue = conversionResult;
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private async Task<string> EncodeBase64DataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            string encoded = string.Empty;

            try
            {
                Encoding encoder = GetEncoder();
                byte[] dataBytes = encoder.GetBytes(data);
                encoded = Convert.ToBase64String(dataBytes);
            }
            catch (Exception ex)
            {
                Logger.LogFault("Base 64 - Encoder", ex, $"Encoding mode: {EncodingMode}");
                return ex.Message;
            }

            return encoded;
        }

        private async Task<string> DecodeBase64DataAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;
            string decoded = string.Empty;

            try
            {
                byte[] decodedData = Convert.FromBase64String(data);
                Encoding encoder = GetEncoder();
                decoded = encoder.GetString(decodedData);
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
                return Encoding.UTF8;
            }
            return Encoding.ASCII;
        }
    }
}
