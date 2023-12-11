namespace DevToys.MacOS.Core;

internal record WindowStateBackup(
    bool IsZoomed,
    nfloat Height,
    nfloat Width,
    nfloat X,
    nfloat Y,
    double MaxHeight,
    double MaxWidth,
    double MinHeight,
    double MinWidth)
{
}
