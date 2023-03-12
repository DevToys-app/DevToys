using DevToys.Api;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DevToys.UI.Framework.Controls.GuiTool;

public sealed partial class UIIconPresenter : ContentControl
{
    public UIIconPresenter()
    {
        this.InitializeComponent();

        Loaded += UIIconPresenter_Loaded;
        Unloaded += UIIconPresenter_Unloaded;
    }

    internal IUIIcon UIIcon => (IUIIcon)DataContext;

    private void UIIconPresenter_Loaded(object sender, RoutedEventArgs e)
    {
        UIIcon.IsEnabledChanged += UIIcon_IsEnabledChanged;
        UIIcon.IsVisibleChanged += UIIcon_IsVisibleChanged;
        UIIcon.FontNameChanged += UIIcon_FontNameChanged;
        UIIcon.GlyphChanged += UIIcon_GlyphChanged;
        UIIcon.SizeChanged += UIIcon_SizeChanged;

        IsEnabled = UIIcon.IsEnabled;
        Visibility = UIIcon.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        SetFontFamily(UIIcon.FontName);
        FontIcon.Glyph = UIIcon.Glyph;
        FontIcon.FontSize = UIIcon.Size;
    }

    private void UIIconPresenter_Unloaded(object sender, RoutedEventArgs e)
    {
        UIIcon.IsEnabledChanged -= UIIcon_IsEnabledChanged;
        UIIcon.IsVisibleChanged -= UIIcon_IsVisibleChanged;
        UIIcon.FontNameChanged -= UIIcon_FontNameChanged;
        UIIcon.GlyphChanged -= UIIcon_GlyphChanged;
        UIIcon.SizeChanged -= UIIcon_SizeChanged;
        Loaded -= UIIconPresenter_Loaded;
        Unloaded -= UIIconPresenter_Unloaded;
    }

    private void UIIcon_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UIIcon.IsEnabled;
    }

    private void UIIcon_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UIIcon.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UIIcon_FontNameChanged(object? sender, EventArgs e)
    {
        SetFontFamily(UIIcon.FontName);
    }

    private void UIIcon_GlyphChanged(object? sender, EventArgs e)
    {
        FontIcon.Glyph = UIIcon.Glyph;
    }

    private void UIIcon_SizeChanged(object? sender, EventArgs e)
    {
        FontIcon.FontSize = UIIcon.Size;
    }

    private void SetFontFamily(string fontName)
    {
        if (!string.IsNullOrEmpty(fontName))
        {
            if (Application.Current.Resources.TryGetValue(fontName, out object? result) && result is FontFamily fontFamily)
            {
                FontIcon.FontFamily = fontFamily;
                return;
            }

            ThrowHelper.ThrowInvalidDataException($"Unable to font the font named '{fontName}' in the app resources.");
        }

        FontIcon.FontFamily = null;
    }
}
