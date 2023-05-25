using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToys.Core;

[Export]
public sealed partial class TitleBarMarginProvider : ObservableObject
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
}
