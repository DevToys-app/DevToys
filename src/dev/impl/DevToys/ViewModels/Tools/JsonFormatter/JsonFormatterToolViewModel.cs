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
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Models;
using DevToys.Views.Tools.JsonFormatter;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.JsonFormatter
{
    [Export(typeof(JsonFormatterToolViewModel))]
    public sealed class JsonFormatterToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// The indentation to apply while formatting.
        /// </summary>
        private static readonly SettingDefinition<Indentation> Indentation
            = new(
                name: $"{nameof(JsonFormatterToolViewModel)}.{nameof(Indentation)}",
                isRoaming: true,
                defaultValue: Models.Indentation.TwoSpaces);

        private readonly IMarketingService _marketingService;
        private readonly Queue<string> _formattingQueue = new();

        private bool _toolSuccessfullyWorked;
        private bool _formattingInProgress;
        private string? _inputValue;
        private string? _outputValue;

        public Type View { get; } = typeof(JsonFormatterToolPage);

        internal JsonFormatterStrings Strings => LanguageManager.Instance.JsonFormatter;

        /// <summary>
        /// Gets or sets the desired indentation.
        /// </summary>
        internal IndentationDisplayPair IndentationMode
        {
            get
            {
                Indentation settingsValue = SettingsProvider.GetSetting(Indentation);
                IndentationDisplayPair? indentation = Indentations.FirstOrDefault(x => x.Value == settingsValue);
                return indentation ?? IndentationDisplayPair.TwoSpaces;
            }
            set
            {
                if (IndentationMode != value)
                {
                    SettingsProvider.SetSetting(Indentation, value.Value);
                    OnPropertyChanged();
                    QueueFormatting();
                }
            }
        }

        /// <summary>
        /// Get a list of supported Indentation
        /// </summary>
        internal IReadOnlyList<IndentationDisplayPair> Indentations = new ObservableCollection<IndentationDisplayPair> {
            Models.IndentationDisplayPair.TwoSpaces,
            Models.IndentationDisplayPair.FourSpaces,
            Models.IndentationDisplayPair.OneTab,
            Models.IndentationDisplayPair.Minified,
        };

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

        internal string? OutputValue
        {
            get => _outputValue;
            set => SetProperty(ref _outputValue, value);
        }

        internal ISettingsProvider SettingsProvider { get; }

        [ImportingConstructor]
        public JsonFormatterToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueFormatting()
        {
            _formattingQueue.Enqueue(InputValue ?? string.Empty);
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_formattingInProgress)
            {
                return;
            }

            _formattingInProgress = true;

            await TaskScheduler.Default;

            while (_formattingQueue.TryDequeue(out string? text))
            {
                string? result = JsonHelper.Format(text, IndentationMode.Value);
                if (result != null)
                {
                    ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                    {
                        OutputValue = result;

                        if (!_toolSuccessfullyWorked)
                        {
                            _toolSuccessfullyWorked = true;
                            _marketingService.NotifyToolSuccessfullyWorked();
                        }
                    }).ForgetSafely();
                }
            }

            _formattingInProgress = false;
        }
    }
}
