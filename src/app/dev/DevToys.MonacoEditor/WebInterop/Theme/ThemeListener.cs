using DevToys.UI.Framework.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using Windows.Foundation.Metadata;
using Windows.UI;

namespace DevToys.MonacoEditor.WebInterop;

internal delegate void ThemeChangedEvent(ThemeListener sender);

/// <summary>
/// Class which listens for changes to Application Theme or High Contrast Modes 
/// and Signals an Event when they occur.
/// </summary>
[AllowForWeb]
internal sealed partial class ThemeListener
{
    internal ThemeListener()
    {
        Parts.ThemeListener.ThemeChanged += ThemeListener_ThemeChanged;

        AccentColorHtmlHex = ToHtmlHex(((SolidColorBrush)Application.Current.Resources["TextControlSelectionHighlightColor"]).Color);

        PartialCtor();
    }

    public string CurrentThemeName => JsonConvert.SerializeObject(Parts.ThemeListener.ActualAppTheme.ToString()); // For Web Retrieval

    public Api.Core.Theme.ApplicationTheme CurrentTheme => Parts.ThemeListener.ActualAppTheme;

    public bool IsHighContrast => Parts.ThemeListener.IsHighContrast;

    public string AccentColorHtmlHex { get; private set; }

    public event ThemeChangedEvent? ThemeChanged;

    partial void PartialCtor();

    private void ThemeListener_ThemeChanged(object? sender, EventArgs e)
    {
        AccentColorHtmlHex = ToHtmlHex(((SolidColorBrush)Application.Current.Resources["TextControlSelectionHighlightColor"]).Color);
        ThemeChanged?.Invoke(this);
    }

    private static string ToHtmlHex(Color color)
    {
        return JsonConvert.SerializeObject($"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}");
    }
}
