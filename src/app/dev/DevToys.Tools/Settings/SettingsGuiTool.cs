using DevToys.Api;

namespace DevToys.Tools.Settings;

[Export(typeof(IGuiTool))]
[Name("Settings")]
[Author("Etienne Baudoux")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uF6A9",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Settings.Settings",
    ShortDisplayTitleResourceName = nameof(Settings.ShortDisplayTitle),
    DescriptionResourceName = nameof(Settings.Description),
    AccessibleNameResourceName = nameof(Settings.AccessibleName))]
[MenuPlacement(MenuPlacement.Footer)]
[NotFavorable]
[NotSearchable]
[NoCompactOverlaySupport]
internal sealed class SettingsGuiTool : IGuiTool
{
    private readonly IUIButton _topLeftButton;

    private int _clickCount;

    public SettingsGuiTool()
    {
        _topLeftButton
            = Button(id: "topLeftButtonUniqueId-for-logs")
            .Text("Top Left button")
            .OnClick(OnMyButtonClickAsync);
    }

    public IUIElement View
        => Stack()
        .Vertical()
        .WithChildren(
            SettingGroup()
                .Icon("FluentSystemIcons", "\uF6A9")
                .Title("Title")
                .Description("Description")
                .InteractiveElement(
                    Button().Text("My option"))
                .WithSettings(
                    Setting()
                        .Title("Title")
                        .Description("Description")
                        .InteractiveElement(
                            Button().Text("My option")),
                    Setting()
                        .Title("Title")
                        .Description("Description")
                        .InteractiveElement(
                            Button().Text("My option")),
                    Setting()
                        .Title("Title")
                        .Description("Description")
                        .InteractiveElement(
                            Button().Text("My option")),
                    Setting()
                        .Title("Title")
                        .Description("Description")
                        .InteractiveElement(
                            Button().Text("My option"))),
            Stack()
                .Horizontal()
                .WithChildren(
                    _topLeftButton,
                    Button().Text("Top Center button"),
                    Button().Text("Top Right button")),
            Stack()
                .Horizontal()
                .Disable()
                .WithChildren(
                    Button().Text("Bottom Left button"),
                    Button().Text("Bottom Center button").OnClick(OnBottomCenterButtonClickAsync),
                    Button().Text("Bottom Right button")));

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
