#nullable enable

using DevTools.Common;
using DevTools.Core.Settings;
using DevTools.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;

namespace DevTools.Providers.Impl.Tools.JsonFormatter
{
    [Export(typeof(JsonFormatterToolViewModel))]
    public sealed class JsonFormatterToolViewModel : ObservableRecipient, IToolViewModel
    {
        private const string DefaultIndentation = "TwoSpaces";

        private readonly IThread _thread;
        private readonly Queue<string> _formattingQueue = new ();

        private bool _formattingInProgress;
        private string? _inputValue;
        private string _indentation = DefaultIndentation;

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

        internal ISettingsProvider SettingsProvider { get; }

        [ImportingConstructor]
        public JsonFormatterToolViewModel(IThread thread, ISettingsProvider settingsProvider)
        {
            _thread = thread;
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
                //Task<string>? conversionResult = null;
                //if (string.Equals(ConversionMode, DefaultConversion, StringComparison.Ordinal))
                //{
                //    conversionResult = EncodeBase64DataAsync(text);
                //}
                //else
                //{
                //    conversionResult = DecodeBase64DataAsync(text);
                //}

                //await Task.WhenAll(conversionResult).ConfigureAwait(false);

                //_thread.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                //{
                //    OutputValue = conversionResult.Result;
                //}).ForgetSafely();
            }

            _formattingInProgress = false;
        }
    }
}
