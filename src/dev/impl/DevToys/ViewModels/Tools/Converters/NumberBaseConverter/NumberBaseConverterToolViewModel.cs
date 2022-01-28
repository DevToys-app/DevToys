#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using DevToys.Views.Tools.NumberBaseConverter;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.NumberBaseConverter
{
    [Export(typeof(NumberBaseConverterToolViewModel))]
    public sealed class NumberBaseConverterToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Which format is the input number.
        /// </summary>
        private static readonly SettingDefinition<Radix> BaseNumber
            = new(
                name: $"{nameof(NumberBaseConverterToolViewModel)}.{nameof(BaseNumber)}",
                isRoaming: true,
                defaultValue: Radix.Decimal);

        /// <summary>
        /// Whether the value should be formatted or not.
        /// </summary>
        private static readonly SettingDefinition<bool> Formatted
            = new(
                name: $"{nameof(NumberBaseConverterToolViewModel)}.{nameof(Formatted)}",
                isRoaming: true,
                defaultValue: true);

        private readonly Queue<string> _convertQueue = new();
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMarketingService _marketingService;

        private string? _inputValue;
        private string? _octalValue;
        private string? _binaryValue;
        private string? _decimalValue;
        private string? _hexadecimalValue;
        private string? _infoBarMessage;
        private bool _isInfoBarOpen;
        private bool _conversionInProgress;
        private bool _toolSuccessfullyWorked;

        internal NumberBaseConverterStrings Strings => LanguageManager.Instance.NumberBaseConverter;

        public Type View { get; } = typeof(NumberBaseConverterToolPage);

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? InputValue
        {
            get => _inputValue;
            set
            {
                _inputValue = value;
                QueueFormatting();
            }
        }

        internal string? BinaryValue
        {
            get => _binaryValue;
            set
            {
                if (value == _binaryValue)
                {
                    return;
                }

                SetProperty(ref _binaryValue, value);
                InputBaseNumber = NumberBaseFormat.Binary;
                InputValue = NumberBaseFormatter.RemoveFormatting(BinaryValue).ToString();
            }
        }

        internal string? OctalValue
        {
            get => _octalValue;
            set
            {
                if (value == _octalValue)
                {
                    return;
                }

                SetProperty(ref _octalValue, value);
                InputBaseNumber = NumberBaseFormat.Octal;
                InputValue = NumberBaseFormatter.RemoveFormatting(OctalValue).ToString();
            }
        }

        internal string? DecimalValue
        {
            get => _decimalValue;
            set
            {
                if (value == _decimalValue)
                {
                    return;
                }

                SetProperty(ref _decimalValue, value);
                InputBaseNumber = NumberBaseFormat.Decimal;
                InputValue = NumberBaseFormatter.RemoveFormatting(DecimalValue).ToString();
            }
        }

        internal string? HexaDecimalValue
        {
            get => _hexadecimalValue;
            set
            {
                if (value == _hexadecimalValue)
                {
                    return;
                }

                SetProperty(ref _hexadecimalValue, value);
                InputBaseNumber = NumberBaseFormat.Hexadecimal;
                InputValue = NumberBaseFormatter.RemoveFormatting(HexaDecimalValue).ToString();
            }
        }

        internal bool IsInfoBarOpen
        {
            get => _isInfoBarOpen;
            private set => SetProperty(ref _isInfoBarOpen, value);
        }

        internal string? InfoBarMessage
        {
            get => _infoBarMessage;
            private set => SetProperty(ref _infoBarMessage, value);
        }

        internal NumberBaseFormat InputBaseNumber
        {
            get
            {
                Radix settingsValue = _settingsProvider.GetSetting(BaseNumber);
                NumberBaseFormat? baseNumberFormat = BaseNumbers.FirstOrDefault(x => x.Value == settingsValue);
                return baseNumberFormat ?? NumberBaseFormat.Decimal;
            }
            set
            {
                if (InputBaseNumber != value)
                {
                    _settingsProvider.SetSetting(BaseNumber, value.Value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get a list of supported BaseNumbers
        /// </summary>
        internal IReadOnlyList<NumberBaseFormat> BaseNumbers = new ObservableCollection<NumberBaseFormat> {
            NumberBaseFormat.Octal,
            NumberBaseFormat.Binary,
            NumberBaseFormat.Decimal,
            NumberBaseFormat.Hexadecimal,
        };

        internal bool IsFormatted
        {
            get => _settingsProvider.GetSetting(Formatted);
            set
            {
                if (_settingsProvider.GetSetting(Formatted) != value)
                {
                    _settingsProvider.SetSetting(Formatted, value);
                    OnPropertyChanged();
                    QueueFormatting();
                }
            }
        }

        [ImportingConstructor]
        public NumberBaseConverterToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueFormatting()
        {
            _convertQueue.Enqueue(InputValue ?? string.Empty);
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

            while (_convertQueue.TryDequeue(out string value))
            {
                string octalValue = string.Empty;
                string binaryValue = string.Empty;
                string decimalValue = string.Empty;
                string hexaDecimalValue = string.Empty;
                string infoBarMessage = string.Empty;
                bool isInfoBarOpen = false;
                try
                {
                    long? baseValue = NumberBaseFormatter.StringToBase(value, InputBaseNumber);
                    if (baseValue != null)
                    {
                        octalValue = NumberBaseFormatter.LongToBase(baseValue.Value, NumberBaseFormat.Octal, IsFormatted);
                        binaryValue = NumberBaseFormatter.LongToBase(baseValue.Value, NumberBaseFormat.Binary, IsFormatted);
                        decimalValue = NumberBaseFormatter.LongToBase(baseValue.Value, NumberBaseFormat.Decimal, IsFormatted);
                        hexaDecimalValue = NumberBaseFormatter.LongToBase(baseValue.Value, NumberBaseFormat.Hexadecimal, IsFormatted);
                    }
                }
                catch (OverflowException exception)
                {
                    isInfoBarOpen = true;
                    infoBarMessage = exception.Message;
                }
                catch (InvalidOperationException exception)
                {
                    isInfoBarOpen = true;
                    infoBarMessage = exception.Message;
                }
                catch (Exception ex)
                {
                    Logger.LogFault("NumberBaseConverter", ex, $"Input base number: {InputBaseNumber}");
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    FillPropertyValues(ref _binaryValue, binaryValue, nameof(BinaryValue));
                    FillPropertyValues(ref _octalValue, octalValue, nameof(OctalValue));
                    FillPropertyValues(ref _decimalValue, decimalValue, nameof(DecimalValue));
                    FillPropertyValues(ref _hexadecimalValue, hexaDecimalValue, nameof(HexaDecimalValue));

                    InfoBarMessage = infoBarMessage;
                    IsInfoBarOpen = isInfoBarOpen;

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private void FillPropertyValues(ref string? property, string? value, string viewModelName)
        {
            SetProperty(ref property, value, viewModelName);
        }
    }
}
