#nullable enable

using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Views.Tools.StringUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
        private static readonly object _lockObject = new();

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
        public StringUtilitiesToolViewModel()
        {
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
            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = _textBackup;
                _forbidBackup = false;
            }
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

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = new string(sentenceCaseString);
                _forbidBackup = false;
            }
        }

        #endregion

        #region LowerCaseCommand

        public IRelayCommand LowerCaseCommand { get; }

        private void ExecuteLowerCaseCommand()
        {
            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = _textBackup?.ToLowerInvariant();
                _forbidBackup = false;
            }
        }

        #endregion

        #region UpperCaseCommand

        public IRelayCommand UpperCaseCommand { get; }

        private void ExecuteUpperCaseCommand()
        {
            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = _textBackup?.ToUpperInvariant();
                _forbidBackup = false;
            }
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

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = new string(titleCaseString);
                _forbidBackup = false;
            }
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
            var nextLetterOrDigitShouldBeUppercase = false;

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

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = camelCaseStringBuilder.ToString();
                _forbidBackup = false;
            }
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
            var nextLetterOrDigitShouldBeUppercase = true;

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

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = pascalCaseStringBuilder.ToString();
                _forbidBackup = false;
            }
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

            var snakeCase = SnakeConstantKebabCobolCaseConverter(text, '_', isUpperCase: false);

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = snakeCase;
                _forbidBackup = false;
            }
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

            var constantCase = SnakeConstantKebabCobolCaseConverter(text, '_', isUpperCase: true);

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = constantCase;
                _forbidBackup = false;
            }
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

            var kebabCase = SnakeConstantKebabCobolCaseConverter(text, '-', isUpperCase: false);

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = kebabCase;
                _forbidBackup = false;
            }
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

            var cobolCase = SnakeConstantKebabCobolCaseConverter(text, '-', isUpperCase: true);

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = cobolCase;
                _forbidBackup = false;
            }
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

            var nextNonLetterOrDigitShouldBeIgnored = true;
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


            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = snakeCaseStringBuilder.ToString();
                _forbidBackup = false;
            }
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

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = new string(titleCaseString);
                _forbidBackup = false;
            }
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

            lock (_lockObject)
            {
                _forbidBackup = true;
                Text = new string(titleCaseString);
                _forbidBackup = false;
            }
        }

        #endregion

        private string SnakeConstantKebabCobolCaseConverter(string text, char spaceReplacement, bool isUpperCase)
        {
            var snakeCaseStringBuilder = new StringBuilder();

            var nextNonLetterOrDigitShouldBeIgnored = true;
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
                // Characters
                characters = text!.Length;

                // Bytes
                bytes = Encoding.UTF8.GetByteCount(text);

                // Words count
                MatchCollection matches = Regex.Matches(text, @"\b[\w]*\b");
                words = matches.Count(m => m.Length > 0);

                // Word distribution
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
