using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using DevToys.Api;
using DevToys.Windows.Core;
using DevToys.Windows.Core.Helpers;
using DevToys.Windows.Native;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.WindowsAndMessaging;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace DevToys.Windows.Controls;

public abstract partial class MicaWindowWithOverlay : Window
{
    private readonly ResourceDictionary _resourceDictionary;
    protected IThemeListener? _themeListener;
    protected EfficiencyModeService? _efficiencyModeService;
    private Button? _restoreButton;
    private Button? _maximizeButton;
    private Button? _minimizeButton;
    private StackPanel? _windowStateButtonsStackPanel;
    private bool _isMouseButtonDownOnDraggableTitleBarArea;
    private Point _mouseDownPositionOnDraggableTitleBarArea;

    protected MicaWindowWithOverlay()
    {
        _resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("/DevToys;component/Controls/MicaWindowWithOverlay.xaml", UriKind.RelativeOrAbsolute)
        };

        Style = _resourceDictionary["MicaWindowWithOverlayStyle"] as Style;

        ApplyResizeBorderThickness();

        Closed += MicaWindowWithOverlay_Closed;
        Loaded += MicaWindowWithOverlay_Loaded;
    }

    internal static readonly DependencyProperty TitleBarMarginLeftProperty
        = DependencyProperty.Register(
            nameof(TitleBarMarginLeft),
            typeof(GridLength),
            typeof(MicaWindowWithOverlay));

    internal GridLength TitleBarMarginLeft
    {
        get => (GridLength)GetValue(TitleBarMarginLeftProperty);
        set => SetValue(TitleBarMarginLeftProperty, value);
    }

    internal static readonly DependencyProperty TitleBarMarginRightProperty
        = DependencyProperty.Register(
            nameof(TitleBarMarginRight),
            typeof(GridLength),
            typeof(MicaWindowWithOverlay));

    internal GridLength TitleBarMarginRight
    {
        get => (GridLength)GetValue(TitleBarMarginRightProperty);
        set => SetValue(TitleBarMarginRightProperty, value);
    }

    internal static readonly DependencyProperty TitleBarWindowStateButtonsWidthProperty
        = DependencyProperty.Register(
            nameof(TitleBarWindowStateButtonsWidth),
            typeof(int),
            typeof(MicaWindowWithOverlay));

    internal int TitleBarWindowStateButtonsWidth
    {
        get => (int)GetValue(TitleBarWindowStateButtonsWidthProperty);
        set => SetValue(TitleBarWindowStateButtonsWidthProperty, value);
    }

    internal static readonly DependencyProperty MarginMaximizedProperty
        = DependencyProperty.Register(
            nameof(MarginMaximized),
            typeof(Thickness),
            typeof(MicaWindowWithOverlay));

    internal Thickness? MarginMaximized
    {
        get => (Thickness)GetValue(MarginMaximizedProperty);
        set => SetValue(MarginMaximizedProperty, value);
    }

    internal static readonly DependencyProperty ForbidMinimizeAndMaximizeProperty
        = DependencyProperty.Register(
            nameof(ForbidMinimizeAndMaximize),
            typeof(bool),
            typeof(MicaWindowWithOverlay),
            new PropertyMetadata(OnForbidMinimizeAndMaximizePropertyChangedCallback));

    internal bool ForbidMinimizeAndMaximize
    {
        get => (bool)GetValue(ForbidMinimizeAndMaximizeProperty);
        set => SetValue(ForbidMinimizeAndMaximizeProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _windowStateButtonsStackPanel = (StackPanel)Template.FindName("WindowStateButtonsStackPanel", this);
        var closeButton = (Button)Template.FindName("CloseButton", this);
        _minimizeButton = (Button)Template.FindName("MinimizeButton", this);
        _restoreButton = (Button)Template.FindName("RestoreButton", this);
        _maximizeButton = (Button)Template.FindName("MaximizeButton", this);
        var draggableTitleBarArea = (Border)Template.FindName("DraggableTitleBarArea", this);
        var overlayControl = (OverlayControl)Template.FindName("TitleBar", this);

        _windowStateButtonsStackPanel.SizeChanged += WindowStateButtonsStackPanel_SizeChanged;
        closeButton.Click += CloseButton_Click;
        _minimizeButton.Click += MinimizeButton_Click;
        _restoreButton.Click += RestoreButton_Click;
        _maximizeButton.Click += MaximizeButton_Click;
        draggableTitleBarArea.MouseLeftButtonDown += DraggableTitleBarArea_MouseLeftButtonDown;
        draggableTitleBarArea.MouseLeftButtonUp += DraggableTitleBarArea_MouseLeftButtonUp;
        draggableTitleBarArea.MouseRightButtonUp += DraggableTitleBarArea_MouseRightButtonUp;
        draggableTitleBarArea.MouseMove += DraggableTitleBarArea_MouseMove;
        this.PreviewKeyDown += MicaWindowWithOverlay_PreviewKeyDown;
        overlayControl.WndProc = WndProc;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        Guard.IsNotNull(_themeListener);
        _themeListener.ThemeChanged += OnAppThemeChanged;

        ApplyWindowAppearance();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.Property.Name is nameof(WindowState))
        {
            ApplyResizeBorderThickness();
        }

        base.OnPropertyChanged(e);
    }

    private void MicaWindowWithOverlay_Loaded(object sender, RoutedEventArgs e)
    {
        Guard.IsNotNull(_efficiencyModeService);
        _efficiencyModeService.RegisterWindow(this);
    }

    private void MicaWindowWithOverlay_Closed(object? sender, EventArgs e)
    {
        Guard.IsNotNull(_efficiencyModeService);
        _efficiencyModeService.UnregisterWindow(this);

        var overlayControl = (OverlayControl)Template.FindName("TitleBar", this);
        overlayControl.Dispose();
    }

    private void MicaWindowWithOverlay_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Space)
        {
            // Show system context menu on Alt + Space.
            e.Handled = true;
            SystemCommands.ShowSystemMenu(this, this.PointToScreen(new Point(0, 0)));
        }
    }

    private void WindowStateButtonsStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Guard.IsNotNull(_windowStateButtonsStackPanel);
        TitleBarWindowStateButtonsWidth = (int)_windowStateButtonsStackPanel.ActualWidth;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MaximizeWindow(this);
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.RestoreWindow(this);
    }

    private void DraggableTitleBarArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (!ForbidMinimizeAndMaximize)
            {
                // Toggle Maximize / Normal state.
                if (WindowState == WindowState.Maximized)
                {
                    SystemCommands.RestoreWindow(this);
                }
                else
                {
                    SystemCommands.MaximizeWindow(this);
                }

                return;
            }
        }
        else
        {
            if (WindowState == WindowState.Maximized)
            {
                _isMouseButtonDownOnDraggableTitleBarArea = true;
                _mouseDownPositionOnDraggableTitleBarArea = e.GetPosition(this);
            }
            else
            {
                try
                {
                    DragMove();
                }
                catch
                {
                }
            }
        }
    }

    private void DraggableTitleBarArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseButtonDownOnDraggableTitleBarArea = false;
    }

    private void DraggableTitleBarArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isMouseButtonDownOnDraggableTitleBarArea
            && e.LeftButton == MouseButtonState.Pressed
            && WindowState == WindowState.Maximized)
        {
            Point position = e.GetPosition(this);
            Point screenPosition = PointToScreen(position);

            Vector vector = _mouseDownPositionOnDraggableTitleBarArea - position;

            if (vector.Length > 1)
            {
                double relativeDistance = position.X / ActualWidth;

                WindowState = WindowState.Normal;

                double actualDistance = ActualWidth * relativeDistance;

                try
                {
                    Top = screenPosition.Y - position.Y;
                    Left = screenPosition.X - actualDistance;

                    DragMove();
                }
                catch
                {
                }
            }
        }
    }

    private void DraggableTitleBarArea_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Show system context menu on right click.
        SystemCommands.ShowSystemMenu(this, this.PointToScreen(Mouse.GetPosition(this)));
    }

    private void OnAppThemeChanged(object? sender, EventArgs e)
    {
        ApplyWindowAppearance();
    }

    private (nint, bool) WndProc(nint hwnd, int msg, nint wParam, nint lParam)
    {
        bool handled = false;

        // Handle Windows 11's Snap Layout.
        if (msg == PInvoke.WM_NCHITTEST)
        {
            bool isWindow11_OrGreater = Environment.OSVersion.Version >= new Version(10, 0, 22000);
            if (isWindow11_OrGreater & SnapLayoutHelper.IsSnapLayoutEnabled())
            {
                nint result = ShowSnapLayout(lParam, ref handled);
                return (result, handled);
            }
        }
        else if (msg == PInvoke.WM_NCLBUTTONDOWN)
        {
            HandleClickOnMaximizeAndRestoreButton(lParam, ref handled);
        }

        return (nint.Zero, handled);
    }

    private void ApplyWindowAppearance()
    {
        // Apply the dark / light theme.
        UpdateTheme();

        bool isWindow10_17763_OrHigher = Environment.OSVersion.Version >= new Version(10, 0, 17763);

        // If Windows 10 17763 or higher
        if (isWindow10_17763_OrHigher)
        {
            var windowHandle = new HWND(new WindowInteropHelper(this).EnsureHandle());

            // Extends the window frame into the client area.
            ExtendWindowFrameIntoClientArea(windowHandle);

            // Hide all the window buttons (minimize, maximize, close). We will replace them by some custom one that will
            // live in an overlay window staying on top of the current one.
            // The reason why is that we will extend the web view to take the whole window space so the Back Button is at the top
            // left hand corner of the window. Unfortunately, Web View has an issue where we can't click on anything on top of it,
            // included the window buttons.
            // See https://github.com/MicrosoftEdge/WebView2Feedback/issues/286
            NativeMethods.HideAllWindowButton(windowHandle);

            // Apply the backdrop color + mica if possible.
            ApplyBackdrop(windowHandle);
        }
    }

    private void ExtendWindowFrameIntoClientArea(nint windowHandle)
    {
        var mainWindowSrc = HwndSource.FromHwnd(windowHandle);

        Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

        var margins = new MARGINS
        {
            cxLeftWidth = -1,
            cxRightWidth = -1,
            cyTopHeight = -1,
            cyBottomHeight = -1
        };

        NativeMethods.ExtendFrame(new HWND(mainWindowSrc.Handle), margins);
    }

    private void ApplyBackdrop(HWND windowHandle)
    {
        Guard.IsNotNull(_themeListener);
        bool isWindow11_22523_OrGreater = Environment.OSVersion.Version >= new Version(10, 0, 22523);
        bool isWindow11_OrGreater = Environment.OSVersion.Version >= new Version(10, 0, 22000);

        if (_themeListener.ActualAppTheme == ApplicationTheme.Dark)
        {
            int parameter = DwmValues.True;
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref parameter);
        }
        else
        {
            int parameter = DwmValues.False;
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref parameter);
        }

        if (isWindow11_22523_OrGreater)
        {
            int Mica = 2;
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref Mica);
        }
        else if (isWindow11_OrGreater)
        {
            int parameter = DwmValues.True;
            NativeMethods.SetWindowAttribute(windowHandle, (DWMWINDOWATTRIBUTE)DWMWINDOWATTRIBUTE_EXTENDED.DWMWA_MICA_EFFECT, ref parameter);
        }
        else
        {
            uint gradientColor = 0xFF202020;
            if (_themeListener.ActualAppTheme == ApplicationTheme.Light)
            {
                gradientColor = 0xFFF3F3F3;
            }

            // Effect for Windows 10
            var accentPolicy = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_GRADIENT,
                AccentFlags = AccentFlag.ACCENT_ENABLE_BORDER | AccentFlag.ACCENT_ENABLE_GRADIENT_COLOR,
                GradientColor = gradientColor
            };

            int accentStructSize = Marshal.SizeOf(accentPolicy);
            nint accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accentPolicy, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            NativeMethods.SetWindowCompositionAttribute(windowHandle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }

    private void ApplyResizeBorderThickness()
    {
        bool isWindow11_OrGreater = Environment.OSVersion.Version >= new Version(10, 0, 22000);
        int cornerRadius = isWindow11_OrGreater ? 8 : 0;

        MarginMaximized = WindowState == WindowState.Maximized ? new Thickness(6) : new Thickness(0);

        if (WindowState == WindowState.Maximized || ResizeMode == System.Windows.ResizeMode.NoResize)
        {
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(cornerRadius),
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = new Thickness(0)
            });
        }
        else
        {
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(cornerRadius),
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = new Thickness(6)
            });
        }
    }

    private void UpdateTheme()
    {
        Guard.IsNotNull(_themeListener);
        string themeResourceDictionaryPath;
        string themeResourceDictionaryToRemovePath;
        if (_themeListener.ActualAppTheme == ApplicationTheme.Dark)
        {
            themeResourceDictionaryPath = "/DevToys;component/Themes/Dark.xaml";
            themeResourceDictionaryToRemovePath = "/DevToys;component/Themes/Light.xaml";
        }
        else
        {
            themeResourceDictionaryPath = "/DevToys;component/Themes/Light.xaml";
            themeResourceDictionaryToRemovePath = "/DevToys;component/Themes/Dark.xaml";
        }

        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri(themeResourceDictionaryPath, UriKind.RelativeOrAbsolute)
        };
        var resourceDictionaryToRemove = new ResourceDictionary
        {
            Source = new Uri(themeResourceDictionaryToRemovePath, UriKind.RelativeOrAbsolute)
        };

        System.Collections.ObjectModel.Collection<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries;
        dictionaries.Remove(resourceDictionaryToRemove);
        dictionaries.Add(resourceDictionary);
        UpdateLayout();
    }

    private nint ShowSnapLayout(nint lParam, ref bool handled)
    {
        short x = (short)(lParam.ToInt32() & 0xffff);
        int y = lParam.ToInt32() >> 16;
        var point = new Point(x, y);
        var dpiHelper = new DpiHelper(this);
        double DPI_SCALE = dpiHelper.LogicalToDeviceUnitsScalingFactorX;

        Button? button = WindowState == WindowState.Maximized ? _restoreButton : _maximizeButton;
        Guard.IsNotNull(button);
        if (button.IsVisible)
        {
            var buttonSize = new Size(button.ActualWidth * DPI_SCALE, button.ActualHeight * DPI_SCALE);
            Point buttonLocation = button.PointToScreen(new Point());
            var rect = new Rect(buttonLocation, buttonSize);

            handled = rect.Contains(point);
            if (handled)
            {
                var color = (LinearGradientBrush)_resourceDictionary["ControlElevationBorder"];
                button.Background = color;
            }
            else
            {
                button.Background = (SolidColorBrush)Application.Current.Resources["FakeTransparentColor"];
            }

            const int HTMAXBUTTON = 9;
            return new nint(HTMAXBUTTON);
        }

        return nint.Zero;
    }

    private void HandleClickOnMaximizeAndRestoreButton(nint lParam, ref bool handled)
    {
        int x = lParam.ToInt32() & 0xffff;
        int y = lParam.ToInt32() >> 16;

        var dpiHelper = new DpiHelper(this);
        double DPI_SCALE = dpiHelper.LogicalToDeviceUnitsScalingFactorX;

        Button? button = WindowState == WindowState.Maximized ? _restoreButton : _maximizeButton;
        Guard.IsNotNull(button);
        if (!button.IsVisible)
        {
            return;
        }

        var rect = new Rect(button.PointToScreen(
        new Point()),
        new Size(button.ActualWidth * DPI_SCALE, button.ActualHeight * DPI_SCALE));
        if (!rect.Contains(new Point(x, y)))
        {
            return;
        }

        handled = true;
        var invokeProv = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
        invokeProv?.Invoke();
    }

    private static void OnForbidMinimizeAndMaximizePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (MicaWindowWithOverlay)d;
        var windowHandle = new HWND(new WindowInteropHelper(window).EnsureHandle());
        if (window.ForbidMinimizeAndMaximize)
        {
            NativeMethods.DisableMinimizeAndMaximizeCapabilities(windowHandle);
        }
        else
        {
            NativeMethods.EnableMinimizeAndMaximizeCapabilities(windowHandle);
        }
    }
}
