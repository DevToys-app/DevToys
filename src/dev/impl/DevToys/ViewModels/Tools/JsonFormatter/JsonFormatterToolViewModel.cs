#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Models.Enumerations;
using DevToys.Views.Tools.JsonFormatter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.JsonFormatter
{
    [Export(typeof(JsonFormatterToolViewModel))]
    public sealed class JsonFormatterToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// The indentation to apply while formatting.
        /// </summary>
        private static readonly SettingDefinition<string> Indentation
            = new(
                name: $"{nameof(JsonFormatterToolViewModel)}.{nameof(Indentation)}",
                isRoaming: true,
                defaultValue: IndentationEnumeration.TwoSpaceIndentation.Code);

        private readonly Queue<string> _formattingQueue = new();

        private bool _formattingInProgress;
        private string? _inputValue;
        private string? _outputValue;

        public Type View { get; } = typeof(JsonFormatterToolPage);

        internal JsonFormatterStrings Strings => LanguageManager.Instance.JsonFormatter;

        /// <summary>
        /// Gets or sets the desired indentation.
        /// </summary>
        internal IndentationEnumeration IndentationMode
        {
            get
            {
                string? settingsValue = SettingsProvider.GetSetting(Indentation);
                return Enumeration.FromCode<IndentationEnumeration>(settingsValue);
            }
            set
            {
                if (!IndentationMode.Equals(value))
                {
                    SettingsProvider.SetSetting(Indentation, value.Code);
                    OnPropertyChanged();
                    QueueFormatting();
                }
            }
        }

        /// <summary>
        /// Get a list of supported Indentation
        /// </summary>
        internal ObservableCollection<IndentationEnumeration> Indentations => IndentationEnumeration.Gets();

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
        public JsonFormatterToolViewModel(ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;
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

            while (_formattingQueue.TryDequeue(out string text))
            {
                string? result = JsonHelper.Format(text, IndentationMode);
                if (result != null)
                {
                    ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                    {
                        OutputValue = result;
                    }).ForgetSafely();
                }
            }

            _formattingInProgress = false;
        }
    }
}
