using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.LoremIpsumGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using NLipsum.Core;
using System.Linq;

namespace DevToys.ViewModels.Tools.LoremIpsumGenerator
{
    [Export(typeof(LoremIpsumGeneratorToolViewModel))]
    public sealed class LoremIpsumGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        private static readonly SettingDefinition<string> Type
            = new(
                name: $"{nameof(LoremIpsumGeneratorToolViewModel)}.{nameof(Type)}",
                isRoaming: true,
                defaultValue: DefaultType);

        private static readonly SettingDefinition<int> Length
            = new(
                name: $"{nameof(LoremIpsumGeneratorToolViewModel)}.{nameof(Length)}",
                isRoaming: true,
                defaultValue: 1);

        /// <summary>
        /// Whether the generated text should start with Lorem ipsum dolor sit amet
        /// </summary>
        private static readonly SettingDefinition<bool> StartWithLoremIpsum
            = new(
                name: $"{nameof(LoremIpsumGeneratorToolViewModel)}.{nameof(StartWithLoremIpsum)}",
                isRoaming: true,
                defaultValue: false);

        private const string LoremIpsumStartText = "Lorem ipsum dolor sit amet";

        private const string ParagraphsType = "Paragraphs";
        private const string SentencesType = "Sentences";
        private const string WordsType = "Words";

        private const string DefaultType = ParagraphsType;

        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly Queue<(string Type, int Length, bool StartWithLoremIpsum)> _generationQueue = new();

        private string _output = string.Empty;
        private bool _generationInProgress;
        private bool _toolSuccessfullyWorked;

        public Type View => typeof(LoremIpsumGeneratorToolPage);

        internal LoremIpsumGeneratorStrings Strings => LanguageManager.Instance.LoremIpsumGenerator;

        internal string LoremIpsumType
        {
            get => _settingsProvider.GetSetting(Type);
            set
            {
                if (_settingsProvider.GetSetting(Type) != value)
                {
                    _settingsProvider.SetSetting(Type, value);
                    OnPropertyChanged();
                    QueueGeneration();
                }
            }
        }

        internal int LoremIpsumLength
        {
            get => _settingsProvider.GetSetting(Length);
            set
            {
                if (_settingsProvider.GetSetting(Length) != value)
                {
                    _settingsProvider.SetSetting(Length, value);
                    OnPropertyChanged();
                    QueueGeneration();
                }
            }
        }

        internal bool StartWithLorem
        {
            get => _settingsProvider.GetSetting(StartWithLoremIpsum);
            set
            {
                if (_settingsProvider.GetSetting(StartWithLoremIpsum) != value)
                {
                    _settingsProvider.SetSetting(StartWithLoremIpsum, value);
                    OnPropertyChanged();
                    QueueGeneration();
                }
            }
        }

        internal string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        [ImportingConstructor]
        public LoremIpsumGeneratorToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;

            QueueGeneration();
        }

        private void QueueGeneration()
        {
            _generationQueue.Enqueue((LoremIpsumType, LoremIpsumLength, StartWithLorem));
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_generationInProgress)
            {
                return;
            }

            _generationInProgress = true;

            await TaskScheduler.Default;

            while (_generationQueue.TryDequeue(out (string Type, int Length, bool StartWithLoremIpsum) options))
            {
                string output = GenerateLipsum(options.Type, options.Length, options.StartWithLoremIpsum);

                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    Output = output;

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _generationInProgress = false;
        }

        private string GenerateLipsum(string type, int length, bool startWithLoremIpsum)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            var generator = new LipsumGenerator();
            string startWords = startWithLoremIpsum ? LoremIpsumStartText : string.Empty;

            switch (type)
            {
                case WordsType:
                    string[] words = generator.GenerateWords(length);
                    words[0] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[0]);
                    return ApplyStartWords(startWords, string.Join(' ', words));
                case SentencesType:
                    return ApplyStartWords(startWords, string.Join(' ', generator.GenerateSentences(length, Sentence.Medium)));
                case ParagraphsType:
                    return ApplyStartWords(startWords, string.Join($"{Environment.NewLine}{Environment.NewLine}",
                        generator.GenerateParagraphs(length, Paragraph.Medium)));
                default:
                    return string.Empty;
            }
        }

        private string ApplyStartWords(string startText, string originalText)
        {
            if (string.IsNullOrWhiteSpace(startText))
            {
                return originalText;
            }

            char space = ' ';
            string[] startTokens = (startText ?? "").Split(space, StringSplitOptions.RemoveEmptyEntries);
            string[] endTokens = (originalText ?? "").Split(space, StringSplitOptions.RemoveEmptyEntries);
            int wordsNeeded = Math.Min(startTokens.Length, endTokens.Length);

            return string.Join(space, startTokens.Take(wordsNeeded).Concat(endTokens.Skip(wordsNeeded)));
        }
    }
}
