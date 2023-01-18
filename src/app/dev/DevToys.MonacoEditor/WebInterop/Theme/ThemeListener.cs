using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Newtonsoft.Json;
using Windows.UI;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using DispatcherQueue = Windows.UI.Core.CoreDispatcher;
#else
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace DevToys.MonacoEditor.WebInterop;

internal delegate void ThemeChangedEvent(ThemeListener sender);

/// <summary>
/// Class which listens for changes to Application Theme or High Contrast Modes 
/// and Signals an Event when they occur.
/// </summary>
[AllowForWeb]
internal sealed partial class ThemeListener // This is a copy of the Toolkit ThemeListener, for some reason if we try and use it directly it's not read by the WebView
{
    private readonly DispatcherQueue _dispatcher;

    public string CurrentThemeName => JsonConvert.SerializeObject(CurrentTheme.ToString()); // For Web Retrieval

    public ApplicationTheme CurrentTheme { get; set; }

    public bool IsHighContrast { get; set; }

    public string AccentColorHtmlHex { get; private set; }

    public event ThemeChangedEvent? ThemeChanged;

    private readonly AccessibilitySettings _accessible = new();
    private readonly UISettings _settings = new();

    internal ThemeListener(DispatcherQueue queue)
    {
        Guard.IsNotNull(queue);
        _dispatcher = queue;

        AccentColorHtmlHex = ToHtmlHex(((SolidColorBrush)Application.Current.Resources["TextControlSelectionHighlightColor"]).Color);
        CurrentTheme = Application.Current.RequestedTheme;
#if !__WASM__
        IsHighContrast = _accessible.HighContrast;
#endif

        // TODO
        //_accessible.HighContrastChanged += _accessible_HighContrastChanged;
        //_settings.ColorValuesChanged += _settings_ColorValuesChanged;

        //// Fallback in case either of the above fail, we'll check when we get activated next.
        //Window.Current.CoreWindow.Activated += CoreWindow_Activated;

        PartialCtor();
        UpdateProperties();
    }

    partial void PartialCtor();

    ~ThemeListener()
    {
        // TODO
        //_accessible.HighContrastChanged -= _accessible_HighContrastChanged;
        //_settings.ColorValuesChanged -= _settings_ColorValuesChanged;

        //Window.Current.CoreWindow.Activated -= CoreWindow_Activated;
    }

    // TODO
    //    private void _accessible_HighContrastChanged(AccessibilitySettings sender, object args)
    //    {
    //#if DEBUG
    //        Debug.WriteLine("HighContrast Changed");
    //#endif

    //        UpdateProperties();
    //    }

    //    // Note: This can get called multiple times during HighContrast switch, do we care?
    //    private void _settings_ColorValuesChanged(UISettings sender, object args)
    //    {
    //        // Getting called off thread, so we need to dispatch to request value.
    //        _dispatcher.RunOnUIThreadAsync(() =>
    //        {
    //            // TODO: This doesn't stop the multiple calls if we're in our faked 'White' HighContrast Mode below.
    //            if (CurrentTheme != Application.Current.RequestedTheme ||
    //                IsHighContrast != _accessible.HighContrast)
    //            {
    //#if DEBUG
    //                Debug.WriteLine("Color Values Changed");
    //#endif

    //                UpdateProperties();
    //            }
    //        }).Forget();
    //    }

    private bool IsSystemHighContrast() =>
        ApiInformation.IsPropertyPresent("Windows.UI.ViewManagement.HighContrast", "HighContrast")
        && _accessible.HighContrast;

    // TODO
    //    private void CoreWindow_Activated(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowActivatedEventArgs args)
    //    {
    //        if (CurrentTheme != Application.Current.RequestedTheme ||
    //            IsHighContrast != IsSystemHighContrast())
    //        {
    //#if DEBUG
    //            Debug.WriteLine("CoreWindow Activated Changed");
    //#endif

    //            UpdateProperties();
    //        }
    //    }

    /// <summary>
    /// Set our current properties and fire a change notification.
    /// </summary>
    private void UpdateProperties()
    {
        // TODO: Not sure if HighContrastScheme names are localized?
        if (IsSystemHighContrast() && _accessible.HighContrastScheme.IndexOf("white", StringComparison.OrdinalIgnoreCase) != -1)
        {
            // If our HighContrastScheme is ON & a lighter one, then we should remain in 'Light' theme mode for Monaco Themes Perspective
            IsHighContrast = false;
            CurrentTheme = ApplicationTheme.Light;
        }
        else
        {
            // Otherwise, we just set to what's in the system as we'd expect.
            IsHighContrast = _accessible.HighContrast;
            CurrentTheme = Application.Current.RequestedTheme;
        }

        AccentColorHtmlHex = ToHtmlHex(((SolidColorBrush)Application.Current.Resources["TextControlSelectionHighlightColor"]).Color);

        ThemeChanged?.Invoke(this);
    }

    public static string ToHtmlHex(Color color)
    {
        return JsonConvert.SerializeObject($"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}");
    }
}
