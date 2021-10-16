#nullable enable

using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Views.Tools.StringUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(StringUtilitiesToolViewModel))]
    public sealed class StringUtilitiesToolViewModel : ObservableRecipient, IToolViewModel
    {
        private string? _text;
        private int _selectionStart;
        private int _line;
        private int _column;
        private int _position;
        private int _characters;
        private int _words;
        private int _lines;
        private int _sentences;
        private int _paragraphs;
        private int _bytes;
        private string? _wordDistribution;
        private string? _characterDistribution;

        public Type View { get; } = typeof(StringUtilitiesToolPage);

        internal StringUtilitiesStrings Strings => LanguageManager.Instance.StringUtilities;

        internal string? Text
        {
            get => _text;
            set
            {
                SetProperty(ref _text, value);
                QueueTextStatisticCalculation();
            }
        }

        internal int SelectionStart
        {
            get => _selectionStart;
            set
            {
                SetProperty(ref _selectionStart, value);
                QueueSelectionStatisticCalculation();
            }
        }

        internal int Line
        {
            get => _line;
            set => SetProperty(ref _line, value);
        }

        internal int Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        internal int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        internal int Characters
        {
            get => _characters;
            set => SetProperty(ref _characters, value);
        }

        internal int Words
        {
            get => _words;
            set => SetProperty(ref _words, value);
        }

        internal int Lines
        {
            get => _lines;
            set => SetProperty(ref _lines, value);
        }

        internal int Sentences
        {
            get => _sentences;
            set => SetProperty(ref _sentences, value);
        }

        internal int Paragraphs
        {
            get => _paragraphs;
            set => SetProperty(ref _paragraphs, value);
        }

        internal int Bytes
        {
            get => _bytes;
            set => SetProperty(ref _bytes, value);
        }

        internal string? WordDistribution
        {
            get => _wordDistribution;
            set => SetProperty(ref _wordDistribution, value);
        }

        internal string? CharacterDistribution
        {
            get => _characterDistribution;
            set => SetProperty(ref _characterDistribution, value);
        }

        [ImportingConstructor]
        public StringUtilitiesToolViewModel()
        {
            QueueSelectionStatisticCalculation();
            QueueTextStatisticCalculation();
        }

        private void QueueTextStatisticCalculation()
        {
            CalculateTextStatisticsAsync(Text).Forget();
        }

        private void QueueSelectionStatisticCalculation()
        {
            if (string.IsNullOrEmpty(Text))
            {
                Line = 1;
                Column = 0;
                return;
            }

            CalculateSelectionStatisticsAsync(Text!, SelectionStart).Forget();
        }

        private async Task CalculateTextStatisticsAsync(string? text)
        {
            await TaskScheduler.Default;

            int characters = 0;
            int words = 0;
            int lines = 1;
            int sentences = 0;
            int paragraphs = 1;
            int bytes = 0;
            string wordDistribution = string.Empty;
            string characterDistribution = string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                characters = text!.Length;
                bytes = Encoding.UTF8.GetByteCount(text);

                MatchCollection matches = Regex.Matches(text, @"\b[\w]*\b");
                words = matches.Count(m => m.Length > 0);

                var stringBuilder = new StringBuilder();
                var wordFrequency
                    = matches
                    .Where(m => m.Length > 0)
                    .GroupBy(m => m.Value)
                    .OrderByDescending(m => m.Count())
                    .ThenBy(m => m.Key);
                foreach (IGrouping<string, Match>? item in wordFrequency)
                {
                    if (item is not null)
                    {
                        stringBuilder.AppendLine($"{item.Key}: {item.Count()}");
                    }
                }
                wordDistribution = stringBuilder.ToString();

                matches = Regex.Matches(text, @"[^\r\n]*[^ \r\n]+[^\r\n]*((\r|\n|\r\n)[^\r\n]*[^ \r\n]+[^\r\n]*)*");
                paragraphs = matches.Count(m => m.Length > 0);

                var characterFrequency = new Dictionary<char, int>();
                int sentenceBeginningPosition = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    char currentChar = text[i];

                    // Detect lines
                    if (currentChar == '\r')
                    {
                        lines++;
                    }

                    // Detect sentences.
                    if (currentChar == '.'
                        || currentChar == '?'
                        || currentChar == '!'
                        || currentChar == '\r')
                    {
                        if (!IsSpanEmptyOrNotLetterAndDigit(text, sentenceBeginningPosition, i))
                        {
                            sentences++;
                            sentenceBeginningPosition = i + 1;
                        }
                    }

                    if (currentChar == ' ')
                    {
                        currentChar = '⎵';
                    }

                    if (!characterFrequency.TryAdd(currentChar, 1))
                    {
                        characterFrequency[currentChar]++;
                    }
                }

                if (sentenceBeginningPosition < text.Length - 1
                    && !IsSpanEmptyOrNotLetterAndDigit(text, sentenceBeginningPosition, text.Length))
                {
                    sentences++;
                }

                stringBuilder.Clear();
                foreach (var item in characterFrequency.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
                {
                    stringBuilder.AppendLine($"{item.Key}: {item.Value}");
                }
                characterDistribution = stringBuilder.ToString();
            }

            await ThreadHelper.RunOnUIThreadAsync(
                () =>
                {
                    Characters = characters;
                    Words = words;
                    Lines = lines;
                    Sentences = sentences;
                    Paragraphs = paragraphs;
                    Bytes = bytes;
                    WordDistribution = wordDistribution;
                    CharacterDistribution = characterDistribution;
                });
        }

        private async Task CalculateSelectionStatisticsAsync(string text, int selectionStart)
        {
            await TaskScheduler.Default;

            int column;
            int line = CountLines(text, selectionStart);
            int beginningOfLinePosition = text.LastIndexOf('\r', Math.Max(0, Math.Min(selectionStart - 1, text.Length - 1)));
            if (beginningOfLinePosition == -1)
            {
                column = selectionStart;
            }
            else
            {
                column = Math.Max(selectionStart - beginningOfLinePosition - 1, 0);
            }

            await ThreadHelper.RunOnUIThreadAsync(
                () =>
                {
                    Line = line;
                    Column = column;
                });
        }

        private int CountLines(string text, int length)
        {
            int lineCount = 1;

            if (string.IsNullOrEmpty(text))
            {
                return lineCount;
            }

            int index = -1;
            while (-1 != (index = text.IndexOf('\r', index + 1, length - (index + 1))))
            {
                lineCount++;
            }

            return lineCount;
        }

        private bool IsSpanEmptyOrNotLetterAndDigit(string text, int start, int end)
        {
            if (start < 0 || end > text.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = start; i < end; i++)
            {
                char currentChar = text[i];
                if (char.IsLetterOrDigit(currentChar))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
