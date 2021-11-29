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
using DevToys.Core.Formatter;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.HexConverter;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.HexConverter
{
    [Export(typeof(HexConverterToolViewModel))]
    public sealed class HexConverterToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Which format is the input number.
        /// </summary>
        private static readonly SettingDefinition<Radix> BaseNumber
            = new(
                name: $"{nameof(HexConverterToolViewModel)}.{nameof(BaseNumber)}",
                isRoaming: true,
                defaultValue: Radix.Decimal);

        /// <summary>
        /// Whether the value should be formatted or not.
        /// </summary>
        private static readonly SettingDefinition<bool> Formatted
            = new(
                name: $"{nameof(HexConverterToolViewModel)}.{nameof(Formatted)}",
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

        internal HexConverterStrings Strings => LanguageManager.Instance.HexConverter;

        public Type View { get; } = typeof(HexConverterToolPage);

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? InputValue
        {
            get => _inputValue;
            set
            {
                SetProperty(ref _inputValue, value);
                QueueFormatting();
            }
        }

        internal string? OctalValue
        {
            get => _octalValue;
            private set => SetProperty(ref _octalValue, value);
        }

        internal string? BinaryValue
        {
            get => _binaryValue;
            private set => SetProperty(ref _binaryValue, value);
        }

        internal string? DecimalValue
        {
            get => _decimalValue;
            private set => SetProperty(ref _decimalValue, value);
        }

        internal string? HexaDecimalValue
        {
            get => _hexadecimalValue;
            private set => SetProperty(ref _hexadecimalValue, value);
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

        internal BaseNumberFormat InputBaseNumber
        {
            get
            {
                Radix settingsValue = _settingsProvider.GetSetting(BaseNumber);
                BaseNumberFormat? baseNumberFormat = BaseNumbers.FirstOrDefault(x => x.Value == settingsValue);
                return baseNumberFormat ?? BaseNumberFormat.Decimal;
            }
            set
            {
                if (InputBaseNumber != value)
                {
                    _settingsProvider.SetSetting(BaseNumber, value.Value);

                    if (InputBaseNumber == BaseNumberFormat.Octal)
                    {
                        InputValue = BaseNumberFormatter.RemoveFormatting(OctalValue).ToString();
                    }
                    else if (InputBaseNumber == BaseNumberFormat.Binary)
                    {
                        InputValue = BaseNumberFormatter.RemoveFormatting(BinaryValue).ToString();
                    }
                    else if (InputBaseNumber == BaseNumberFormat.Hexadecimal)
                    {
                        InputValue = BaseNumberFormatter.RemoveFormatting(HexaDecimalValue).ToString();
                    }
                    else
                    {
                        InputValue = BaseNumberFormatter.RemoveFormatting(DecimalValue).ToString();
                    }
                    OnPropertyChanged();
                    QueueFormatting();
                }
            }
        }

        /// <summary>
        /// Get a list of supported BaseNumbers
        /// </summary>
        internal IReadOnlyList<BaseNumberFormat> BaseNumbers = new ObservableCollection<BaseNumberFormat> {
            BaseNumberFormat.Octal,
            BaseNumberFormat.Binary,
            BaseNumberFormat.Decimal,
            BaseNumberFormat.Hexadecimal,
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
        public HexConverterToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
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
                    long? baseValue = BaseNumberFormatter.StringToBase(value, InputBaseNumber);
                    if (baseValue != null)
                    {
                        octalValue = BaseNumberFormatter.LongToBase(baseValue.Value, BaseNumberFormat.Octal, IsFormatted);
                        binaryValue = BaseNumberFormatter.LongToBase(baseValue.Value, BaseNumberFormat.Binary, IsFormatted);
                        decimalValue = BaseNumberFormatter.LongToBase(baseValue.Value, BaseNumberFormat.Decimal, IsFormatted);
                        hexaDecimalValue = BaseNumberFormatter.LongToBase(baseValue.Value, BaseNumberFormat.Hexadecimal, IsFormatted);
                    }
                }
                catch (OverflowException exception)
                {
                    isInfoBarOpen = true;
                    infoBarMessage = exception.Message;
                }
                catch (Exception ex)
                {
                    Logger.LogFault("HexConverter", ex, $"Input base number: {InputBaseNumber}");
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OctalValue = octalValue;
                    BinaryValue = binaryValue;
                    DecimalValue = decimalValue;
                    HexaDecimalValue = hexaDecimalValue;
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
    }
}
