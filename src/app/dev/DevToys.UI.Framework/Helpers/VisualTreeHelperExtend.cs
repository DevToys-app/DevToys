using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace DevToys.UI.Framework.Helpers;

internal static class VisualTreeHelperExtend
{
    /// <summary>
    /// Get the first parent object of a given <paramref name="dependencyObject"/> that is of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>Returns null if no parent is of type <typeparamref name="T"/>.</returns>
    internal static T? FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
    {
        Guard.IsNotNull(dependencyObject);
        DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);

        if (parent == null)
        {
            return default;
        }

        if (parent is T parentT)
        {
            return parentT;
        }

        return FindParent<T>(parent);
    }
}
