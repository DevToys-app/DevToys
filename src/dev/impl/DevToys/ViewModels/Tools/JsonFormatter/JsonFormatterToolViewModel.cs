#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Views.Tools.JsonFormatter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.JsonFormatter
{
    [Export(typeof(JsonFormatterToolViewModel))]
    public sealed class JsonFormatterToolViewModel : ObservableRecipient, IToolViewModel
    {
        private const string TwoSpaceIndentation = "TwoSpaces";
        private const string FourSpaceIndentation = "FourSpaces";
        private const string OneTabIndentation = "OneTab";
        private const string NoIndentation = "Minified";

        private readonly Queue<string> _formattingQueue = new();

        private bool _formattingInProgress;
        private string? _inputValue;
        private string? _outputValue;
        private string _indentation = TwoSpaceIndentation;

        public Type View { get; } = typeof(JsonFormatterToolPage);

        internal JsonFormatterStrings Strings => LanguageManager.Instance.JsonFormatter;

        /// <summary>
        /// Gets or sets the desired indentation.
        /// </summary>
        internal string Indentation
        {
            get => _indentation;
            set
            {
                SetProperty(ref _indentation, value);
                QueueFormatting();
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
                var success = FormatJson(text, out string result);

                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OutputValue = result;
                }).ForgetSafely();
            }

            _formattingInProgress = false;
        }

        private bool FormatJson(string input, out string output)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                output = string.Empty;
                return false;
            }

            try
            {
                JToken? jtoken = JToken.Parse(input);
                if (jtoken is not null)
                {
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

                            case OneTabIndentation:
                                jsonTextWriter.Formatting = Formatting.Indented;
                                jsonTextWriter.IndentChar = '\t';
                                jsonTextWriter.Indentation = 1;
                                break;

                            case NoIndentation:
                                jsonTextWriter.Formatting = Formatting.None;
                                break;

                            default:
                                throw new NotSupportedException();
                        }

                        jtoken.WriteTo(jsonTextWriter);
                    }

                    output = stringBuilder.ToString();
                    return true;
                }

                output = string.Empty;
                return false;
            }
            catch (JsonReaderException ex)
            {
                output = ex.Message;
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Logger.LogFault("Json formatter", ex, $"Indentation: {Indentation}");
                output = ex.Message;
                return false;
            }
        }
    }
}
