#nullable enable

using System;
using System.Collections.Generic;
using DevToys.Api.Tools;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace DevToys.UI.Extensions
{
    [MarkupExtensionReturnType(ReturnType = typeof(IList<TextHighlighter>))]
    [Bindable]
    public sealed class HighlighterExtension : MarkupExtension
    {
        /// <summary> 
        /// Identifies the KeyboardAccelerator attachted property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HighlightersProperty
            = DependencyProperty.RegisterAttached(
                "Highlighters",
                typeof(MatchSpan[]),
                typeof(HighlighterExtension),
                new PropertyMetadata(Array.Empty<MatchSpan>(), OnHighlightersChanged));

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
                var value = (MatchSpan[])e.NewValue;
                source.TextHighlighters.Clear();

                if (value.Length > 0)
                {
                    var highlighter = new TextHighlighter()
                    {
                        Background = new SolidColorBrush(Colors.Yellow)
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
}
