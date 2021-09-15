#nullable enable

using DevTools.Common;
using DevTools.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace DevTools.Providers.Impl.Tools.Base64EncoderDecoder
{
    [Export(typeof(Base64EncoderDecoderToolViewModel))]
    public class Base64EncoderDecoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        private string? _inputValue;
        private string? _outputValue;
        private string? _encodingMode;
        private string? _conversionMode;
        private readonly IThread _thread;

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
                _thread.ThrowIfNotOnUIThread();
                _inputValue = value;

                if (ConversionMode == "Encode")
                {
                    OutputValue = EncodeBase64Data(value);
                }
                else
                {
                    OutputValue = DecodeBase64Data(value);
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the output text.
        /// </summary>
        internal string? OutputValue
        {
            get => _outputValue;
            set
            {
                _thread.ThrowIfNotOnUIThread();
                _outputValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the conversion mode.
        /// </summary>
        internal string? ConversionMode
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_conversionMode))
                {
                    return "Encode";
                }
                return _conversionMode;
            }
            set
            {
                _thread.ThrowIfNotOnUIThread();
                _conversionMode = value;
                string? tmp = InputValue;
                InputValue = OutputValue;
                OutputValue = tmp;
            }
        }

        /// <summary>
        /// Gets or sets the encoding mode.
        /// </summary>
        internal string? EncodingMode
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_encodingMode))
                {
                    return "UTF-8";
                }
                return _encodingMode;
            }
            set
            {
                _thread.ThrowIfNotOnUIThread();
                _encodingMode = value;
            }
        }

        [ImportingConstructor]
        public Base64EncoderDecoderToolViewModel(IThread thread)
        {
            _thread = thread;
        }

        public string EncodeBase64Data(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            string? encoded;

            try
            {
                Encoding encoder = GetEncoder();
                byte[] dataBytes = encoder.GetBytes(data);
                encoded = Convert.ToBase64String(dataBytes);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return encoded;
        }

        public string DecodeBase64Data(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            string? decoded;

            try
            {
                byte[] decodedData = Convert.FromBase64String(data);
                Encoding encoder = GetEncoder();
                decoded = encoder.GetString(decodedData);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return decoded;
        }

        private Encoding GetEncoder()
        {
            if (EncodingMode == "UTF-8")
            {
                return Encoding.UTF8;
            }
            return Encoding.ASCII;
        }
    }
}
