using DevToys.Api;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace DevToys.UI.Framework.Extensions;

[MarkupExtensionReturnType(ReturnType = typeof(IList<TextHighlighter>))]
[Microsoft.UI.Xaml.Data.Bindable]
public sealed class HighlighterExtension : MarkupExtension
{
    /// <summary> 
    /// Identifies the KeyboardAccelerator attachted property. This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty HighlightersProperty
        = DependencyProperty.RegisterAttached(
            "Highlighters",
            typeof(TextSpan[]),
            typeof(HighlighterExtension),
            new PropertyMetadata(Array.Empty<TextSpan>(), OnHighlightersChanged));

    /// <summary>
    /// Gets the value of the Highlighters attached property from the specified FrameworkElement.
    /// </summary>
    public static object GetHighlighters(DependencyObject obj)
    {
        return obj.GetValue(HighlightersProperty);
    }

    /// <summary>
    /// Sets the value of the Highlighters attached property to the specified FrameworkElement.
    /// </summary>
    /// <param name="obj">The object on which to set the KeyboardAccelerator attached property.</param>
    /// <param name="value">The property value to set.</param>
    public static void SetHighlighters(DependencyObject obj, object value)
    {
        obj.SetValue(HighlightersProperty, value);
    }

    /// <summary>
    /// Highlighters changed handler. 
    /// </summary>
    /// <param name="d">FrameworkElement that changed its KeyboardAccelerator attached property.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs with the new and old value.</param> 
    private static void OnHighlightersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RichTextBlock source)
        {
            var value = e.NewValue as TextSpan[];
            source.TextHighlighters.Clear();

            if (value is not null && value.Length > 0)
            {
                ElementTheme currentTheme = source.ActualTheme;
                string? highlighterBackgroundResourceName = currentTheme == ElementTheme.Light ? "SystemAccentColorLight2" : "SystemAccentColorDark1";
                Color highlighterForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;

                var highlighter = new TextHighlighter()
                {
                    Background = new SolidColorBrush((Color)Application.Current.Resources[highlighterBackgroundResourceName]),
                    Foreground = new SolidColorBrush(highlighterForegroundColor)
                };

                for (int i = 0; i < value.Length; i++)
                {
                    highlighter.Ranges.Add(
                        new TextRange
                        {
                            StartIndex = value[i].StartPosition,
                            Length = value[i].Length
                        });
                }

                source.TextHighlighters.Add(highlighter);
            }
        }
    }
}
