#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Reflection;
using System.Threading.Tasks;
using DevToys.Api.Core.Navigation;
using DevToys.Api.Core.Settings;
using DevToys.Api.Core.Theme;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Settings;
using DevToys.Core.Threading;
using DevToys.Views.Tools.Settings;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Services.Store;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace DevToys.ViewModels.Settings
{
    [Export(typeof(SettingsToolViewModel))]
    public sealed class SettingsToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly IWindowManager _windowManager;
        private readonly ISettingsProvider _settingsProvider;

        public Type View { get; } = typeof(SettingsToolPage);

        internal SettingsStrings Strings => LanguageManager.Instance.Settings;

        internal List<LanguageDefinition> AvailableLanguages => LanguageManager.Instance.AvailableLanguages;

        internal string Language
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.Language);
            set => _settingsProvider.SetSetting(PredefinedSettings.Language, value);
        }

        internal AppTheme Theme
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.Theme);
            set => _settingsProvider.SetSetting(PredefinedSettings.Theme, value);
        }

        internal bool SmartDetection
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.SmartDetection);
            set => _settingsProvider.SetSetting(PredefinedSettings.SmartDetection, value);
        }

        internal bool SmartDetectionPaste
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.SmartDetectionPaste);
            set => _settingsProvider.SetSetting(PredefinedSettings.SmartDetectionPaste, value);
        }

        internal ObservableCollection<string> SupportedFonts { get; } = new();

        internal string TextEditorFont
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorFont, value);
        }

        internal bool TextEditorTextWrapping
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorTextWrapping, value);
        }

        internal bool TextEditorLineNumbers
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorLineNumbers, value);
        }

        internal bool TextEditorHighlightCurrentLine
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorHighlightCurrentLine, value);
        }

        internal bool TextEditorRenderWhitespace
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorRenderWhitespace);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorRenderWhitespace, value);
        }

        /// <summary>
        /// Gets the version of the application.
        /// </summary>
        internal string Version
        {
            get
            {
                string? version = Strings.GetFormattedVersion(typeof(SettingsToolViewModel).GetTypeInfo().Assembly.GetName().Version.ToString());

                string? architecture = Package.Current.Id.Architecture.ToString();
#if DEBUG
                string? buildConfiguration = "DEBUG";
#else
                string buildConfiguration = "RELEASE";
#endif

                string? gitBranch = ThisAssembly.Git.Branch;
                string? gitCommit = ThisAssembly.Git.Commit;

                return $"{version} | {architecture} | {buildConfiguration} | {gitBranch} | {gitCommit}";
            }
        }

        [ImportingConstructor]
        public SettingsToolViewModel(
            IWindowManager windowManager,
            ISettingsProvider settingsProvider)
        {
            _windowManager = windowManager;
            _settingsProvider = settingsProvider;

            CopyVersionCommand = new RelayCommand(ExecuteCopyVersionCommand);
            PrivacyPolicyCommand = new AsyncRelayCommand(ExecutePrivacyPolicyCommandAsync);
            ThirdPartyNoticesCommand = new AsyncRelayCommand(ExecuteThirdPartyNoticesCommandAsync);
            LicenseCommand = new AsyncRelayCommand(ExecuteLicenseCommandAsync);
            RateAndReviewCommand = new AsyncRelayCommand(ExecuteRateAndReviewCommandAsync);
            OpenLogsCommand = new AsyncRelayCommand(ExecuteOpenLogsCommandAsync);

            LoadFonts();
        }

        #region CopyVersionCommand

        internal IRelayCommand CopyVersionCommand { get; }

        private void ExecuteCopyVersionCommand()
        {
            try
            {
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetText(Version);

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                Logger.LogFault($"Failed to copy data from copy version command, version: ${Version}", ex);
            }
        }

        #endregion

        #region PrivacyPolicyCommand

        internal IAsyncRelayCommand PrivacyPolicyCommand { get; }

        private async Task ExecutePrivacyPolicyCommandAsync()
        {
            await _windowManager.ShowContentDialogAsync(
                new MarkdownContentDialog(
                    await AssetsHelper.GetPrivacyStatementAsync()),
                Strings.Close,
                title: Strings.PrivacyPolicy);
        }

        #endregion

        #region ThirdPartyNoticesCommand

        internal IAsyncRelayCommand ThirdPartyNoticesCommand { get; }

        private async Task ExecuteThirdPartyNoticesCommandAsync()
        {
            await _windowManager.ShowContentDialogAsync(
                new MarkdownContentDialog(
                    await AssetsHelper.GetThirdPartyNoticesAsync()),
                Strings.Close,
                title: Strings.ThirdPartyNotices);
        }

        #endregion

        #region LicenseCommand

        internal IAsyncRelayCommand LicenseCommand { get; }

        private async Task ExecuteLicenseCommandAsync()
        {
            await _windowManager.ShowContentDialogAsync(
                new MarkdownContentDialog(
                    await AssetsHelper.GetLicenseAsync()),
                Strings.Close,
                title: Strings.License);
        }

        #endregion

        #region RateAndReviewCommand

        internal IAsyncRelayCommand RateAndReviewCommand { get; }

        private async Task ExecuteRateAndReviewCommandAsync()
        {
            var storeContext = StoreContext.GetDefault();

            StoreRateAndReviewResult result = await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                return await storeContext.RequestRateAndReviewAppAsync();
            }).ConfigureAwait(false);
        }

        #endregion

        #region OpenLogsCommand

        internal IAsyncRelayCommand OpenLogsCommand { get; }

        private async Task ExecuteOpenLogsCommandAsync()
        {
            await Logger.OpenLogsAsync();
        }

        #endregion

        private void LoadFonts()
        {
            string[]? systemFonts = CanvasTextFormat.GetSystemFontFamilies();

            for (int i = 0; i < systemFonts.Length; i++)
            {
                SupportedFonts.Add(systemFonts[i]);
            }

            OnPropertyChanged(nameof(TextEditorFont));
        }
    }
}
