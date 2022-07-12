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
using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.JsonYaml;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    [Export(typeof(JsonYamlToolViewModel))]
    public sealed class JsonYamlToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the tool should convert JSON to YAML or YAML to JSON.
        /// </summary>
        private static readonly SettingDefinition<string> Conversion
            = new(
                name: $"{nameof(JsonYamlToolViewModel)}.{nameof(Conversion)}",
                isRoaming: true,
                defaultValue: JsonToYaml);

        /// <summary>
        /// The indentation to apply while converting.
        /// </summary>
        private static readonly SettingDefinition<Indentation> Indentation
            = new(
                name: $"{nameof(JsonYamlToolViewModel)}.{nameof(Indentation)}",
                isRoaming: true,
                defaultValue: Models.Indentation.TwoSpaces);

        internal const string JsonToYaml = nameof(JsonToYaml);
        internal const string YamlToJson = nameof(YamlToJson);
        private const string TwoSpaceIndentation = "TwoSpaces";
        private const string FourSpaceIndentation = "FourSpaces";

        private readonly IMarketingService _marketingService;
        private readonly Queue<string> _conversionQueue = new();

        private readonly JsonSerializerSettings _defaultJsonSerializerSettings = new()
        {
            FloatParseHandling = FloatParseHandling.Decimal
        };

        private bool _toolSuccessfullyWorked;
        private bool _conversionInProgress;
        private bool _setPropertyInProgress;
        private string? _inputValue;
        private string? _inputValueLanguage;
        private string? _outputValue;
        private string? _outputValueLanguage;

        public Type View { get; } = typeof(JsonYamlToolPage);

        internal JsonYamlStrings Strings => LanguageManager.Instance.JsonYaml;

        /// <summary>
        /// Gets or sets the desired conversion mode.
        /// </summary>
        internal string ConversionMode
        {
            get
            {
                string? current = SettingsProvider.GetSetting(Conversion);
                if (string.IsNullOrWhiteSpace(current) ||
                    string.Equals(current, JsonToYaml, StringComparison.Ordinal))
                {
                    InputValueLanguage = "json";
                    OutputValueLanguage = "yaml";
                    return JsonToYaml;
                }
                InputValueLanguage = "yaml";
                OutputValueLanguage = "json";
                return YamlToJson;
            }
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (!string.Equals(SettingsProvider.GetSetting(Conversion), value, StringComparison.Ordinal))
                    {
                        SettingsProvider.SetSetting(Conversion, value);
                        OnPropertyChanged();

                        if (string.Equals(value, JsonToYaml))
                        {
                            if (JsonHelper.IsValid(OutputValue))
                            {
                                InputValue = OutputValue;
                            }

                            InputValueLanguage = "json";
                            OutputValueLanguage = "yaml";
                        }
                        else
                        {
                            if (YamlHelper.IsValidYaml(OutputValue))
                            {
                                InputValue = OutputValue;
                            }

                            InputValueLanguage = "yaml";
                            OutputValueLanguage = "json";
                        }
                    }

                    _setPropertyInProgress = false;
                }
            }
        }

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
                    QueueConversion();
                }
            }
        }

        /// <summary>
        /// Get a list of supported Indentation
        /// </summary>
        internal IReadOnlyList<IndentationDisplayPair> Indentations = new ObservableCollection<IndentationDisplayPair> {
            Models.IndentationDisplayPair.TwoSpaces,
            Models.IndentationDisplayPair.FourSpaces
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
                QueueConversion();
            }
        }

        /// <summary>
        /// Gets or sets the input code editor's language.
        /// </summary>
        internal string? InputValueLanguage
        {
            get => _inputValueLanguage;
            set => SetProperty(ref _inputValueLanguage, value);
        }

        /// <summary>
        /// Gets or sets the output text.
        /// </summary>
        internal string? OutputValue
        {
            get => _outputValue;
            set => SetProperty(ref _outputValue, value);
        }

        /// <summary>
        /// Gets or sets the output code editor's language.
        /// </summary>
        internal string? OutputValueLanguage
        {
            get => _outputValueLanguage;
            set => SetProperty(ref _outputValueLanguage, value);
        }

        internal ISettingsProvider SettingsProvider { get; }

        [ImportingConstructor]
        public JsonYamlToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            _marketingService = marketingService;
            InputValueLanguage = "json";
            OutputValueLanguage = "yaml";
        }

        private void QueueConversion()
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
                string? result;
                if (string.Equals(ConversionMode, JsonToYaml, StringComparison.Ordinal))
                {
                    result = YamlHelper.ConvertFromJson(text, IndentationMode.Value);
                    if (string.IsNullOrEmpty(result))
                    {
                        result = Strings.InvalidYaml;
                    }
                }
                else
                {
                    result = JsonHelper.ConvertFromYaml(text, IndentationMode.Value);
                    if (string.IsNullOrEmpty(result))
                    {
                        result = Strings.InvalidYaml;
                    }
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OutputValue = result;

                    if (!string.IsNullOrWhiteSpace(result) && !_toolSuccessfullyWorked)
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
