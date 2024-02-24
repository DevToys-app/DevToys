using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Localization.Strings.MainWindow;

namespace DevToys.Core;

[Export]
public sealed partial class TitleBarInfoProvider : ObservableObject
{
    /// <summary>
    /// Gets or sets margin of the title bar on the left.
    /// </summary>
    [ObservableProperty]
    private int _titleBarMarginLeft = 0;

    /// <summary>
    /// Gets or sets margin of the title bar on the right.
    /// </summary>
    [ObservableProperty]
    private int _titleBarMarginRight = 0;

    /// <summary>
    /// Gets or sets the width of the window state buttons.
    /// </summary>
    [ObservableProperty]
    private int _titleBarWindowStateButtonsWidth;

    /// <summary>
    /// Gets or sets the title to display in the window title bar.
    /// </summary>
    /// <remarks>
    /// When the app is in Compact Overlay mode, the title includes the tool name.
    /// </remarks>
    [ObservableProperty]
    private string? _title = MainWindow.WindowTitle;

    /// <summary>
    /// Gets or sets the title to display in the window title bar, which always includes the tool name.
    /// </summary>
    [ObservableProperty]
    private string? _titleWithToolName = MainWindow.WindowTitle;
}
