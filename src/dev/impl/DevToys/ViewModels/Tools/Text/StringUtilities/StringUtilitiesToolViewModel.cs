﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.StringUtilities;
using Microsoft.Toolkit.Mvvm.Input;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(StringUtilitiesToolViewModel))]
    public sealed class StringUtilitiesToolViewModel : QueueWorkerViewModelBase<string>, IToolViewModel
    {
        private static readonly object _lockObject = new();

        private readonly IMarketingService _marketingService;
        private readonly Random _random;

        private bool _toolSuccessfullyWorked;
        private bool _forbidBackup;
        private string? _text;
        private string? _textBackup;
        private string? _wordDistribution;
        private string? _characterDistribution;
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

        public Type View { get; } = typeof(StringUtilitiesToolPage);

        internal StringUtilitiesStrings Strings => LanguageManager.Instance.StringUtilities;

        internal string? Text
        {
            get => _text;
            set
            {
                lock (_lockObject)
                {
                    if (!_forbidBackup)
                    {
                        _textBackup = value;
                    }
                    SetProperty(ref _text, value);
                    OnPropertyChanged(nameof(OriginalCaseCommand));
                    QueueTextStatisticCalculation();
                }
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
        public StringUtilitiesToolViewModel(IMarketingService marketingService)
        {
            _marketingService = marketingService;
            _random = new Random();
            OriginalCaseCommand = new RelayCommand(ExecuteOriginalCaseCommand, CanExecuteOriginalCaseCommand);
            SentenceCaseCommand = new RelayCommand(ExecuteSentenceCaseCommand);
            LowerCaseCommand = new RelayCommand(ExecuteLowerCaseCommand);
            UpperCaseCommand = new RelayCommand(ExecuteUpperCaseCommand);
            TitleCaseCommand = new RelayCommand(ExecuteTitleCaseCommand);
            CamelCaseCommand = new RelayCommand(ExecuteCamelCaseCommand);
            PascalCaseCommand = new RelayCommand(ExecutePascalCaseCommand);
            SnakeCaseCommand = new RelayCommand(ExecuteSnakeCaseCommand);
            ConstantCaseCommand = new RelayCommand(ExecuteConstantCaseCommand);
            KebabCaseCommand = new RelayCommand(ExecuteKebabCaseCommand);
            CobolCaseCommand = new RelayCommand(ExecuteCobolCaseCommand);
            TrainCaseCommand = new RelayCommand(ExecuteTrainCaseCommand);
            AlternatingCaseCommand = new RelayCommand(ExecuteAlternatingCaseCommand);
            InverseCaseCommand = new RelayCommand(ExecuteInverseCaseCommand);
            AlphabetizeCommand = new RelayCommand(ExecuteAlphabetizeCommand);
            ReverseAlphabetizeCommand = new RelayCommand(ExecuteReverseAlphabetizeCommand);
            AlphabetizeLastCommand = new RelayCommand(ExecuteAlphabetizeLastCommand);
            ReverseAlphabetizeLastCommand = new RelayCommand(ExecuteReverseAlphabetizeLastCommand);
            ReverseCommand = new RelayCommand(ExecuteReverseCommand);
            RandomizeCommand = new RelayCommand(ExecuteRandomizeCommand);

            QueueSelectionStatisticCalculation();
            QueueTextStatisticCalculation();
        }

        #region OriginalCaseCommand

        public IRelayCommand OriginalCaseCommand { get; }

        private bool CanExecuteOriginalCaseCommand()
        {
            return !string.IsNullOrEmpty(_textBackup) && !string.Equals(_text, _textBackup, StringComparison.Ordinal);
        }

        private void ExecuteOriginalCaseCommand()
        {
            SetTextWithoutBackup(_textBackup);
        }

        #endregion

        #region SentenceCaseCommand

        public IRelayCommand SentenceCaseCommand { get; }

        private void ExecuteSentenceCaseCommand()
        {
            char[]? sentenceCaseString = _textBackup?.ToCharArray();
            if (sentenceCaseString is null)
            {
                return;
            }

            bool newSentence = true;
            for (int i = 0; i < sentenceCaseString.Length; i++)
            {
                if (sentenceCaseString[i] is '.' or '?' or '!' or '\r')
                {
                    newSentence = true;
                    continue;
                }

                if (char.IsLetterOrDigit(sentenceCaseString[i]))
                {
                    if (newSentence)
                    {
                        sentenceCaseString[i] = char.ToUpperInvariant(sentenceCaseString[i]);
                        newSentence = false;
                    }
                    else
                    {
                        sentenceCaseString[i] = char.ToLowerInvariant(sentenceCaseString[i]);
                    }
                }
            }

            SetTextWithoutBackup(new string(sentenceCaseString));
        }

        #endregion

        #region LowerCaseCommand

        public IRelayCommand LowerCaseCommand { get; }

        private void ExecuteLowerCaseCommand()
        {
            SetTextWithoutBackup(_textBackup?.ToLowerInvariant());
        }

        #endregion

        #region UpperCaseCommand

        public IRelayCommand UpperCaseCommand { get; }

        private void ExecuteUpperCaseCommand()
        {
            SetTextWithoutBackup(_textBackup?.ToUpperInvariant());
        }

        #endregion

        #region TitleCaseCommand

        public IRelayCommand TitleCaseCommand { get; }

        private void ExecuteTitleCaseCommand()
        {
            char[]? titleCaseString = _textBackup?.ToCharArray();
            if (titleCaseString is null)
            {
                return;
            }

            for (int i = 0; i < titleCaseString.Length; i++)
            {
                if (i == 0
                    || !char.IsLetterOrDigit(titleCaseString[i - 1]))
                {
                    titleCaseString[i] = char.ToUpperInvariant(titleCaseString[i]);
                }
                else
                {
                    titleCaseString[i] = char.ToLowerInvariant(titleCaseString[i]);
                }
            }

            SetTextWithoutBackup(new string(titleCaseString));
        }

        #endregion

        #region CamelCaseCommand

        public IRelayCommand CamelCaseCommand { get; }

        private void ExecuteCamelCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            var camelCaseStringBuilder = new StringBuilder();
            bool nextLetterOrDigitShouldBeUppercase = false;

            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if (char.IsLetterOrDigit(currentChar))
                {
                    if (nextLetterOrDigitShouldBeUppercase)
                    {
                        camelCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                        nextLetterOrDigitShouldBeUppercase = false;
                    }
                    else
                    {
                        camelCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                    }
                }
                else
                {
                    if (currentChar == '\r')
                    {
                        nextLetterOrDigitShouldBeUppercase = false;
                        camelCaseStringBuilder.Append(currentChar);
                    }
                    else
                    {
                        nextLetterOrDigitShouldBeUppercase = true;
                    }
                }
            }

            SetTextWithoutBackup(camelCaseStringBuilder.ToString());
        }

        #endregion

        #region PascalCaseCommand

        public IRelayCommand PascalCaseCommand { get; }

        private void ExecutePascalCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            var pascalCaseStringBuilder = new StringBuilder();
            bool nextLetterOrDigitShouldBeUppercase = true;

            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if (char.IsLetterOrDigit(currentChar))
                {
                    if (nextLetterOrDigitShouldBeUppercase)
                    {
                        pascalCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                        nextLetterOrDigitShouldBeUppercase = false;
                    }
                    else
                    {
                        pascalCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                    }
                }
                else
                {
                    nextLetterOrDigitShouldBeUppercase = true;
                    if (currentChar == '\r')
                    {
                        pascalCaseStringBuilder.Append(currentChar);
                    }
                }
            }

            SetTextWithoutBackup(pascalCaseStringBuilder.ToString());
        }

        #endregion

        #region SnakeCaseCommand

        public IRelayCommand SnakeCaseCommand { get; }

        private void ExecuteSnakeCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            string? snakeCase = SnakeConstantKebabCobolCaseConverter(text, '_', isUpperCase: false);

            SetTextWithoutBackup(snakeCase);
        }

        #endregion

        #region ConstantCaseCommand

        public IRelayCommand ConstantCaseCommand { get; }

        private void ExecuteConstantCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            string? constantCase = SnakeConstantKebabCobolCaseConverter(text, '_', isUpperCase: true);

            SetTextWithoutBackup(constantCase);
        }

        #endregion

        #region KebabCaseCommand

        public IRelayCommand KebabCaseCommand { get; }

        private void ExecuteKebabCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            string? kebabCase = SnakeConstantKebabCobolCaseConverter(text, '-', isUpperCase: false);

            SetTextWithoutBackup(kebabCase);
        }

        #endregion

        #region CobolCaseCommand

        public IRelayCommand CobolCaseCommand { get; }

        private void ExecuteCobolCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            string? cobolCase = SnakeConstantKebabCobolCaseConverter(text, '-', isUpperCase: true);

            SetTextWithoutBackup(cobolCase);
        }

        #endregion

        #region TrainCaseCommand

        public IRelayCommand TrainCaseCommand { get; }

        private void ExecuteTrainCaseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            var snakeCaseStringBuilder = new StringBuilder();

            bool nextNonLetterOrDigitShouldBeIgnored = true;
            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if (char.IsLetterOrDigit(currentChar))
                {
                    if (nextNonLetterOrDigitShouldBeIgnored)
                    {
                        snakeCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                    }
                    else
                    {
                        snakeCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                    }
                    nextNonLetterOrDigitShouldBeIgnored = false;
                }
                else if (currentChar == '\r')
                {
                    nextNonLetterOrDigitShouldBeIgnored = true;
                    snakeCaseStringBuilder.Append(currentChar);
                }
                else if (!nextNonLetterOrDigitShouldBeIgnored)
                {
                    if (i < text.Length - 1
                        && char.IsLetterOrDigit(text[i + 1]))
                    {
                        nextNonLetterOrDigitShouldBeIgnored = true;
                        snakeCaseStringBuilder.Append('-');
                    }
                }
            }

            SetTextWithoutBackup(snakeCaseStringBuilder.ToString());
        }

        #endregion

        #region AlternatingCaseCommand

        public IRelayCommand AlternatingCaseCommand { get; }

        private void ExecuteAlternatingCaseCommand()
        {
            char[]? titleCaseString = _textBackup?.ToCharArray();
            if (titleCaseString is null)
            {
                return;
            }

            bool lowerCase = true;
            for (int i = 0; i < titleCaseString.Length; i++)
            {
                if (lowerCase)
                {
                    titleCaseString[i] = char.ToLowerInvariant(titleCaseString[i]);
                }
                else
                {
                    titleCaseString[i] = char.ToUpperInvariant(titleCaseString[i]);
                }

                lowerCase = !lowerCase;
            }

            SetTextWithoutBackup(new string(titleCaseString));
        }

        #endregion

        #region InverseCaseCommand

        public IRelayCommand InverseCaseCommand { get; }

        private void ExecuteInverseCaseCommand()
        {
            char[]? titleCaseString = _textBackup?.ToCharArray();
            if (titleCaseString is null)
            {
                return;
            }

            bool lowerCase = false;
            for (int i = 0; i < titleCaseString.Length; i++)
            {
                if (lowerCase)
                {
                    titleCaseString[i] = char.ToLowerInvariant(titleCaseString[i]);
                }
                else
                {
                    titleCaseString[i] = char.ToUpperInvariant(titleCaseString[i]);
                }

                lowerCase = !lowerCase;
            }

            SetTextWithoutBackup(new string(titleCaseString));
        }

        #endregion

        #region AlphabetizeCommand

        public IRelayCommand AlphabetizeCommand { get; }

        private void ExecuteAlphabetizeCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            IOrderedEnumerable<string> lines = text.Split('\r').OrderBy(line => line, StringComparer.CurrentCulture);
            text = string.Join('\r', lines);

            SetTextWithoutBackup(text);
        }

        #endregion

        #region ReverseAlphabetizeCommand

        public IRelayCommand ReverseAlphabetizeCommand { get; }

        private void ExecuteReverseAlphabetizeCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            IOrderedEnumerable<string> lines = text.Split('\r').OrderByDescending(line => line, StringComparer.CurrentCulture);
            text = string.Join('\r', lines);

            SetTextWithoutBackup(text);
        }

        #endregion

        #region AlphabetizeLastCommand

        public IRelayCommand AlphabetizeLastCommand { get; }

        private void ExecuteAlphabetizeLastCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            var lines = text.Split('\r').ToList();
            lines.Sort((line1, line2) =>
            {
                string line1LastWord = new(line1.Reverse().TakeWhile(char.IsLetterOrDigit).Reverse().ToArray());
                string line2LastWord = new(line2.Reverse().TakeWhile(char.IsLetterOrDigit).Reverse().ToArray());

                return string.Compare(line1LastWord, line2LastWord, StringComparison.CurrentCulture);
            });
            text = string.Join('\r', lines);

            SetTextWithoutBackup(text);
        }

        #endregion

        #region ReverseAlphabetizeLastCommand

        public IRelayCommand ReverseAlphabetizeLastCommand { get; }

        private void ExecuteReverseAlphabetizeLastCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            var lines = text.Split('\r').ToList();
            lines.Sort((line1, line2) =>
            {
                string line1LastWord = new(line1.Reverse().TakeWhile(char.IsLetterOrDigit).Reverse().ToArray());
                string line2LastWord = new(line2.Reverse().TakeWhile(char.IsLetterOrDigit).Reverse().ToArray());

                return string.Compare(line1LastWord, line2LastWord, StringComparison.CurrentCulture) * -1;
            });
            text = string.Join('\r', lines);

            SetTextWithoutBackup(text);
        }

        #endregion

        #region ReverseCommand

        public IRelayCommand ReverseCommand { get; }

        private void ExecuteReverseCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            IEnumerable<string> lines = text.Split('\r').Reverse();
            text = string.Join('\r', lines);

            SetTextWithoutBackup(text);
        }

        #endregion

        #region RandomizeCommand

        public IRelayCommand RandomizeCommand { get; }

        private void ExecuteRandomizeCommand()
        {
            string? text = _textBackup;
            if (text is null)
            {
                return;
            }

            IOrderedEnumerable<string> lines = text.Split('\r').OrderBy(_ => _random.Next());
            text = string.Join('\r', lines);

            SetTextWithoutBackup(text);
        }

        #endregion

        private void SetTextWithoutBackup(string? text)
        {
            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = text;
                _forbidBackup = false;
            }
        }

        private string SnakeConstantKebabCobolCaseConverter(string text, char spaceReplacement, bool isUpperCase)
        {
            var snakeCaseStringBuilder = new StringBuilder();

            bool nextNonLetterOrDigitShouldBeIgnored = true;
            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if (char.IsLetterOrDigit(currentChar))
                {
                    nextNonLetterOrDigitShouldBeIgnored = false;
                    if (isUpperCase)
                    {
                        snakeCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                    }
                    else
                    {
                        snakeCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                    }
                }
                else if (currentChar == '\r')
                {
                    nextNonLetterOrDigitShouldBeIgnored = true;
                    snakeCaseStringBuilder.Append(currentChar);
                }
                else if (!nextNonLetterOrDigitShouldBeIgnored)
                {
                    if (i < text.Length - 1
                        && char.IsLetterOrDigit(text[i + 1]))
                    {
                        nextNonLetterOrDigitShouldBeIgnored = true;
                        snakeCaseStringBuilder.Append(spaceReplacement);
                    }
                }
            }

            return snakeCaseStringBuilder.ToString();
        }

        private void QueueTextStatisticCalculation()
        {
            EnqueueComputation(Text ?? string.Empty);
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

        protected override async Task TreatComputationQueueAsync(string text)
        {
            await TaskScheduler.Default;

            int characters = 0;
            int words = 0;
            int lines = 1;
            int sentences = 0;
            int paragraphs = 1;
            int bytes = 0;
            string? wordDistribution = string.Empty;
            string? characterDistribution = string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                // Characters
                characters = text!.Length;

                // Bytes
                bytes = Encoding.UTF8.GetByteCount(text);

                // Words count
                MatchCollection matches = Regex.Matches(text, @"\b[\w]*\b");
                words = matches.Count(m => m.Length > 0);

                // Word distribution
                var stringBuilder = new StringBuilder();
                IOrderedEnumerable<IGrouping<string, Match>>? wordFrequency
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

                // Paragraphs
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
                    if (currentChar is '.' or '?' or '!' or '\r')
                    {
                        if (!IsSpanEmptyOrNotLetterAndDigit(text, sentenceBeginningPosition, i))
                        {
                            sentences++;
                            sentenceBeginningPosition = i + 1;
                        }
                    }

                    if (currentChar == '\r')
                    {
                        continue;
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

                // Character distributions
                stringBuilder.Clear();
                foreach (KeyValuePair<char, int> item in characterFrequency.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
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

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
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
