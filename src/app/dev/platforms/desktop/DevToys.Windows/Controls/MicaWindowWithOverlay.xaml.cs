﻿using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using DevToys.Api;
using DevToys.Windows.Native;

namespace DevToys.Windows.Controls;

public abstract partial class MicaWindowWithOverlay : Window
{
    private readonly Lazy<Version> _windowsVersion
        = new(() =>
        {
            if (NativeMethods.RtlGetVersion(out OSVERSIONINFOEX osv) != 0)
            {
                throw new PlatformNotSupportedException("This app can only run on Windows.");
            }

            return new Version(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.Revision);
        });

    private readonly ResourceDictionary _resourceDictionary;
    protected IThemeListener? _themeListener;
    private Button? _restoreButton;
    private Button? _maximizeButton;

    protected MicaWindowWithOverlay()
    {
        _resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("/DevToys.Windows;component/Controls/MicaWindowWithOverlay.xaml", UriKind.RelativeOrAbsolute)
        };

        Style = _resourceDictionary["MicaWindowWithOverlayStyle"] as Style;

        ApplyResizeBorderThickness();

        Closed += MicaWindowWithOverlay_Closed;
    }

    internal static readonly DependencyProperty TitleBarMarginLeftProperty
        = DependencyProperty.Register(
            nameof(TitleBarMarginLeft),
            typeof(GridLength),
            typeof(MicaWindowWithOverlay));

    internal GridLength TitleBarMarginLeft
    {
        get => (GridLength)GetValue(MarginMaximizedProperty);
        set => SetValue(MarginMaximizedProperty, value);
    }

    internal static readonly DependencyProperty TitleBarMarginRightProperty
        = DependencyProperty.Register(
            nameof(TitleBarMarginRight),
            typeof(GridLength),
            typeof(MicaWindowWithOverlay));

    internal GridLength TitleBarMarginRight
    {
        get => (GridLength)GetValue(MarginMaximizedProperty);
        set => SetValue(MarginMaximizedProperty, value);
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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var closeButton = (Button)Template.FindName("CloseButton", this);
        var minimizeButton = (Button)Template.FindName("MinimizeButton", this);
        _restoreButton = (Button)Template.FindName("RestoreButton", this);
        _maximizeButton = (Button)Template.FindName("MaximizeButton", this);
        var draggableTitleBarArea = (Border)Template.FindName("DraggableTitleBarArea", this);
        var overlayControl = (OverlayControl)Template.FindName("TitleBar", this);

        closeButton.Click += CloseButton_Click;
        minimizeButton.Click += MinimizeButton_Click;
        _restoreButton.Click += RestoreButton_Click;
        _maximizeButton.Click += MaximizeButton_Click;
        draggableTitleBarArea.MouseLeftButtonDown += DraggableTitleBarArea_MouseLeftButtonDown;
        draggableTitleBarArea.MouseRightButtonUp += DraggableTitleBarArea_MouseRightButtonUp;
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

    private void MicaWindowWithOverlay_Closed(object? sender, EventArgs e)
    {
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
            // Toggle Maximize / Normal state.
            if (WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }
        else
        {
            // Move the window.
            DragMove();
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
        if (msg == HwndSourceMessages.WM_NCHITTEST)
        {
            bool isWindow11_OrGreater = _windowsVersion.Value >= new Version(10, 0, 22000);
            if (isWindow11_OrGreater & SnapLayoutHelper.IsSnapLayoutEnabled())
            {
                nint result = ShowSnapLayout(lParam, ref handled);
                return (result, handled);
            }
        }
        else if (msg == HwndSourceMessages.WM_NCLBUTTONDOWN)
        {
            HandleClickOnMaximizeAndRestoreButon(lParam, ref handled);
        }

        return (nint.Zero, handled);
    }

    private void ApplyWindowAppearance()
    {
        // Apply the dark / light theme.
        UpdateTheme();

        bool isWindow10_17763_OrHigher = _windowsVersion.Value >= new Version(10, 0, 17763);

        // If Windows 10 17763 or higher
        if (isWindow10_17763_OrHigher)
        {
            nint windowHandle = new WindowInteropHelper(this).EnsureHandle();

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

        NativeMethods.ExtendFrame(mainWindowSrc.Handle, margins);
    }

    private void ApplyBackdrop(nint windowHandle)
    {
        Guard.IsNotNull(_themeListener);
        bool isWindow11_22523_OrGreater = _windowsVersion.Value >= new Version(10, 0, 22523);
        bool isWindow11_OrGreater = _windowsVersion.Value >= new Version(10, 0, 22000);

        if (_themeListener.ActualAppTheme == ApplicationTheme.Dark)
        {
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, DwmValues.True);
        }
        else
        {
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, DwmValues.False);
        }

        if (isWindow11_22523_OrGreater)
        {
            const int Mica = 2;
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, Mica);
        }
        else if (isWindow11_OrGreater)
        {
            NativeMethods.SetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT, DwmValues.True);
        }
    }

    private void ApplyResizeBorderThickness()
    {
        bool isWindow11_OrGreater = _windowsVersion.Value >= new Version(10, 0, 22000);
        int cornerRadius = isWindow11_OrGreater ? 8 : 0;

        MarginMaximized = WindowState == WindowState.Maximized ? new Thickness(6) : new Thickness(0);

        if (WindowState == WindowState.Maximized || ResizeMode == ResizeMode.NoResize)
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
            themeResourceDictionaryPath = "/DevToys.Windows;component/Themes/Dark.xaml";
            themeResourceDictionaryToRemovePath = "/DevToys.Windows;component/Themes/Light.xaml";
        }
        else
        {
            themeResourceDictionaryPath = "/DevToys.Windows;component/Themes/Light.xaml";
            themeResourceDictionaryToRemovePath = "/DevToys.Windows;component/Themes/Dark.xaml";
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

        bool isWindow10_17763_OrLower = _windowsVersion.Value < new Version(10, 0, 17763);

        // If Windows 10 17763 or lower
        if (isWindow10_17763_OrLower)
        {
            // Todo: Based on the app theme, apply a Background color to the Window.
        }
    }

    private nint ShowSnapLayout(nint lParam, ref bool handled)
    {
        short x = (short)(lParam.ToInt32() & 0xffff);
        int y = lParam.ToInt32() >> 16;
        var point = new Point(x, y);
        double DPI_SCALE = DpiHelper.LogicalToDeviceUnitsScalingFactorX;

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

    private void HandleClickOnMaximizeAndRestoreButon(nint lParam, ref bool handled)
    {
        int x = lParam.ToInt32() & 0xffff;
        int y = lParam.ToInt32() >> 16;

        double DPI_SCALE = DpiHelper.LogicalToDeviceUnitsScalingFactorX;

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
}
