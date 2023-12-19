using CommunityToolkit.Mvvm.ComponentModel;

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
    [ObservableProperty]
    private string? _title;
}
