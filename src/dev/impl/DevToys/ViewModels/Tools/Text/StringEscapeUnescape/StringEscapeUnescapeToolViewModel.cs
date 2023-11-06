#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.StringEscapeUnescape;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.StringEscapeUnescape
{
    [Export(typeof(StringEscapeUnescapeToolViewModel))]
    public sealed class StringEscapeUnescapeToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// What newline encoding the newline characters should be replaced with.
        /// </summary>
        private static readonly SettingDefinition<SpecialCharacter> NewlineEncodingMode
            = new(
                name: $"{nameof(StringEscapeUnescapeToolViewModel)}.{nameof(NewlineEncodingMode)}",
                isRoaming: true,
                defaultValue: GetDefaultNewlineEncoding());
        /// <summary>
        /// Whether the tool should escape or unescape the text.
        /// </summary>
        private static readonly SettingDefinition<bool> EscapeMode
            = new(
                name: $"{nameof(StringEscapeUnescapeToolViewModel)}.{nameof(EscapeMode)}",
                isRoaming: true,
                defaultValue: true);
        /// <summary>
        /// Gets the NewlineEcoding used by default per the user's Operating System
        /// </summary>
        /// <returns></returns>
        private static Models.SpecialCharacter GetDefaultNewlineEncoding()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Models.SpecialCharacter.CarriageReturnLineFeed
                    : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Models.SpecialCharacter.CarriageReturn : Models.SpecialCharacter.LineFeed);
        }


        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly Queue<string> _conversionQueue = new();

        private string? _inputValue;
        private string? _outputValue;
        private bool _conversionInProgress;
        private bool _setPropertyInProgress;
        private bool _toolSuccessfullyWorked;

        public Type View { get; } = typeof(StringEscapeUnescapeToolPage);

        internal StringEscapeUnescapeStrings Strings => LanguageManager.Instance.StringEscapeUnescape;

        /// <summary>
        /// Gets or sets the desired newline encoding.
        /// </summary>
        internal NewlineEncodingDisplayPair NewlineEncoding
        {
            get
            {
                SpecialCharacter settingsValue = _settingsProvider.GetSetting(NewlineEncodingMode);
                NewlineEncodingDisplayPair? newlineEncoding = NewlineEncodings.FirstOrDefault(x => x.Value == settingsValue);
                return newlineEncoding ?? NewlineEncodingDisplayPair.CarriageReturn;
            }
            set
            {
                if (NewlineEncoding != value)
                {
                    _settingsProvider.SetSetting(NewlineEncodingMode, value.Value);
                    OnPropertyChanged();
                    QueueConversionCalculation();
                }
            }
        }

        /// <summary>
        /// Get a list of supported NewlineEncoding
        /// </summary>
        internal IReadOnlyList<NewlineEncodingDisplayPair> NewlineEncodings = new ObservableCollection<NewlineEncodingDisplayPair> {
            Models.NewlineEncodingDisplayPair.Linefeed,
            Models.NewlineEncodingDisplayPair.CarriageReturn,
            Models.NewlineEncodingDisplayPair.CarriageReturnLineFeed,
        };

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
        /// Gets or sets the escaping conversion mode.
        /// </summary>
        internal bool IsEscapeMode
        {
            get => _settingsProvider.GetSetting(EscapeMode);
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (_settingsProvider.GetSetting(EscapeMode) != value)
                    {
                        _settingsProvider.SetSetting(EscapeMode, value);
                        OnPropertyChanged();
                    }
                    InputValue = OutputValue;
                    _setPropertyInProgress = false;
                }
            }
        }

        [ImportingConstructor]
        public StringEscapeUnescapeToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
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
                if (IsEscapeMode)
                {
                    conversionResult = await EscapeStringAsync(text).ConfigureAwait(false);
                }
                else
                {
                    conversionResult = await UnescapeStringAsync(text).ConfigureAwait(false);
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

        private async Task<string> EscapeStringAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;
            var encoded = new StringBuilder();

            try
            {
                int i = 0;
                while (i < data!.Length)
                {
                    string replacementString = string.Empty;
                    int jumpLength = 0;
                    if (TextMatchAtIndex(data, SpecialCharacterDefinition.CarriageReturnLinefeed, i))
                    {
                        jumpLength = 2;
                        replacementString = SpecialCharacterDefinition.Parse(_settingsProvider.GetSetting(NewlineEncodingMode)).Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.LineFeed, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Parse(_settingsProvider.GetSetting(NewlineEncodingMode)).Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.CarriageReturn, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Parse(_settingsProvider.GetSetting(NewlineEncodingMode)).Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.Tab, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Tab.Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.Backspace, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Backspace.Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.FormFeed, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.FormFeed.Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.DoubleQuote, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.DoubleQuote.Escaped;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.Backslash, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Backslash.Escaped;
                    }

                    if (!string.IsNullOrEmpty(replacementString) && jumpLength > 0)
                    {
                        encoded.Append(replacementString);
                        i += jumpLength;
                    }
                    else
                    {
                        encoded.Append(data[i]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault("String Escape", ex);
                return ex.Message;
            }

            return encoded.ToString();
        }

        private async Task<string> UnescapeStringAsync(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;
            var decoded = new StringBuilder();

            try
            {
                int i = 0;
                while (i < data!.Length)
                {
                    string replacementString = string.Empty;
                    int jumpLength = 0;

                    if (TextMatchAtIndex(data, SpecialCharacterDefinition.CarriageReturnLinefeed.Escaped, i))
                    {
                        jumpLength = 4;
                        replacementString = SpecialCharacterDefinition.Parse(_settingsProvider.GetSetting(NewlineEncodingMode)).Value;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.LineFeed.Escaped, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Parse(_settingsProvider.GetSetting(NewlineEncodingMode)).Value;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.CarriageReturn.Escaped, i))
                    {
                        jumpLength = 1;
                        replacementString = SpecialCharacterDefinition.Parse(_settingsProvider.GetSetting(NewlineEncodingMode)).Value;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.Tab.Escaped, i))
                    {
                        jumpLength = 2;
                        replacementString = SpecialCharacterDefinition.Tab;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.Backspace.Escaped, i))
                    {
                        jumpLength = 2;
                        replacementString = SpecialCharacterDefinition.Backspace;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.FormFeed.Escaped, i))
                    {
                        jumpLength = 2;
                        replacementString = SpecialCharacterDefinition.FormFeed;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.DoubleQuote.Escaped, i))
                    {
                        jumpLength = 2;
                        replacementString = SpecialCharacterDefinition.DoubleQuote;
                    }
                    else if (TextMatchAtIndex(data, SpecialCharacterDefinition.Backslash.Escaped, i))
                    {
                        jumpLength = 2;
                        replacementString = SpecialCharacterDefinition.Backslash;
                    }

                    if (!string.IsNullOrEmpty(replacementString) && jumpLength > 0)
                    {
                        decoded.Append(replacementString);
                        i += jumpLength;
                    }
                    else
                    {
                        decoded.Append(data[i]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault("String Unescape", ex);
                return ex.Message;
            }

            return decoded.ToString();
        }

        private bool TextMatchAtIndex(string data, string test, int startIndex)
        {
            if (string.IsNullOrEmpty(test))
            {
                return false;
            }

            if (data.Length < test.Length)
            {
                return false;
            }

            for (int i = 0; i < test.Length; i++)
            {
                if (data[startIndex + i] != test[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
