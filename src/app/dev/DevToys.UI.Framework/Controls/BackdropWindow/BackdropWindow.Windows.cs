#if __WINDOWS__
using System.Runtime.InteropServices;
using DevToys.UI.Framework.Threading;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.Shell.Common;
using WinRT;
using WinRT.Interop;
using static Windows.Win32.PInvoke;

namespace DevToys.UI.Framework.Controls;

public sealed partial class BackdropWindow : Window
{
    private readonly WindowsSystemDispatcherQueueHelper _wsdqHelper;
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _configurationSource;

    public BackdropWindow()
    {
        _wsdqHelper = new WindowsSystemDispatcherQueueHelper();
        _wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

        AppWindow = GetAppWindow();
    }

    internal AppWindow AppWindow { get; }

    public partial void Resize(int width, int height)
    {
        PointInt32 currentWindowPosition = AppWindow.Position;

        // In WinUI 3, we need to scale the window size to the monitor.
        // No need to do it for the monitor size and location like in WPF though.
        HMONITOR monitorHandle = MonitorFromPoint(new System.Drawing.Point(currentWindowPosition.X, currentWindowPosition.Y), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        GetScaleFactorForMonitor(monitorHandle, out DEVICE_SCALE_FACTOR scale);
        double scaleFactor = (int)scale / 100.0;
        AppWindow.Resize(new SizeInt32((int)(width * scaleFactor), (int)(height * scaleFactor)));
    }

    public partial void Move(int x, int y)
    {
        PointInt32 position = new(x, y);
        AppWindow.Move(position);
    }

    public partial void Show()
    {
        DispatcherQueue.ThrowIfNotOnUIThread();

        if (Content is FrameworkElement frameworkElement)
        {
            frameworkElement.Loaded += Window_Loaded;
        }

        SetStartupWindowLocation();
        SetBackdrop();
        ExtendTitleBar();

        AppWindow.Closing += Window_Closing;
        base.Activate();

        Shown?.Invoke(this, EventArgs.Empty);
    }

    private AppWindow GetAppWindow()
    {
        IntPtr windowHandle = GetWindowHandle();
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        return AppWindow.GetFromWindowId(windowId);
    }

    private IntPtr GetWindowHandle()
    {
        return WindowNative.GetWindowHandle(this);
    }

    private void SetStartupWindowLocation()
    {
        switch (WindowStartupLocation)
        {
            case WindowStartupLocation.CenterOwner:
                // TODO
                throw new NotImplementedException();

            case WindowStartupLocation.CenterScreen:
                var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
                if (displayArea is not null)
                {
                    PointInt32 centeredPosition = AppWindow.Position;

                    centeredPosition.X = (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;
                    centeredPosition.Y = (displayArea.WorkArea.Height - AppWindow.Size.Height) / 2;
                    Move(centeredPosition.X, centeredPosition.Y);
                }
                break;

            case WindowStartupLocation.Manual:
                // Let the OS decide for us.
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private void ExtendTitleBar()
    {
        // Check to see if customization is supported.
        // Currently only supported on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;

            // Set active window colors
            titleBar.ButtonBackgroundColor = Colors.Transparent;

            // Set inactive window colors
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        else
        {
            // Title bar customization using these APIs is currently
            // supported only on Windows 11. In other cases, hide
            // the custom title bar element.

            // Show alternative UI for any functionality in
            // the title bar, such as search.
        }
    }

    private void DraggableArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SetDragRegionForCustomTitleBar();
    }

    private void SetBackdrop()
    {
        if (_micaController != null)
        {
            _micaController.Dispose();
            _micaController = null;
        }

        Activated -= Window_Activated;

        ((FrameworkElement)Content).ActualThemeChanged -= Window_ThemeChanged;
        _configurationSource = null;

        TrySetMicaBackdrop();
    }

    private bool TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            // Hooking up the policy object
            _configurationSource = new SystemBackdropConfiguration();
            Activated += Window_Activated;
            ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

            _micaController = new MicaController
            {
                Kind = MicaKind.Base
            };

            // Initial configuration state.
            _configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            _micaController.SetSystemBackdropConfiguration(_configurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }

    private void SetConfigurationSourceTheme()
    {
        if (_configurationSource is not null)
        {
            switch (((FrameworkElement)Content).ActualTheme)
            {
                case ElementTheme.Dark:
                    _configurationSource.Theme = SystemBackdropTheme.Dark;
                    break;

                case ElementTheme.Light:
                    _configurationSource.Theme = SystemBackdropTheme.Light;
                    break;

                case ElementTheme.Default:
                    _configurationSource.Theme = SystemBackdropTheme.Default;
                    break;
            }
        }
    }

    private void SetDragRegionForCustomTitleBar()
    {
        // Check to see if customization is supported.
        // Currently only supported on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported()
            && AppWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            var defaultPoint = new Point(0, 0);
            double scaleAdjustment = GetScaleAdjustment();

            var dragRectsList = new List<RectInt32>();

            for (int i = 0; i < _draggableAreas.Count; i++)
            {
                FrameworkElement draggableArea = _draggableAreas[i];
                Point draggableAreaLocationInWindow
                     = draggableArea
                         .TransformToVisual(this.Content)
                         .TransformPoint(defaultPoint);

                RectInt32 dragRectangle;
                dragRectangle.X = (int)(draggableAreaLocationInWindow.X * scaleAdjustment);
                dragRectangle.Y = (int)(draggableAreaLocationInWindow.Y * scaleAdjustment);
                dragRectangle.Height = (int)(draggableArea.ActualHeight * scaleAdjustment);
                dragRectangle.Width = (int)(draggableArea.ActualWidth * scaleAdjustment);
                dragRectsList.Add(dragRectangle);
            }

            AppWindow.TitleBar.SetDragRectangles(dragRectsList.ToArray());
        }
    }

    private double GetScaleAdjustment()
    {
        IntPtr hWnd = GetWindowHandle();
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
        IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

        // Get DPI.
        int result = GetDpiForMonitor(new HMONITOR(hMonitor), MONITOR_DPI_TYPE.MDT_DEFAULT, out uint dpiX, out uint _);
        if (result != 0)
        {
            throw new Exception("Could not get DPI for monitor.");
        }

        uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        return scaleFactorPercent / 100.0;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < _draggableAreas.Count; i++)
        {
            _draggableAreas[i].SizeChanged += DraggableArea_SizeChanged;
        }

        SetDragRegionForCustomTitleBar();
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_configurationSource is not null)
        {
            _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void Window_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        Closing?.Invoke(this, EventArgs.Empty);

        // Make sure any Mica controller is disposed so it doesn't try to
        // use this hidden window.
        if (_micaController != null)
        {
            _micaController.Dispose();
            _micaController = null;
        }

        AppWindow.Closing -= Window_Closing;
        Activated -= Window_Activated;

        _configurationSource = null;

        if (Content is FrameworkElement frameworkElement)
        {
            frameworkElement.Loaded -= Window_Loaded;
        }
    }

    private class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DispatcherQueueOptions
        {
            internal int _dwSize;
            internal int _threadType;
            internal int _apartmentType;
        }

        [DllImport("CoreMessaging.dll", ExactSpelling = true)]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        private object? _dispatcherQueueController = null;

        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options._dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options._threadType = 2;    // DQTYPE_THREAD_CURRENT
                options._apartmentType = 2; // DQTAT_COM_STA

                _ = CreateDispatcherQueueController(options, ref _dispatcherQueueController!);
            }
        }
    }
}
#endif
