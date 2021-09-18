#nullable enable

using DevTools.Common;
using DevTools.Common.UI.Controls;
using DevTools.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;

namespace DevTools.Providers.Impl.Tools.RegEx
{
    [Export(typeof(RegExToolViewModel))]
    public sealed class RegExToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly IThread _thread;
        private readonly Queue<(string pattern, string text)> _regExMatchingQueue = new();

        private bool _calculationInProgress;
        private string? _regularExpression;
        private string? _text;

        public Type View { get; } = typeof(RegExToolPage);

        internal RegExStrings Strings => LanguageManager.Instance.RegEx;

        internal string? RegularExpression
        {
            get => _regularExpression;
            set
            {
                SetProperty(ref _regularExpression, value);
                QueueRegExMatch();
            }
        }

        internal string? Text
        {
            get => _text;
            set
            {
                SetProperty(ref _text, value);
                QueueRegExMatch();
            }
        }

        internal ICustomTextBox? MatchTextBox { private get; set; }

        [ImportingConstructor]
        public RegExToolViewModel(IThread thread)
        {
            _thread = thread;
        }

        private void QueueRegExMatch()
        {
            _regExMatchingQueue.Enqueue(new(RegularExpression ?? string.Empty, Text ?? string.Empty));
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_calculationInProgress)
            {
                return;
            }

            _calculationInProgress = true;

            await TaskScheduler.Default;

            while (_regExMatchingQueue.TryDequeue(out (string pattern, string text) data))
            {
                var spans = new List<HighlightSpan>();

                try
                {
                    string pattern = data.pattern.Trim('/');

                    var regex = new Regex(data.pattern, RegexOptions.Multiline);
                    MatchCollection matches = regex.Matches(data.text.Replace("\r\n", "\r"));

                    foreach (Match match in matches)
                    {
                        spans.Add(
                            new HighlightSpan()
                            {
                                StartIndex = match.Index,
                                Length = match.Length,
                                BackgroundColor = Colors.Yellow,
                                ForegroundColor = Colors.Black
                            });
                    }
                }
                catch
                {
                    // TODO: indicate the user that the regex is wrong.
                }

                _thread.RunOnUIThreadAsync(
                    ThreadPriority.Low, 
                    () =>
                    {
                        MatchTextBox?.SetHighlights(spans);
                    }).ForgetSafely();
            }

            _calculationInProgress = false;
        }
    }
}
