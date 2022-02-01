#nullable enable

using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace DevToys.UI.Extensions
{
    /// <summary>
    /// Based on https://gist.github.com/angularsen/90040fb174f71c5ab3ad
    /// </summary>
    [Bindable]
    public sealed class MarginExtension : MarkupExtension
    {
        public static readonly DependencyProperty MarginProperty
            = DependencyProperty.RegisterAttached(
                "Margin",
                typeof(Thickness),
                typeof(MarginExtension),
                new PropertyMetadata(new Thickness(), MarginChangedCallback));

        public static readonly DependencyProperty LastItemMarginProperty
            = DependencyProperty.RegisterAttached(
                "LastItemMargin",
                typeof(Thickness),
                typeof(MarginExtension),
                new PropertyMetadata(new Thickness(), MarginChangedCallback));

        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        public static void SetLastItemMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(LastItemMarginProperty, value);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        private static Thickness GetLastItemMargin(Panel obj)
        {
            return (Thickness)obj.GetValue(LastItemMarginProperty);
        }

        private static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Make sure this is put on a panel
            if (sender is not Panel panel)
            {
                return;
            }

            // Avoid duplicate registrations
            panel.Loaded -= OnPanelLoaded;
            panel.Loaded += OnPanelLoaded;

            if (panel.IsLoaded)
            {
                OnPanelLoaded(panel, null);
            }
        }

        private static void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            var panel = (Panel)sender;

            // Go over the children and set margin for them:
            IEnumerable<UIElement> visibleChildren = panel.Children.Where(elem => elem.Visibility != Visibility.Collapsed);
            for (int i = 0; i < visibleChildren.Count(); i++)
            {
                UIElement child = panel.Children[i];
                if (child is not FrameworkElement fe)
                {
                    continue;
                }

                bool isLastItem = i == visibleChildren.Count() - 1;
                fe.Margin = isLastItem ? GetLastItemMargin(panel) : GetMargin(panel);
            }
        }
    }
}
