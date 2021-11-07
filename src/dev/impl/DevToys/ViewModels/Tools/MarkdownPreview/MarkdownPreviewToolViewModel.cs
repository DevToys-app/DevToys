#nullable enable

using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Core.Theme;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Messages;
using DevToys.Views.Tools.MarkdownPreview;
using Markdig;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.MarkdownPreview
{
    [Export(typeof(MarkdownPreviewToolViewModel))]
    public sealed class MarkdownPreviewToolViewModel : ObservableRecipient, IToolViewModel
    {
        private static string? _htmlDocument;

        private static readonly SettingDefinition<string?> ThemeSetting
            = new(
                name: $"{nameof(MarkdownPreviewToolViewModel)}.{nameof(ThemeSetting)}",
                isRoaming: true,
                defaultValue: null);

        private readonly IMarketingService _marketingService;
        private readonly IThemeListener _themeListener;
        private readonly Queue<string> _workQueue = new();

        private bool _toolSuccessfullyWorked;
        private bool _workInProgress;
        private string? _inputValue;

        public Type View { get; } = typeof(MarkdownPreviewToolPage);

        internal MarkdownPreviewStrings Strings => LanguageManager.Instance.MarkdownPreview;

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? InputValue
        {
            get => _inputValue;
            set
            {
                SetProperty(ref _inputValue, value);
                QueueWork();
            }
        }

        internal ApplicationTheme Theme
        {
            get
            {
                string? theme = SettingsProvider.GetSetting(ThemeSetting);
                if (string.IsNullOrEmpty(theme))
                {
                    theme = _themeListener.ActualAppTheme.ToString();
                }
                return (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), theme);
            }
            set
            {
                if (!string.Equals(SettingsProvider.GetSetting(ThemeSetting), value.ToString()))
                {
                    SettingsProvider.SetSetting(ThemeSetting, value.ToString());
                    OnPropertyChanged();
                    QueueWork();
                }
            }
        }

        internal ISettingsProvider SettingsProvider { get; }

        [ImportingConstructor]
        public MarkdownPreviewToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService, IThemeListener themeListener)
        {
            _marketingService = marketingService;
            _themeListener = themeListener;
            SettingsProvider = settingsProvider;

            // Activate the view model's messenger.
            IsActive = true;

            QueueWork();
        }

        private void QueueWork()
        {
            _workQueue.Enqueue(InputValue ?? string.Empty);
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_workInProgress)
            {
                return;
            }

            _workInProgress = true;

            await TaskScheduler.Default;

            while (_workQueue.TryDequeue(out string text))
            {
                string html = await MarkdownToHtmlPageAsync(text, Theme);

                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    Messenger.Send(new NavigateToMarkdownPreviewHtmlMessage(html));

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _workInProgress = false;
        }

        internal async Task<string> MarkdownToHtmlPageAsync(string markdown, ApplicationTheme theme)
        {
            await TaskScheduler.Default;

            MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseEmojiAndSmiley().UseSmartyPants().UseAdvancedExtensions().Build();
            string htmlBody = Markdown.ToHtml(markdown, pipeline);

            if (string.IsNullOrEmpty(_htmlDocument))
            {
                StorageFile indexFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/GitHubMarkdownCss/index.html"));
                _htmlDocument = await FileIO.ReadTextAsync(indexFile);
            }

            Assumes.NotNullOrEmpty(_htmlDocument, nameof(_htmlDocument));

            string htmlDocument
                = ((string)_htmlDocument!.Clone())
                .Replace("{{renderTheme}}", theme.ToString().ToLower())
                .Replace("{{backgroundColor}}", theme == ApplicationTheme.Dark ? "#0d1117" : "#ffffff")
                .Replace("{{htmlBody}}", htmlBody);

            return htmlDocument;
        }
    }
}
