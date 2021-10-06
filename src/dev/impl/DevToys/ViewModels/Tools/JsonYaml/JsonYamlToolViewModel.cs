#nullable enable

using ColorCode;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.UI.Controls.FormattedTextBlock;
using DevToys.UI.Controls.FormattedTextBlock.Languages;
using DevToys.Views.Tools.JsonYaml;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    [Export(typeof(JsonYamlToolViewModel))]
    public sealed class JsonYamlToolViewModel : ObservableRecipient, IToolViewModel
    {
        internal const string JsonToYaml = nameof(JsonToYaml);
        internal const string YamlToJson = nameof(YamlToJson);
        private const string TwoSpaceIndentation = "TwoSpaces";
        private const string FourSpaceIndentation = "FourSpaces";

        private readonly Queue<string> _conversionQueue = new();

        private bool _conversionInProgress;
        private bool _setPropertyInProgress;
        private string? _inputValue;
        private string? _outputValue;
        private string _indentation = TwoSpaceIndentation;
        private string _conversionMode = JsonToYaml;

        public Type View { get; } = typeof(JsonYamlToolPage);

        internal JsonYamlStrings Strings => LanguageManager.Instance.JsonYaml;

        /// <summary>
        /// Gets or sets the desired conversion mode.
        /// </summary>
        internal string ConversionMode
        {
            get => _conversionMode;
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;
                    ThreadHelper.ThrowIfNotOnUIThread();
                    SetProperty(ref _conversionMode, value);

                    if (string.Equals(value, JsonToYaml))
                    {
                        if (JsonHelper.IsValidJson(_outputValue))
                        {
                            InputValue = _outputValue;
                        }
                    }
                    else
                    {
                        if (YamlHelper.IsValidYaml(_outputValue))
                        {
                            InputValue = _outputValue;
                        }
                    }

                    _setPropertyInProgress = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired indentation.
        /// </summary>
        internal string Indentation
        {
            get => _indentation;
            set
            {
                SetProperty(ref _indentation, value);
                QueueConversion();
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

        internal ISettingsProvider SettingsProvider { get; }

        internal IFormattedTextBlock? OutputTextBlock { private get; set; }

        [ImportingConstructor]
        public JsonYamlToolViewModel(ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;

            Languages.Load(new Yaml());
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

            Assumes.NotNull(OutputTextBlock, nameof(OutputTextBlock));

            _conversionInProgress = true;

            await TaskScheduler.Default;

            while (_conversionQueue.TryDequeue(out string text))
            {
                bool success;
                string result;
                ILanguage language;
                if (string.Equals(ConversionMode, JsonToYaml, StringComparison.Ordinal))
                {
                    success = ConvertJsonToYaml(text, out result);
                    language = Languages.FindById("yaml");
                }
                else
                {
                    success = ConvertYamlToJson(text, out result);
                    language = Languages.JavaScript;
                }

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    _outputValue = result;
                    if (success)
                    {
                        OutputTextBlock!.SetText(result, language);
                    }
                    else
                    {
                        OutputTextBlock!.SetText(result);
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
                dynamic jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(input);
                if (jsonObject is not null && jsonObject is not string)
                {
                    int indent = 0;
                    switch (Indentation)
                    {
                        case TwoSpaceIndentation:
                            indent = 2;
                            break;

                        case FourSpaceIndentation:
                            indent = 4;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

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
                Logger.LogFault("Yaml to Json Converter", ex);
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

                var deserializer = new Deserializer();
                object? yamlObject = deserializer.Deserialize(stringReader);

                if (yamlObject is null || yamlObject is string)
                {
                    output = Strings.InvalidYaml;
                    return false;
                }

                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    switch (Indentation)
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

                    var jsonSerializer = new JsonSerializer();
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
