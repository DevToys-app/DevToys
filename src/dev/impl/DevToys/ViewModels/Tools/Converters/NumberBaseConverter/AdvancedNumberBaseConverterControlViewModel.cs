﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Messages;
using DevToys.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using DevToys.Api.Core.Settings;
using DevToys.Api.Core;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter.Exceptions;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter
{
    [Export(typeof(AdvancedNumberBaseConverterControlViewModel))]
    public class AdvancedNumberBaseConverterControlViewModel : ObservableRecipient, IToolViewModel, IRecipient<ChangeNumberFormattingMessage>
    {
        public Type View { get; } = typeof(AdvancedNumberBaseConverterControlViewModel);
        internal NumberBaseConverterStrings Strings => LanguageManager.Instance.NumberBaseConverter;

        private readonly Queue<string> _convertQueue = new();

        private string? _inputValue;
        private string? _outputValue;
        private string? _outputCustomDictionary;
        private string? _inputCustomDictionary;
        private bool _conversionInProgress;
        private bool _toolSuccessfullyWorked;
        private bool isFormatted;
        private bool _useInputCustomDictionary;
        private bool _useOutputCustomDictionary;

        private readonly IMarketingService _marketingService;

        private NumberBaseFormat _inputDictionary = NumberBaseFormat.RFC4648_Base16;
        private NumberBaseFormat _outputDictionary = NumberBaseFormat.RFC4648_Base64;

        private NumberBaseFormat _inputCustomFormat = NumberBaseFormat.DefaultCustom;
        private NumberBaseFormat _outputCustomFormat = NumberBaseFormat.DefaultCustom;

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

        internal NumberBaseFormat InputNumberFormat
        {
            get
            {
                return _inputDictionary;
            }
            set
            {
                if (InputNumberFormat != value)
                {
                    UseInputCustomDictionary = value.Value is Radix.Custom;
                    NumberBaseFormat? newFormat = value == NumberBaseFormat.DefaultCustom ? InputCustomFormat : value;
                    _inputDictionary = newFormat;
                    QueueFormatting();
                }
            }
        }

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? OutputValue
        {
            get => _outputValue;
            private set
            {
                SetProperty(ref _outputValue, value);
            }
        }

        internal NumberBaseFormat OutputNumberFormat
        {
            get
            {
                return _outputDictionary;
            }
            set
            {
                if (OutputNumberFormat != value)
                {
                    UseOutputCustomDictionary = value.Value is Radix.Custom;
                    NumberBaseFormat? newFormat = value == NumberBaseFormat.DefaultCustom ? OutputCustomFormat : value;
                    _outputDictionary = newFormat;
                    QueueFormatting();
                }
            }
        }

        private NumberBaseFormat InputCustomFormat
        {
            get => _inputCustomFormat;
            set => _inputCustomFormat = value;
        }

        private NumberBaseFormat OutputCustomFormat
        {
            get => _outputCustomFormat;
            set => _outputCustomFormat = value;
        }

        public string? InputCustomDictionary
        {
            get => _inputCustomDictionary;
            set
            {
                SetProperty(ref _inputCustomDictionary, value);
                InputNumberFormat = InputCustomFormat = BuildCustomBase(InputNumberFormat, _inputCustomDictionary);
            }
        }

        internal string? OutputCustomDictionary
        {
            get => _outputCustomDictionary;
            set
            {
                SetProperty(ref _outputCustomDictionary, value);
                OutputNumberFormat = OutputCustomFormat = BuildCustomBase(OutputNumberFormat, _outputCustomDictionary);
            }
        }

        internal bool UseInputCustomDictionary
        {
            get => _useInputCustomDictionary;
            set
            {
                SetProperty(ref _useInputCustomDictionary, value);
            }
        }

        internal bool UseOutputCustomDictionary
        {
            get => _useOutputCustomDictionary;
            set
            {
                SetProperty(ref _useOutputCustomDictionary, value);
            }
        }

        public bool IsFormatted
        {
            get => isFormatted;
            private set
            {
                if(isFormatted == value)
                {
                    return;
                }
                SetProperty(ref isFormatted, value);
                QueueFormatting();
            }
        }

        /// <summary>
        /// Get a list of supported BaseNumbers
        /// </summary>
        internal IReadOnlyList<NumberBaseFormat> BaseNumbers = new ObservableCollection<NumberBaseFormat> {
            NumberBaseFormat.RFC4648_Base16,
            NumberBaseFormat.RFC4648_Base32,
            NumberBaseFormat.RFC4648_Base32_ExtendedHex,
            NumberBaseFormat.RFC4648_Base64,
            NumberBaseFormat.RFC4648_Base64UrlEncode,
            NumberBaseFormat.DefaultCustom
        };

        [ImportingConstructor]
        public AdvancedNumberBaseConverterControlViewModel(IMarketingService marketingService)
        {
            _marketingService = marketingService;
            IsActive = true;
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
                string resultValue = string.Empty;
                string infoBarMessage = string.Empty;
                bool isInfoBarOpen = false;
                try
                {
                    long? baseValue = NumberBaseFormatter.StringToBase(value, _inputDictionary);
                    if (baseValue != null)
                    {
                        resultValue = NumberBaseFormatter.LongToBase(baseValue.Value, _outputDictionary, IsFormatted);
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
                    Logger.LogFault("NumberBaseConverter", ex, $"Input base number: {InputNumberFormat}");
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OutputValue = resultValue;

                    Messenger.Send(new ChangeInfoBarStatusMessage(isInfoBarOpen, infoBarMessage));

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private NumberBaseFormat BuildCustomBase(NumberBaseFormat format, string? inputDict)
        {
            try
            {
                if (inputDict is not null)
                {
                    return NumberBaseFormatBuilder.BuildFormat(builder =>
                    {
                        builder.BaseNumber = inputDict.Length;
                        builder.Dictionary = inputDict;
                    });
                }
            }
            catch(InvalidBaseNumberException)
            {
                Messenger.Send(new ChangeInfoBarStatusMessage(true, Strings.BaseNumberError));
            }
            catch(InvalidDictionarySizeException)
            {
                Messenger.Send(new ChangeInfoBarStatusMessage(true, Strings.DictionarySizeError));
            }
            catch(InvalidDictionaryBaseNumberPairException)
            {
                Messenger.Send(new ChangeInfoBarStatusMessage(true, Strings.IncompatibleBaseDictionaryError));
            }
            return format;
        }

        public void Receive(ChangeNumberFormattingMessage message)
        {
            IsFormatted = message.IsFormatted;
        }
    }
}
