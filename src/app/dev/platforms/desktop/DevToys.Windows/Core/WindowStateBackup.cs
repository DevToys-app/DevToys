using System.Windows;

namespace DevToys.Windows.Core;

internal record WindowStateBackup(
    bool Topmost,
    double Height,
    double Width,
    double Top,
    double Left,
    double MaxHeight,
    double MaxWidth,
    double MinHeight,
    double MinWidth,
    WindowState State)
{
}
