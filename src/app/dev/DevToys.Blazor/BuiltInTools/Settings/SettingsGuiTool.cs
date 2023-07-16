using System.Reflection;
using DevToys.Blazor.Core.Languages;
using DevToys.Core.Settings;

namespace DevToys.Blazor.BuiltInTools.Settings;

[Export(typeof(IGuiTool))]
[Name("Settings")]
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
    private readonly ISettingsProvider _settingsProvider;
    private readonly IFontProvider _fontProvider;
    private readonly IUIDropDownListItem[] _availableLanguages;
    private readonly IUIDropDownListItem _currentLanguage;
    private readonly IUIDropDownListItem[] _availableFonts;
    private readonly IUISetting _smartDetectionAutomaticallyPasteSetting = Setting("smart-detection-automatically-paste-setting");
    private readonly string _previewJsonText;

    [ImportingConstructor]
    public SettingsGuiTool(ISettingsProvider settingsProvider, IFontProvider fontProvider)
    {
        _settingsProvider = settingsProvider;
        _fontProvider = fontProvider;

        // Load available languages and current language.
        (IUIDropDownListItem[] availableLanguagesItems, IUIDropDownListItem currentLanguageItem) = LoadLanguages();
        _currentLanguage = currentLanguageItem;
        _availableLanguages = availableLanguagesItems;

        // Load available fonts
        _availableFonts = LoadAvailableFonts();

        // Load sample JSON.
        _previewJsonText = LoadPreviewJsonText();
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
                                            .OnClick(OnHelpTranslatingButtonClickAsync),

                                        SelectDropDownList("language-select-drop-down-list")
                                            .WithItems(_availableLanguages)
                                            .Select(_currentLanguage)
                                            .OnItemSelected(OnLanguageSelectedAsync))),

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
                        SettingGroup("smart-detection-settings")
                            .Icon("FluentSystemIcons", '\uF4D5')
                            .Title(Settings.SmartDetection)
                            .Description(Settings.SmartDetectionDescription)
                            .Handle(_settingsProvider, PredefinedSettings.SmartDetection, OnSmartDetectionOptionChangedAsync)
                            .WithSettings(

                                _smartDetectionAutomaticallyPasteSetting
                                    .Title(Settings.SmartDetectionPaste)
                                    .Handle(_settingsProvider, PredefinedSettings.SmartDetectionPaste))),

                // Text Editor settings
                Stack()
                    .Vertical()
                    .WithChildren(

                        Label().Text(Settings.TextEditor),
                        Setting("text-editor-font-settings")
                            .Icon("FluentSystemIcons", '\uF7E3')
                            .Title(Settings.Font)
                            .Handle(
                                _settingsProvider,
                                PredefinedSettings.TextEditorFont,
                                onOptionSelected: null,
                                dropDownListItems: _availableFonts),

                        MultilineTextInput("text-editor-render-preview")
                            .Title(Settings.TextEditorPreview)
                            .AlignVertically(UIVerticalAlignment.Top)
                            .ReadOnly()
                            .Language("json")
                            .Text(_previewJsonText))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private ValueTask OnHelpTranslatingButtonClickAsync()
    {
        Shell.OpenUrlInWebBrowser("https://crowdin.com/project/devtoys");
        return ValueTask.CompletedTask;
    }

    private ValueTask OnLanguageSelectedAsync(IUIDropDownListItem? selectedItem)
    {
        if (selectedItem is not null && selectedItem.Value is LanguageDefinition languageDefinition)
        {
            _settingsProvider.SetSetting(PredefinedSettings.Language, languageDefinition.InternalName);
        }
        return ValueTask.CompletedTask;
    }

    private ValueTask OnSmartDetectionOptionChangedAsync(bool enabled)
    {
        if (enabled)
        {
            _smartDetectionAutomaticallyPasteSetting.Enable();
        }
        else
        {
            _smartDetectionAutomaticallyPasteSetting.Disable();
        }

        return ValueTask.CompletedTask;
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
}
