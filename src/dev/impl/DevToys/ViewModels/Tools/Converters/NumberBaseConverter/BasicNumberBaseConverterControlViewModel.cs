#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Api.Tools;
using DevToys.Models;
using DevToys.UI.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml;
using DevToys.Core;
using DevToys.Messages;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter
{
    [Export(typeof(BasicNumberBaseConverterControlViewModel))]
    public class BasicNumberBaseConverterControlViewModel : ObservableRecipient, IToolViewModel, IRecipient<ChangeNumberFormattingMessage>
    {
        /// <summary>
        /// Which format is the input number.
        /// </summary>
        private Radix _baseNumber = Radix.Decimal;

        /// <summary>
        /// Whether the value should be formatted or not.
        /// </summary>
        private bool _formatted = true;

        private readonly Queue<string> _convertQueue = new();

        private string? _inputValue;
        private string? _octalValue;
        private string? _binaryValue;
        private string? _decimalValue;
        private string? _hexadecimalValue;
        private bool _conversionInProgress;
        private bool _toolSuccessfullyWorked;

        public Type View { get; } = typeof(BasicNumberBaseConverterControlViewModel);
        internal NumberBaseConverterStrings Strings => LanguageManager.Instance.NumberBaseConverter;

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

        internal NumberBaseFormat InputBaseNumber
        {
            get
            {
                NumberBaseFormat? baseNumberFormat = BaseNumbers.FirstOrDefault(x => x.Value == _baseNumber);
                return baseNumberFormat ?? NumberBaseFormat.Decimal;
            }
            set
            {
                if (InputBaseNumber != value)
                {
                    SetProperty(ref _baseNumber, value.Value);
                    //OnPropertyChanged();
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
            get => _formatted;
            set
            {
                if (_formatted != value)
                {
                    SetProperty(ref _formatted, value);
                    //OnPropertyChanged();
                    QueueFormatting(true);
                }
            }
        }

        public BasicNumberBaseConverterControlViewModel()
        {
            IsActive = true;
            InputFocusChanged = ControlFocusChanged;
        }

        internal RoutedEventHandler InputFocusChanged { get; }
        private void ControlFocusChanged(object source, RoutedEventArgs args)
        {
            if (!IsFormatted)
            {
                return;
            }

            var input = (CustomTextBox)source;

            if (input.Text.Length == 0)
            {
                return;
            }

            switch (input.Tag)
            {
                case "Binary" when InputBaseNumber == NumberBaseFormat.Binary:
                    input.Text = NumberBaseFormatter.FormatNumber(input.Text, InputBaseNumber);
                    break;
                case "Octal" when InputBaseNumber == NumberBaseFormat.Octal:
                    input.Text = NumberBaseFormatter.FormatNumber(input.Text, InputBaseNumber);
                    break;
                case "Decimal" when InputBaseNumber == NumberBaseFormat.Decimal:
                    input.Text = NumberBaseFormatter.FormatNumber(input.Text, InputBaseNumber);
                    break;
                case "Hexadecimal" when InputBaseNumber == NumberBaseFormat.Hexadecimal:
                    input.Text = NumberBaseFormatter.FormatNumber(input.Text, InputBaseNumber);
                    break;
            }
        }

        private void QueueFormatting(bool formatAll = false)
        {
            _convertQueue.Enqueue(InputValue ?? string.Empty);
            TreatQueueAsync(formatAll).Forget();
        }

        private async Task TreatQueueAsync(bool formatAll)
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
                    FillPropertyValues(ref _binaryValue, binaryValue, nameof(BinaryValue), formatAll || NumberBaseFormat.Binary != InputBaseNumber);
                    FillPropertyValues(ref _octalValue, octalValue, nameof(OctalValue), formatAll || NumberBaseFormat.Octal != InputBaseNumber);
                    FillPropertyValues(ref _decimalValue, decimalValue, nameof(DecimalValue), formatAll || NumberBaseFormat.Decimal != InputBaseNumber);
                    FillPropertyValues(ref _hexadecimalValue, hexaDecimalValue, nameof(HexaDecimalValue), formatAll || NumberBaseFormat.Hexadecimal != InputBaseNumber);

                    if (isInfoBarOpen)
                    {
                        Messenger.Send(new ChangeInfoBarStatusMessage(infoBarMessage));
                    }

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        //_marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private void FillPropertyValues(ref string? property, string? value, string viewModelName, bool format)
        {
            if (format)
            {
                SetProperty(ref property, value, viewModelName);
            }
        }

        public void Receive(ChangeNumberFormattingMessage message)
        {
            IsFormatted = message.IsFormatted;
        }
    }
}
