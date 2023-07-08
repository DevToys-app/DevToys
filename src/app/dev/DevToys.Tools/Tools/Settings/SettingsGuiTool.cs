namespace DevToys.Tools.Tools.Settings;

internal enum GridTestRow
{
    FileSelection,
    InputTextBox,
    Output
}

internal enum GridTestColumn
{
    UniqueColumn
}

[Export(typeof(IGuiTool))]
[Name("Settings")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF6A9',
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Settings.Settings",
    ShortDisplayTitleResourceName = nameof(Settings.ShortDisplayTitle),
    DescriptionResourceName = nameof(Settings.Description),
    AccessibleNameResourceName = nameof(Settings.AccessibleName))]
[MenuPlacement(MenuPlacement.Footer)]
[NotFavorable]
[NotSearchable]
[NoCompactOverlaySupport]
internal sealed class SettingsGuiTool : IGuiTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private ISettingsProvider _settingsProvider = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    /// <summary>
    /// Dummy setting
    /// </summary>
    public static readonly SettingDefinition<bool> DummySetting
        = new(name: nameof(DummySetting), defaultValue: true);

    /// <summary>
    /// Dummy setting
    /// </summary>
    public static readonly SettingDefinition<AvailableApplicationTheme> DummySetting2
        = new(name: nameof(DummySetting2), defaultValue: AvailableApplicationTheme.Default);

    public UIToolView View
        => new(
            Stack()
             .Vertical()
             .WithChildren(
                 SettingGroup()
                     .Icon("FluentSystemIcons", '\uF6A9')
                     .Title("Title")
                     .Description("Description")
                     .Handle(
                         _settingsProvider,
                         DummySetting2,
                         onOptionSelected: null,
                         Item("Use system settings", AvailableApplicationTheme.Default),
                         Item("Light", AvailableApplicationTheme.Light),
                         Item("Dark", AvailableApplicationTheme.Dark))
                     .WithSettings(
                         Setting()
                             .Title("Compact mode")
                             .Handle(_settingsProvider, PredefinedSettings.CompactMode),
                         Setting()
                             .Title("Line numbers")
                             .Handle(_settingsProvider, PredefinedSettings.TextEditorLineNumbers),
                         Setting()
                             .Title("Title")
                             .Description("Description")
                             .InteractiveElement(
                                 Switch()),
                         Setting()
                             .Title("Title")
                             .Description("Description")
                             .InteractiveElement(
                                 Switch()))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }
}
