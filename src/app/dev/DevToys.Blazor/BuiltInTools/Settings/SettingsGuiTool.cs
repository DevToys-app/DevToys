using System.Reflection;
using DevToys.Blazor.Core.Languages;
using DevToys.Core;
using DevToys.Core.Logging;
using DevToys.Core.Settings;

namespace DevToys.Blazor.BuiltInTools.Settings;

[Export(typeof(IGuiTool))]
[Name(SettingsInternalToolName)]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF6A9',
    ResourceManagerAssemblyIdentifier = nameof(DevToysBlazorResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Blazor.BuiltInTools.Settings.Settings",
    ShortDisplayTitleResourceName = nameof(Settings.ShortDisplayTitle),
    DescriptionResourceName = nameof(Settings.Description),
    AccessibleNameResourceName = nameof(Settings.AccessibleName))]
[MenuPlacement(MenuPlacement.Footer)]
[NotFavorable]
[NotSearchable]
[NoCompactOverlaySupport]
internal sealed class SettingsGuiTool : IGuiTool
{
    internal const string SettingsInternalToolName = "Settings";

    private readonly ISettingsProvider _settingsProvider;
    private readonly IClipboard _clipboard;
    private readonly IFontProvider _fontProvider;
    private readonly IFileStorage _fileStorage;
    private readonly IUIDropDownListItem[] _availableLanguages;
    private readonly IUIDropDownListItem _currentLanguage;
    private readonly IUIDropDownListItem[] _availableFonts;
    private readonly IUISetting _smartDetectionAutomaticallyPasteSetting = Setting("smart-detection-automatically-paste-setting");
    private readonly IUISetting _textEditorFontSetting = Setting("text-editor-font-setting");
    private readonly IUISetting _textEditorEndOfLineSetting = Setting("text-editor-eol-setting");
    private readonly string _previewJsonText;

    [ImportingConstructor]
    public SettingsGuiTool(
        ISettingsProvider settingsProvider,
        IFontProvider fontProvider,
        IClipboard clipboard,
        IFileStorage fileStorage)
    {
        _settingsProvider = settingsProvider;
        _fontProvider = fontProvider;
        _clipboard = clipboard;
        _fileStorage = fileStorage;

        // Load available languages and current language.
        (IUIDropDownListItem[] availableLanguagesItems, IUIDropDownListItem currentLanguageItem) = LoadLanguages();
        _currentLanguage = currentLanguageItem;
        _availableLanguages = availableLanguagesItems;

        // Load available fonts
        _availableFonts = LoadAvailableFonts();

        // Load sample JSON.
        _previewJsonText = LoadPreviewJsonText();

        OnTextEditorEndOfLinePreferenceChanged(_settingsProvider.GetSetting(PredefinedSettings.TextEditorEndOfLinePreference));
    }

    public UIToolView View
        => new(
            Stack()
                .Vertical()
                .LargeSpacing()
                .WithChildren(

                // Appearance settings
                Stack()
                    .Vertical()
                    .WithChildren(

                        Label().Text(Settings.Appearance),
                        Setting("language-setting")
                            .Icon("FluentSystemIcons", '\uF4F2')
                            .Title(Settings.Language)
                            .Description(Settings.LanguageDescription)
                            .InteractiveElement(

                                Stack()
                                    .Horizontal()
                                    .WithChildren(

                                        Button("help-translate-devtoys")
                                            .HyperlinkAppearance()
                                            .Text(Settings.HelpTranslating)
                                            .OnClick(OnHelpTranslatingButtonClick),

                                        SelectDropDownList("language-select-drop-down-list")
                                            .WithItems(_availableLanguages)
                                            .Select(_currentLanguage)
                                            .OnItemSelected(OnLanguageSelected))),

                        Setting("app-theme-setting")
                            .Icon("FluentSystemIcons", '\uF591')
                            .Title(Settings.AppTheme)
                            .Description(Settings.AppThemeDescription)
                            .Handle(
                                _settingsProvider,
                                PredefinedSettings.Theme,
                                onOptionSelected: null,
                                Item(Settings.UseSystemSettings, AvailableApplicationTheme.Default),
                                Item(Settings.Light, AvailableApplicationTheme.Light),
                                Item(Settings.Dark, AvailableApplicationTheme.Dark)),

                        Setting("compact-mode-setting")
                            .Icon("FluentSystemIcons", '\uE0DC')
                            .Title(Settings.CompactMode)
                            .Description(Settings.CompactModeDescription)
                            .Handle(
                                _settingsProvider,
                                PredefinedSettings.CompactMode)),

                // Behavior settings
                Stack()
                    .Vertical()
                    .WithChildren(

                        Label().Text(Settings.Behaviors),
                        Setting("update-setting")
                            .Icon("FluentSystemIcons", '\uF150')
                            .Title(Settings.CheckForUpdate)
                            .Description(Settings.CheckForUpdateDescription)
                            .Handle(
                                _settingsProvider,
                                PredefinedSettings.CheckForUpdate),

                        SettingGroup("smart-detection-settings")
                            .Icon("FluentSystemIcons", '\uF4D5')
                            .Title(Settings.SmartDetection)
                            .Description(Settings.SmartDetectionDescription)
                            .Handle(_settingsProvider, PredefinedSettings.SmartDetection, OnSmartDetectionOptionChanged)
                            .WithSettings(

                                _smartDetectionAutomaticallyPasteSetting
                                    .Title(Settings.SmartDetectionPaste)
                                    .Handle(_settingsProvider, PredefinedSettings.SmartDetectionPaste))),

                // Editing settings
                Stack()
                    .Vertical()
                    .WithChildren(

                        Label().Text(Settings.Editing),
                        SettingGroup()
                            .Icon("FluentSystemIcons", '\uE3BB')
                            .Title(Settings.TextEditor)
                            .WithChildren(

                                _textEditorFontSetting
                                    .Title(Settings.Font)
                                    .StateDescription(_settingsProvider.GetSetting(PredefinedSettings.TextEditorFont))
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorFont,
                                        OnTextEditorFontChanged,
                                        dropDownListItems: _availableFonts),

                                Setting("text-editor-word-wrap-settings")
                                    .Title(Settings.WordWrap)
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorTextWrapping,
                                        stateDescriptionWhenOn: Settings.WordWrapStateDescriptionWhenOn,
                                        stateDescriptionWhenOff: null),

                                Setting("text-editor-line-number-settings")
                                    .Title(Settings.LineNumbers)
                                    .Description(Settings.LineNumbersDescription)
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorLineNumbers,
                                        stateDescriptionWhenOn: Settings.LineNumbersStateDescriptionWhenOn,
                                        stateDescriptionWhenOff: null),

                                Setting("text-editor-line-highlight-settings")
                                    .Title(Settings.HighlightCurrentLine)
                                    .Description(Settings.HighlightCurrentLineDescription)
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorHighlightCurrentLine,
                                        stateDescriptionWhenOn: Settings.HighlightCurrentLineStateDescriptionWhenOn,
                                        stateDescriptionWhenOff: null),

                                Setting("text-editor-white-spaces-settings")
                                    .Title(Settings.RenderWhitespace)
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorRenderWhitespace,
                                        stateDescriptionWhenOn: Settings.RenderWhitespaceStateDescriptionWhenOn,
                                        stateDescriptionWhenOff: null),

                                _textEditorEndOfLineSetting
                                    .Title(Settings.EndOfLinePreference)
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorEndOfLinePreference,
                                        OnTextEditorEndOfLinePreferenceChanged,
                                        Item(Settings.EndOfLinePreferenceAutomatic, UITextEndOfLinePreference.TextDefault),
                                        Item(Settings.EndOfLinePreferenceLF, UITextEndOfLinePreference.LF),
                                        Item(Settings.EndOfLinePreferenceCRLF, UITextEndOfLinePreference.CRLF)),

                                Setting("text-editor-clear-text-on-paste-settings")
                                    .Title(Settings.PasteClearsText)
                                    .Description(Settings.PasteClearsTextDescription)
                                    .Handle(
                                        _settingsProvider,
                                        PredefinedSettings.TextEditorPasteClearsText,
                                        stateDescriptionWhenOn: Settings.PasteClearsTextStateDescriptionWhenOn,
                                        stateDescriptionWhenOff: null),

                                MultiLineTextInput("text-editor-render-preview")
                                    .Title(Settings.TextEditorPreview)
                                    .AlignVertically(UIVerticalAlignment.Top)
                                    .Language("json")
                                    .Text(_previewJsonText))),

                // About
                Stack()
                    .Vertical()
                    .WithChildren(

                        Label().Text(Settings.About),
                        SettingGroup("about-settings")
                            .Icon("FluentSystemIcons", '\uF4A2')
                            .Title("DevToys")
                            .Description(GetAppVersionDescription())
                            .InteractiveElement(
                                Button("copy-about-settings")
                                    .Icon("FluentSystemIcons", '\uF32B')
                                    .OnClick(OnCopyVersionNumberButtonClickAsync))
                            .WithChildren(

                                Stack()
                                    .Vertical()
                                    .WithChildren(
                                        Label()
                                            .Style(UILabelStyle.BodyStrong)
                                            .Text(Settings.SpecialThanks),

                                        Button("logo-designer")
                                            .HyperlinkAppearance()
                                            .AlignHorizontally(UIHorizontalAlignment.Left)
                                            .Text(string.Format(Settings.IconDesigner, "Zee-Al-Eid Ahmad"))
                                            .OnClick(OnLogoDesignerButtonClick),

                                        Button("devtoysmac")
                                            .HyperlinkAppearance()
                                            .AlignHorizontally(UIHorizontalAlignment.Left)
                                            .Text(string.Format(Settings.DevToysMac, "ObuchiYuki"))
                                            .OnClick(OnDevToysMacAuthorButtonClick))),

                        SettingGroup("useful-links-settings")
                            .Icon("FluentSystemIcons", '\uF4E4')
                            .Title(Settings.UsefulLinks)
                            .WithChildren(

                                Wrap()
                                    .WithChildren(
                                        Button("link-source-code")
                                            .HyperlinkAppearance()
                                            .Text(Settings.UsefulLinksSourceCode)
                                            .OnClick(OnSourceCodeButtonClick),

                                        Button("link-privacy-policy")
                                            .HyperlinkAppearance()
                                            .Text(Settings.UsefulLinksPrivacyPolicy)
                                            .OnClick(OnPrivacyPolicyButtonClick),

                                        Button("link-license")
                                            .HyperlinkAppearance()
                                            .Text(Settings.UsefulLinksLicense)
                                            .OnClick(OnLicenseButtonClick),

                                        Button("link-third-party-license")
                                            .HyperlinkAppearance()
                                            .Text(Settings.UsefulLinksThirdPartyLicenses)
                                            .OnClick(OnThirdPartyLicenseButtonClick),

                                        Button("link-report-a-problem")
                                            .HyperlinkAppearance()
                                            .Text(Settings.UsefulLinksReportProblem)
                                            .OnClick(OnReportProblemButtonClick))),

                        Setting("logs-settings")
                            .Icon("FluentSystemIcons", '\uE4F0')
                            .Title(Settings.OpenLogs)
                            .InteractiveElement(
                                Button("open-logs")
                                    .Icon("FluentSystemIcons", '\uF418')
                                    .OnClick(OnOpenLogsButtonClick)))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private void OnHelpTranslatingButtonClick()
    {
        OSHelper.OpenFileInShell("https://crowdin.com/project/devtoys");
    }

    private void OnLanguageSelected(IUIDropDownListItem? selectedItem)
    {
        if (selectedItem is not null && selectedItem.Value is LanguageDefinition languageDefinition)
        {
            _settingsProvider.SetSetting(PredefinedSettings.Language, languageDefinition.InternalName);
        }
    }

    private void OnSmartDetectionOptionChanged(bool enabled)
    {
        if (enabled)
        {
            _smartDetectionAutomaticallyPasteSetting.Enable();
        }
        else
        {
            _smartDetectionAutomaticallyPasteSetting.Disable();
        }
    }

    private async ValueTask OnCopyVersionNumberButtonClickAsync()
    {
        await _clipboard.SetClipboardTextAsync(GetAppVersionDescription());
    }

    private void OnTextEditorFontChanged(string fontName)
    {
        _textEditorFontSetting.StateDescription(fontName);
    }

    private void OnTextEditorEndOfLinePreferenceChanged(UITextEndOfLinePreference endOfLinePreference)
    {
        switch (endOfLinePreference)
        {
            case UITextEndOfLinePreference.TextDefault:
                _textEditorEndOfLineSetting.StateDescription(Settings.EndOfLinePreferenceAutomatic);
                break;
            case UITextEndOfLinePreference.LF:
                _textEditorEndOfLineSetting.StateDescription(Settings.EndOfLinePreferenceLF);
                break;
            case UITextEndOfLinePreference.CRLF:
                _textEditorEndOfLineSetting.StateDescription(Settings.EndOfLinePreferenceCRLF);
                break;
            default:
                ThrowHelper.ThrowNotSupportedException();
                break;
        }
    }

    private IUIDropDownListItem[] LoadAvailableFonts()
    {
        string[] systemFontFamilies = _fontProvider.GetFontFamilies();
        var availableFonts = new IUIDropDownListItem[systemFontFamilies.Length];

        for (int i = 0; i < systemFontFamilies.Length; i++)
        {
            string fontName = systemFontFamilies[i];
            availableFonts[i]
                = Item(
                    fontName,
                    fontName);
        }

        return availableFonts;
    }

    private (IUIDropDownListItem[] availableLanguagesItems, IUIDropDownListItem currentLanguageItem) LoadLanguages()
    {
        string currentLanguage = _settingsProvider.GetSetting(PredefinedSettings.Language);
        var availableLanguagesItems = new IUIDropDownListItem[LanguageManager.Instance.AvailableLanguages.Count];
        IUIDropDownListItem currentLanguageItem = default!;

        for (int i = 0; i < LanguageManager.Instance.AvailableLanguages.Count; i++)
        {
            LanguageDefinition languageDefinition = LanguageManager.Instance.AvailableLanguages[i];
            availableLanguagesItems[i]
                = Item(
                    languageDefinition.DisplayName,
                    languageDefinition);

            if (languageDefinition.InternalName == currentLanguage)
            {
                currentLanguageItem = availableLanguagesItems[i];
            }
        }

        Guard.IsNotNull(currentLanguageItem);
        return (availableLanguagesItems, currentLanguageItem);
    }

    private static string LoadPreviewJsonText()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "DevToys.Blazor.Assets.samples.json-sample.json";

        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string GetAppVersionDescription()
    {
        var assemblyInformationalVersion = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))!;
        return string.Format(Settings.Version, assemblyInformationalVersion.InformationalVersion);
    }

    private void OnLogoDesignerButtonClick()
    {
        OSHelper.OpenFileInShell("https://twitter.com/zeealeid");
    }

    private void OnDevToysMacAuthorButtonClick()
    {
        OSHelper.OpenFileInShell("https://twitter.com/obuchi_yuki");
    }

    private void OnSourceCodeButtonClick()
    {
        OSHelper.OpenFileInShell("https://github.com/DevToys-app/DevToys");
    }

    private void OnPrivacyPolicyButtonClick()
    {
        OSHelper.OpenFileInShell("https://github.com/DevToys-app/DevToys/blob/main/PRIVACY-POLICY.md");
    }

    private void OnLicenseButtonClick()
    {
        OSHelper.OpenFileInShell("https://github.com/DevToys-app/DevToys/blob/main/LICENSE.md");
    }

    private void OnThirdPartyLicenseButtonClick()
    {
        OSHelper.OpenFileInShell("https://github.com/DevToys-app/DevToys/blob/main/THIRD-PARTY-NOTICES.md");
    }

    private void OnReportProblemButtonClick()
    {
        OSHelper.OpenFileInShell("https://github.com/DevToys-app/DevToys/issues");
    }

    private void OnOpenLogsButtonClick()
    {
        string logsFolder = Path.Combine(_fileStorage.AppCacheDirectory, FileLoggerProvider.LogFolderName);
        OSHelper.OpenFileInShell(logsFolder);
    }
}
