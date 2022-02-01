#nullable enable

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace DevToys.UI.Extensions
{
    /// <summary>
    /// Based on https://gist.github.com/angularsen/90040fb174f71c5ab3ad
    /// </summary>
    [Bindable]
    public sealed class SpacingExtension : MarkupExtension
    {

        public static readonly DependencyProperty VerticalProperty 
            = DependencyProperty.RegisterAttached(
                "Vertical", 
                typeof(double), 
                typeof(SpacingExtension),
                new PropertyMetadata(0d, VerticalChangedCallback));

        public static readonly DependencyProperty HorizontalProperty 
            = DependencyProperty.RegisterAttached(
                "Horizontal", 
                typeof(double), 
                typeof(SpacingExtension),
                new PropertyMetadata(0d, HorizontalChangedCallback));

        public static double GetHorizontal(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalProperty);
        }

        public static void SetHorizontal(DependencyObject obj, double space)
        {
            obj.SetValue(HorizontalProperty, space);
        }

        public static double GetVertical(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalProperty);
        }

        public static void SetVertical(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalProperty, value);
        }

        private static void HorizontalChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            double space = (double)e.NewValue;
            var obj = (DependencyObject)sender;

            MarginExtension.SetMargin(obj, new Thickness(0, 0, space, 0));
            MarginExtension.SetLastItemMargin(obj, new Thickness(0));
        }

        private static void VerticalChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            double space = (double)e.NewValue;
            var obj = (DependencyObject)sender;
            MarginExtension.SetMargin(obj, new Thickness(0, 0, 0, space));
            MarginExtension.SetLastItemMargin(obj, new Thickness(0));
        }

    }
}
