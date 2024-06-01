using DevToys.Blazor.BuiltInTools.ExtensionsManager;

namespace DevToys.Blazor.BuiltInTools.SupportDevelopment;

//[Export(typeof(IGuiTool))]
[Name("SupportDevelopment")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE6FF',
    ResourceManagerAssemblyIdentifier = nameof(DevToysBlazorResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Blazor.BuiltInTools.SupportDevelopment.SupportDevelopment",
    ShortDisplayTitleResourceName = nameof(SupportDevelopment.ShortDisplayTitle),
    DescriptionResourceName = nameof(SupportDevelopment.Description),
    AccessibleNameResourceName = nameof(SupportDevelopment.AccessibleName))]
[MenuPlacement(MenuPlacement.Footer)]
[NotFavorable]
[NotSearchable]
[NoCompactOverlaySupport]
[Order(Before = ExtensionsManagerGuiTool.ExtensionmanagerToolName)]
internal sealed class SupportDevelopmentGuidTools : IGuiTool
{
    // TODO: Finish this tool.
    public UIToolView View
        => new(
            Stack()
                .Vertical()
                .MediumSpacing()
                .WithChildren(

                    Label()
                        .Style(UILabelStyle.Caption)
                        .Text("This app is free, but its development has a cost (time, hardware and services). Here are many ways to support our work:"),

                    Card(
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(

                                Label()
                                    .Style(UILabelStyle.BodyStrong)
                                    .Text("Buy extra features we made with ❤️"),

                                Wrap()
                                    .WithChildren(

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("My Extension 1"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("My Extension 2")))),

                    Card(
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(

                                Label()
                                    .Style(UILabelStyle.BodyStrong)
                                    .Text("Buy some swag 👕"),

                                Wrap()
                                    .WithChildren(

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Check out our swag store")))),

                    Card(
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(

                                Label()
                                    .Style(UILabelStyle.BodyStrong)
                                    .Text("Donate"),

                                Wrap()
                                    .WithChildren(

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Sponsor our GitHub repository"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Donate with PayPal"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Donate with Buy Me A Coffee")))),

                    Card(
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(

                                Label()
                                    .Style(UILabelStyle.BodyStrong)
                                    .Text("Talk about this app on social medias"),

                                Wrap()
                                    .WithChildren(

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Share on X"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Share on Threads"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Share on LinkedIn"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Share on Reddit"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Share on Product Hunt"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Share on Hacker News")))),

                    Card(
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(

                                Label()
                                    .Style(UILabelStyle.BodyStrong)
                                    .Text("Contribute to DevToys"),

                                Wrap()
                                    .WithChildren(

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Translate the app"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Open an issue")))),

                    Card(
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(

                                Label()
                                    .Style(UILabelStyle.BodyStrong)
                                    .Text("Rate the app"),

                                Wrap()
                                    .WithChildren(

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Open Microsoft Store"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Open Apple AppStore"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Open Software Center"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Open WinGet"),

                                        Button()
                                            .HyperlinkAppearance()
                                            .Text("Open Chocolatey"))))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }
}
