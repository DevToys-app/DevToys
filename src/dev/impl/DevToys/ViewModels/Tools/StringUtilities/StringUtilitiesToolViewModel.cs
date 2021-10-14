#nullable enable

using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Views.Tools.StringUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(StringUtilitiesToolViewModel))]
    public sealed class StringUtilitiesToolViewModel : ObservableRecipient, IToolViewModel
    {
        private bool _calculationTextStatisticsInProgress;
        private bool _calculationSelectionStatisticsInProgress;
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
            if (_calculationTextStatisticsInProgress)
            {
                return;
            }

            _calculationTextStatisticsInProgress = true;

            await TaskScheduler.Default;
        }

        private async Task CalculateSelectionStatisticsAsync(string text, int selectionStart)
        {
            if (_calculationSelectionStatisticsInProgress)
            {
                return;
            }

            _calculationSelectionStatisticsInProgress = true;

            await TaskScheduler.Default;

            
        }

        private static int CountLines(ReadOnlySpan<char> text, int length)
        {
            int lineCount = 1;

            if (string.IsNullOrEmpty(text))
            {
                return lineCount;
            }

            int index = -1;
            int count = 0;
            while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1)))
                count++;

            return count + 1;
        }
    }
}
