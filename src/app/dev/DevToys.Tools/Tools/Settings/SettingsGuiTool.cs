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

    private readonly IUIButton _topLeftButton;

    private int _clickCount;

    public SettingsGuiTool()
    {
        _topLeftButton
            = Button(id: "topLeftButtonUniqueId-for-logs")
            .Text("Top Left button")
            .OnClick(OnMyButtonClickAsync);
    }

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
                         OnDummySetting2ChangedAsync,
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
                                 Switch())),
                 Wrap()
                     .WithChildren(
                         _topLeftButton,
                         Button().Text("Top Center button"),
                         Button().Text("Top Right button")),
                 Wrap()
                     .Disable()
                     .WithChildren(
                         Button().Text("Bottom Left button"),
                         Button().Text("Bottom Center button").OnClick(OnBottomCenterButtonClickAsync),
                         Button().Text("Bottom Right button")),
                 NumberInput(),
                 Grid()
                    .Rows(
                        (GridTestRow.FileSelection, Fraction(1)),
                        (GridTestRow.InputTextBox, 200),
                        (GridTestRow.Output, Auto))
                    .Columns(
                        (GridTestColumn.UniqueColumn, Fraction(1)))
                    .Cells(
                        Cell(
                            GridTestRow.FileSelection, GridTestColumn.UniqueColumn,
                            FileSelector()
                                .CanSelectManyFiles()
                                .LimitFileTypesTo("*.bmp", "jpeg", "png", ".jpg")
                                .OnFilesSelected(OnFilesSelected)),
                        Cell(
                            GridTestRow.InputTextBox, GridTestColumn.UniqueColumn,
                            SplitGrid()
                                .Vertical()
                                .WithLeftPaneChild(
                                    MultilineTextInput()
                                        .Title("Monaco editor")
                                        .Extendable()
                                        .CanCopyWhenEditable()
                                        .Text("{\"hello\": \"there\"}")
                                        .Language("json")
                                        .CommandBarExtraContent(Button().Text("None")))
                                .WithRightPaneChild(
                                    MultilineTextInput()
                                        .Title("Monaco editor")
                                        .Extendable()
                                        .CanCopyWhenEditable()
                                        .Text("{\"hello\": \"there\"}")
                                        .Language("json")
                                        .CommandBarExtraContent(Button().Text("None")))),
                        Cell(
                            GridTestRow.Output, GridTestColumn.UniqueColumn,
                            SingleLineTextInput().Title("Read-write text input with copy").ReadOnly())),
                 DiffTextInput()
                    .Title("Difference")
                    .OriginalText("hello")
                    .Extendable()
                    .ReadOnly()
                    .ModifiedText("hello world")),
            isScrollable: true);

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private ValueTask OnDummySettingChangedAsync(bool state)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask OnFilesSelected(PickedFile[] files)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask OnDropDownListSelectionChangeAsync(IUIDropDownListItem? selection)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask OnDummySetting2ChangedAsync(AvailableApplicationTheme theme)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask OnMyButtonClickAsync()
    {
        _clickCount++;
        _topLeftButton.Text($"Clicked {_clickCount} time !");
        return ValueTask.CompletedTask;
    }

    private ValueTask OnBottomCenterButtonClickAsync()
    {
        return ValueTask.CompletedTask;
    }
}
