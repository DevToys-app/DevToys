﻿#if DEBUG
using System.Text.Json;

namespace DevToys.Tools.Tools.Test;

[Export(typeof(IGuiTool))]                                                                      // Register this class as a tool with a GUI.
[Name("Test")]                                                                // Internal tool name.
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",                                                         // Font to use for the icon of the tool.
    IconGlyph = '\uE670',                                                                       // Icon glyph
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),  // Reference to the IResourceManagerAssemblyIdentifier containing the resource manager
    ResourceManagerBaseName = "DevToys.Tools.Tools.Test.Test",                                  // Full name to the .RESX.
    ShortDisplayTitleResourceName = nameof(Test.ShortDisplayTitle),                             // Name of the resource in the .RESX for the title of the tool.
    LongDisplayTitleResourceName = nameof(Test.ShortDisplayTitle),
    DescriptionResourceName = nameof(Test.Description),
    AccessibleNameResourceName = nameof(Test.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]                                      // Register this tool as supporting JSON from Smart Detection.
[MenuPlacement(MenuPlacement.Footer)]                                                           // Place the tool in the footer of the menu
internal sealed class TestGuiTool : IGuiTool
{
    private enum GridTestRow
    {
        Header,
        Content,
        Footer
    }

    private enum GridTestColumn
    {
        Left,
        Right
    }

    private static readonly SettingDefinition<bool> DummyBooleanSetting
        = new(name: nameof(DummyBooleanSetting), defaultValue: true);

    public static readonly SettingDefinition<AvailableApplicationTheme> DummyListSetting
        = new(name: nameof(DummyListSetting), defaultValue: AvailableApplicationTheme.Default);

    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputJsonEditor;

    [ImportingConstructor]
    public TestGuiTool(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        _inputJsonEditor = MultilineTextInput();
        _inputJsonEditor.TextChanged += InputJsonEditor_TextChanged;
    }

    public UIToolView View
        => new(
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()

                // Grid has 3 rows. One fit its content. One takes all the space available. One is 150px of height.
                .Rows(
                    (GridTestRow.Header, Auto),
                    (GridTestRow.Content, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridTestRow.Footer, 150))

                // Grid has 2 columns. One takes 2/3 of the space. One takes 1/3 of the space.
                .Columns(
                    (GridTestColumn.Left, new UIGridLength(2, UIGridUnitType.Fraction)),
                    (GridTestColumn.Right, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    // Cell that is at the top left in the grid.
                    Cell(
                        GridTestRow.Header,
                        GridTestColumn.Left,

                        Stack()
                            .Vertical()

                            .WithChildren(
                                // Allow the user to import PNG, JPG, JPEG or BMP files.
                                FileSelector()
                                    .CanSelectManyFiles()
                                    .LimitFileTypesTo("png", ".jpg", "*.jpeg", "BMP")
                                    .OnFilesSelected(OnFilesSelectedAsync),

                                Wrap() // A horizontal Stack that wraps when needed.
                                    .WithChildren(
                                        Button().Text("Disable Input").OnClick(OnButtonClickAsync),
                                        Button().Text("Top Center button"),
                                        Button().Text("Top Right button")),

                                Wrap() // A horizontal Stack that wraps when needed.
                                    .Disable()
                                    .WithChildren(
                                        Button().Text("Bottom Left button"),
                                        Button().Text("Bottom Center button"),
                                        Button().Text("Bottom Right button")))),

                    // Cell that is at the top right in the grid.
                    Cell(
                        GridTestRow.Header,
                        GridTestColumn.Right,

                        DiffTextInput().Title("Just a diff editor")),

                    // Cell that is at the Content Left in the grid.
                    Cell(
                        GridTestRow.Content,
                        GridTestColumn.Left,

                        Stack()
                            .Vertical()

                            .WithChildren(
                                Label()
                                    .Style(UILabelStyle.Display)
                                    .Text("A large text"),

                                SettingGroup()
                                    .Icon("FluentSystemIcons", '\uF6A9')
                                    .Title("Dummy list setting")
                                    .Description("Description")

                                    // Enum and struct settings can be handled automatically. Just need to set a list of items and associate them to a value.
                                    .Handle(
                                        _settingsProvider,
                                        DummyListSetting,
                                        OnDummyListSettingChangedAsync,
                                        Item("Use system settings", AvailableApplicationTheme.Default),
                                        Item("Light", AvailableApplicationTheme.Light),
                                        Item("Dark", AvailableApplicationTheme.Dark))

                                    .WithSettings(
                                        Setting()
                                            .Title("Dummy setting")
                                            // Boolean settings can also be handled automatically.
                                            .Handle(_settingsProvider, DummyBooleanSetting),

                                        Setting()
                                            .Title("Title")
                                            .Description("Description")
                                            // We can also set any kind of content in the setting card.
                                            .InteractiveElement(
                                                Button().Text("Button setting"))),

                                InfoBar()
                                    .Title("Title")
                                    .Description("Description")
                                    .Informational()
                                    .WithActionButton("Click me", isAccent: false, actionOnClick: null)
                                    .Open())),

                    // Cell that is at the Content Right in the grid.
                    Cell(
                        GridTestRow.Content,
                        GridTestColumn.Right,

                        Stack()
                            .Vertical()
                            .MediumSpacing()

                            .WithChildren(
                                SingleLineTextInput()
                                    .Title("Single-line input text"),

                                NumberInput()
                                    .Title("Number input")
                                    .Maximum(100)
                                    .Minimum(0)
                                    .Step(0.5)
                                    .Value(1.5),

                                SelectDropDownList()
                                    .AlignHorizontally(UIHorizontalAlignment.Left) // Align on the left.
                                    .Title("Left-aligned drop down list")
                                    .WithItems(
                                        Item("Item 1", value: null),
                                        Item("Item 2", value: null),
                                        Item("Item 3", value: null)))),

                    // Cell that is in the Footer row and take the whole width of the grid (it takes the 2 columns).
                    Cell(
                        GridTestRow.Footer, GridTestRow.Footer,    // This cell is in the last row
                        GridTestColumn.Left, GridTestColumn.Right, // and spans from first to last column

                        SplitGrid()
                            .Vertical()

                            .WithLeftPaneChild(
                                _inputJsonEditor
                                    .Language("json")
                                    .Title("Input"))

                            .WithRightPaneChild(
                                DataGrid()
                                    .WithColumns("Column 1", "Column 2", "Column 3")
                                    .WithRows(
                                        Row(null, "Cell 1", "This is the Cell number 2", "Cell3"),
                                        Row(null, Cell("Cell 1"), Cell("Cell 2"), Cell(Button().Text("Cell 3"))),
                                        Row(null, Label().Text("Duis accusam sed eos veniam molestie invidunt vel invidunt magna kasd diam invidunt erat option diam. Ullamcorper euismod et justo ipsum lorem dolores nulla. Hendrerit odio ea dolor eos amet et ea vero. Ut consetetur et elitr nonumy."), "Cell 1", "This is the Cell number 2", "Cell3"),
                                        Row(null, "Cell 1", "This is the Cell number 2", "Cell3"),
                                        Row(null, "Cell 1", "This is the Cell number 2", "Cell3"),
                                        Row(null, "Cell 1", "This is the Cell number 2", "Cell3"),
                                        Row(null, "Cell 1", "This is the Cell number 2", "Cell3"),
                                        Row(null, "Cell 1", "This is the Cell number 2", "Cell3"))
                                ))));

    // Smart detection handler.
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Json && parsedData is Tuple<JsonDocument, string> strongTypedParsedData)
        {
            // Pass the JSON from Smart Detection into the Input text editor.
            _inputJsonEditor.Text(strongTypedParsedData.Item2);
        }
    }

    // Invoked when the user selects some files in the FileSelector.
    private ValueTask OnFilesSelectedAsync(SandboxedFileReader[] files)
    {
        for (int i = 0; i < files.Length; i++)
        {
            files[0].Dispose();
        }
        return ValueTask.CompletedTask;
    }

    // Invoked on click on "Disable Input" button.
    private ValueTask OnButtonClickAsync()
    {
        _inputJsonEditor.Disable();
        return ValueTask.CompletedTask;
    }

    // Invoked when user change the setting "DummyListSetting".
    private ValueTask OnDummyListSettingChangedAsync(AvailableApplicationTheme theme)
    {
        return ValueTask.CompletedTask;
    }

    // Invoked when Input text editor's text changed.
    private void InputJsonEditor_TextChanged(object? sender, EventArgs e)
    {
        string editorText = _inputJsonEditor.Text;
    }
}
#endif
