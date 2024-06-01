using System.Collections.Immutable;
using DevToys.Blazor.BuiltInTools.Settings;
using DevToys.Core;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace DevToys.Blazor.BuiltInTools.ExtensionsManager;

[Export(typeof(IGuiTool))]
[Name(ExtensionmanagerToolName)]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE9E8',
    ResourceManagerAssemblyIdentifier = nameof(DevToysBlazorResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Blazor.BuiltInTools.ExtensionsManager.ExtensionsManager",
    ShortDisplayTitleResourceName = nameof(ExtensionsManager.ShortDisplayTitle),
    DescriptionResourceName = nameof(ExtensionsManager.Description),
    AccessibleNameResourceName = nameof(ExtensionsManager.AccessibleName))]
[MenuPlacement(MenuPlacement.Footer)]
[NotFavorable]
[NotSearchable]
[NoCompactOverlaySupport]
[Order(Before = SettingsGuiTool.SettingsInternalToolName)]
[TargetPlatform(Platform.Windows)]
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacOS)]
internal sealed class ExtensionsManagerGuiTool : IGuiTool
{
    internal const string ExtensionmanagerToolName = "Extensions Manager";

    private enum GridRows
    {
        TopButtons,
        InfoBars,
        ExtensionList
    }

    private enum GridColumns
    {
        Left,
        Right
    }

    private static readonly string[] sizeUnits = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    private readonly Lazy<UIToolView> _view = new(() => new(isScrollable: false));
    private readonly IUIInfoBar _restartRequiredInfoBar = InfoBar("restart-required-after-changing-extension-info-bar");
    private readonly IUIList _extensionList = List("installed-extensions-list");

    private bool _shownWarningInstallExtension;

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    public UIToolView View
    {
        get
        {
            LoadExtensionListAsync().Forget();
            return _view.Value
                .WithRootElement(
                    Grid()
                        .RowLargeSpacing()

                        .Rows(
                            (GridRows.TopButtons, Auto),
                            (GridRows.InfoBars, Auto),
                            (GridRows.ExtensionList, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Columns(
                            (GridColumns.Left, Auto),
                            (GridColumns.Right, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Cells(
                            Cell(
                                GridRows.TopButtons,
                                GridColumns.Left,

                                Stack()
                                    .Horizontal()

                                    .WithChildren(
                                        Button("install-extension-button")
                                            .Text(ExtensionsManager.InstallExtension)
                                            .OnClick(OnInstallExtensionButtonClickAsync),

                                        Button("find-more-extensions-online-button")
                                            .HyperlinkAppearance()
                                            .Text(ExtensionsManager.FindMoreExtension)
                                            .OnClick(OnFindMoreExtensionsOnlineButtonClick))),

                            Cell(
                                GridRows.TopButtons,
                                GridColumns.Right,

                                Button("learn-develop-extension-button")
                                    .AlignHorizontally(UIHorizontalAlignment.Right)
                                    .HyperlinkAppearance()
                                    .Text(ExtensionsManager.LearnDevelopExtension)
                                    .OnClick(OnLearnDevelopExtensionButtonClick)),

                            Cell(
                                GridRows.InfoBars,
                                GridRows.InfoBars,
                                GridColumns.Left,
                                GridColumns.Right,

                                _restartRequiredInfoBar
                                    .Informational()
                                    .Closable()
                                    .Title(ExtensionsManager.RestartRequiredInfoBarTitle)
                                    .Description(ExtensionsManager.RestartRequiredInfoBarDescription)),

                            Cell(
                                GridRows.ExtensionList,
                                GridRows.ExtensionList,
                                GridColumns.Left,
                                GridColumns.Right,

                                _extensionList
                                    .ForbidSelectItem())));
        }
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    private void OnFindMoreExtensionsOnlineButtonClick()
    {
        OSHelper.OpenFileInShell("https://www.nuget.org/packages?q=Tags%3A%22devtoys-app%22");
    }

    private void OnLearnDevelopExtensionButtonClick()
    {
        OSHelper.OpenFileInShell("http://devtoys.app/doc");
    }

    private async ValueTask OnInstallExtensionButtonClickAsync()
    {
        await ShowWarningUseThirdPartyExtensionMessageBoxAsync();

        SandboxedFileReader[] nugetPackages = await _fileStorage.PickOpenFilesAsync("nupkg");

        for (int i = 0; i < nugetPackages.Length; i++)
        {
            ExtensionInstallationResult result = await ExtensionInstallationManager.InstallExtensionAsync(nugetPackages[i]);

            if (result.AlreadyInstalled)
            {
                // Extension is already installed.
                await ShowExtensionAlreadyInstalledMessageBoxAsync(result.NuspecReader.GetTitle());
                return;
            }

            _extensionList.Items.Add(CreateExtensionListItem(result.NuspecReader, result.ExtensionInstallationPath));

            _restartRequiredInfoBar.Open();
        }
    }

    private void OnUninstallExtensionButtonClick(string extensionInstallationPath)
    {
        ExtensionInstallationManager.ScheduleExtensionToBeUninstalled(extensionInstallationPath);
        _restartRequiredInfoBar.Open();
        _extensionList.Items.RemoveValue(extensionInstallationPath);
    }

    private async Task LoadExtensionListAsync()
    {
        _extensionList.Items.Clear();

        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        for (int i = 0; i < ExtensionInstallationManager.ExtensionInstallationFolders.Length; i++)
        {
            if (Directory.Exists(ExtensionInstallationManager.ExtensionInstallationFolders[i]))
            {
                IEnumerable<string> nuspecFiles
                    = Directory.EnumerateFiles(ExtensionInstallationManager.ExtensionInstallationFolders[i], "*.nuspec", SearchOption.AllDirectories);

                foreach (string nuspecFile in nuspecFiles)
                {
                    var nuspec = new NuspecReader(nuspecFile);
                    _extensionList.Items.Add(CreateExtensionListItem(nuspec, Path.GetDirectoryName(nuspecFile)!));
                }
            }
        }
    }

    private IUIListItem CreateExtensionListItem(NuspecReader nuspec, string extensionInstallationPath)
    {
        ImmutableArray<IUIElement>.Builder actionBuilder = ImmutableArray.CreateBuilder<IUIElement>();

        // Calculate installation size.
        long size = 0;
        var dir = new DirectoryInfo(extensionInstallationPath);
        foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
        {
            size += fi.Length;
        }

        // Get project Url
        string? projectUrl = nuspec.GetProjectUrl();
        if (!string.IsNullOrEmpty(projectUrl))
        {
            actionBuilder.Add(
                Button()
                    .Icon("FluentSystemIcons", '\uE6B1')
                    .OnClick(() => NavigateToUrl(projectUrl)));
        }

        // Get project repository
        RepositoryMetadata repositoryInfo = nuspec.GetRepositoryMetadata();
        if (!string.IsNullOrEmpty(repositoryInfo?.Url))
        {
            actionBuilder.Add(
                Button()
                    .Icon("FluentSystemIcons", '\uF0A6')
                    .OnClick(() => NavigateToUrl(repositoryInfo.Url)));
        }

        // Add delete button.
        IUIButton uninstallButton
            = Button()
                .Icon("FluentSystemIcons", '\uE47B')
                .OnClick(() => OnUninstallExtensionButtonClick(extensionInstallationPath));
        actionBuilder.Add(uninstallButton);

        // Create the item.
        return Item(
            Setting()
                .Icon("FluentSystemIcons", '\uE9E8')
                .Title(string.Format(ExtensionsManager.ExtensionTitleInList, nuspec.GetTitle(), nuspec.GetVersion()))
                .Description(nuspec.GetDescription())
                .InteractiveElement(
                    Stack()
                        .Horizontal()
                        .MediumSpacing()
                        .WithChildren(
                            Label().Text(SizeWithUnit(size)),
                            Stack()
                                .Horizontal()
                                .SmallSpacing()
                                .WithChildren(actionBuilder.ToArray()))),
            extensionInstallationPath);

        static void NavigateToUrl(string url)
        {
            OSHelper.OpenFileInShell(url);
        }
    }

    private async Task ShowWarningUseThirdPartyExtensionMessageBoxAsync()
    {
        if (_shownWarningInstallExtension)
        {
            return;
        }

        _shownWarningInstallExtension = true;

        await _view.Value.OpenDialogAsync(
            Stack()
                .Vertical()

                .WithChildren(
                    Label()
                        .Style(UILabelStyle.Subtitle)
                        .Text(ExtensionsManager.WarningThirdPartyExtensionUsageTitle),
                    Label()
                        .Style(UILabelStyle.Body)
                        .Text(ExtensionsManager.WarningThirdPartyExtensionUsageDescription)),

            footerContent:
                Stack()
                .AlignHorizontally(UIHorizontalAlignment.Right)
                .Horizontal()
                    .WithChildren(
                        Button("extension-third-party-warning-terms-service-button")
                            .HyperlinkAppearance()
                            .Text(ExtensionsManager.WarningThirdPartyExtensionUsageTermsConditions)
                            .OnClick(OnTermsConditionsDialogButtonClick),
                        Button("extension-third-party-warning-understand-button")
                            .AccentAppearance()
                            .Text(ExtensionsManager.WarningThirdPartyExtensionUsageIUnderstand)
                            .OnClick(OnIUnderstandDialogButtonClick)),
            isDismissible: false);

        void OnTermsConditionsDialogButtonClick()
        {
            OSHelper.OpenFileInShell("https://github.com/DevToys-app/DevToys/blob/main/EXTENSIONS-TERM-AND-CONDITIONS.md");
        }

        void OnIUnderstandDialogButtonClick()
        {
            _view.Value.CurrentOpenedDialog?.Close();
        }

        await _view.Value.CurrentOpenedDialog!.DialogCloseAwaiter;
    }

    private async Task ShowExtensionAlreadyInstalledMessageBoxAsync(string extensionTitle)
    {
        await _view.Value.OpenDialogAsync(
            Stack()
                .Vertical()

                .WithChildren(
                    Label()
                        .Style(UILabelStyle.Subtitle)
                        .Text(ExtensionsManager.ExtensionAlreadyInstalledTitle),
                    Label()
                        .Style(UILabelStyle.Body)
                        .Text(string.Format(ExtensionsManager.ExtensionAlreadyInstalledDescription, extensionTitle))),

            Button("extension-already-exist-dialog-ok-button")
                .AlignHorizontally(UIHorizontalAlignment.Right)
                .Text(ExtensionsManager.OKButtonText)
                .OnClick(OnCloseDialogButtonClick),
            isDismissible: true);

        void OnCloseDialogButtonClick()
        {
            _view.Value.CurrentOpenedDialog?.Close();
        }
    }

    private static string SizeWithUnit(long value, int decimalPlaces = 1)
    {
        Guard.IsGreaterThan(decimalPlaces, 0);
        Guard.IsGreaterThanOrEqualTo(value, 0);

        if (value == 0)
        {
            return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
        }

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format(
            "{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            sizeUnits[mag]);
    }
}
