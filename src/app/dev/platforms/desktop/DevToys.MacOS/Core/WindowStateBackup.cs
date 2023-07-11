namespace DevToys.MacOS.Core;

internal record WindowStateBackup(
    bool IsZoomed,
    double Height,
    double Width,
    double Top,
    double Left,
    double MaxHeight,
    double MaxWidth,
    double MinHeight,
    double MinWidth)
{
}

