#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Helpers;
using DevToys.Views.Tools.JsonYaml;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using DevToys.ViewModels.Tools.Converters.JsonYaml;

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
        private static readonly SettingDefinition<string> Indentation
            = new(
                name: $"{nameof(JsonYamlToolViewModel)}.{nameof(Indentation)}",
                isRoaming: true,
                defaultValue: TwoSpaceIndentation);

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
            get => SettingsProvider.GetSetting(Conversion);
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
        internal string IndentationMode
        {
            get => SettingsProvider.GetSetting(Indentation);
            set
            {
                if (!string.Equals(SettingsProvider.GetSetting(Indentation), value, StringComparison.Ordinal))
                {
                    SettingsProvider.SetSetting(Indentation, value);
                    OnPropertyChanged();
                    QueueConversion();
                }
            }
        }

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
                bool success;
                string result;
                if (string.Equals(ConversionMode, JsonToYaml, StringComparison.Ordinal))
                {
                    success = ConvertJsonToYaml(text, out result);
                }
                else
                {
                    success = ConvertYamlToJson(text, out result);
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OutputValue = result;

                    if (success && !_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private bool ConvertJsonToYaml(string input, out string output)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                output = string.Empty;
                return false;
            }

            try
            {
                dynamic? jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(input, _defaultJsonSerializerSettings);
                if (jsonObject is not null and not string)
                {
                    int indent = 0;
                    indent = IndentationMode switch
                    {
                        TwoSpaceIndentation => 2,
                        FourSpaceIndentation => 4,
                        _ => throw new NotSupportedException(),
                    };
                    var serializer
                        = Serializer.FromValueSerializer(
                            new SerializerBuilder().BuildValueSerializer(),
                            EmitterSettings.Default.WithBestIndent(indent).WithIndentedSequences());

                    string? yaml = serializer.Serialize(jsonObject);
                    output = yaml ?? string.Empty;
                    return true;
                }

                output = string.Empty;
            }
            catch (JsonReaderException ex)
            {
                output = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Json to Yaml Converter", ex);
                output = string.Empty;
            }

            return false;
        }

        private bool ConvertYamlToJson(string input, out string output)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                output = string.Empty;
                return false;
            }

            try
            {
                using var stringReader = new StringReader(input);

                var deserializer = new DeserializerBuilder()
                    .WithNodeTypeResolver(new DecimalYamlTypeResolver())
                    .Build();

                object? yamlObject = deserializer.Deserialize(stringReader);

                if (yamlObject is null or string)
                {
                    output = Strings.InvalidYaml;
                    return false;
                }

                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    switch (IndentationMode)
                    {
                        case TwoSpaceIndentation:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 2;
                            break;

                        case FourSpaceIndentation:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 4;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    var jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
                    {
                        Converters = { new DecimalJsonConverter() }
                    });
                    jsonSerializer.Serialize(jsonTextWriter, yamlObject);
                }

                output = stringBuilder.ToString();
                return true;
            }
            catch (SemanticErrorException ex)
            {
                output = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Yaml to Json Converter", ex);
                output = string.Empty;
            }

            return false;
        }
    }
}
