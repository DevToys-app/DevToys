#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.StringEscapeUnescape;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.StringEscapeUnescape
{
    [Export(typeof(StringEscapeUnescapeToolViewModel))]
    public sealed class StringEscapeUnescapeToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the tool should escape or unescape the text.
        /// </summary>
        private static readonly SettingDefinition<bool> EscapeMode
            = new(
                name: $"{nameof(StringEscapeUnescapeToolViewModel)}.{nameof(EscapeMode)}",
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

        public Type View { get; } = typeof(StringEscapeUnescapeToolPage);

        internal StringEscapeUnescapeStrings Strings => LanguageManager.Instance.StringEscapeUnescape;

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
                    if (TextMatchAtIndex(data, "\n", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\n";
                    }
                    else if (TextMatchAtIndex(data, "\r", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\r";
                    }
                    else if (TextMatchAtIndex(data, "\t", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\t";
                    }
                    else if (TextMatchAtIndex(data, "\b", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\b";
                    }
                    else if (TextMatchAtIndex(data, "\f", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\f";
                    }
                    else if (TextMatchAtIndex(data, "\"", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\\"";
                    }
                    else if (TextMatchAtIndex(data, "\\", i))
                    {
                        jumpLength = 1;
                        replacementString = "\\\\";
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
                    if (TextMatchAtIndex(data, "\\n", i))
                    {
                        jumpLength = 2;
                        replacementString = "\n";
                    }
                    else if (TextMatchAtIndex(data, "\\r", i))
                    {
                        jumpLength = 2;
                        replacementString = "\r";
                    }
                    else if (TextMatchAtIndex(data, "\\t", i))
                    {
                        jumpLength = 2;
                        replacementString = "\t";
                    }
                    else if (TextMatchAtIndex(data, "\\b", i))
                    {
                        jumpLength = 2;
                        replacementString = "\b";
                    }
                    else if (TextMatchAtIndex(data, "\\f", i))
                    {
                        jumpLength = 2;
                        replacementString = "\f";
                    }
                    else if (TextMatchAtIndex(data, "\\\"", i))
                    {
                        jumpLength = 2;
                        replacementString = "\"";
                    }
                    else if (TextMatchAtIndex(data, "\\\\", i))
                    {
                        jumpLength = 2;
                        replacementString = "\\";
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
