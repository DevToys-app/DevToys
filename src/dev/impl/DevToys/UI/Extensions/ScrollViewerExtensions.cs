#nullable enable

using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace DevToys.UI.Extensions
{
    internal static class ScrollViewerExtensions
    {
        internal static ExpressionAnimation StartExpressionAnimation(
            this ScrollViewer scrollViewer,
            UIElement target,
            Axis axis)
        {
            return scrollViewer.StartExpressionAnimation(target, sourceAxis: axis, targetAxis: axis);
        }

        internal static ExpressionAnimation StartExpressionAnimation(
            this ScrollViewer scrollViewer,
            UIElement target,
            Axis sourceAxis,
            Axis targetAxis)
        {
            CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

            ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"{nameof(scrollViewer)}.{nameof(UIElement.Translation)}.{sourceAxis}");
            animation.SetReferenceParameter(nameof(scrollViewer), scrollSet);

            Visual visual = ElementCompositionPreview.GetElementVisual(target);
            visual.StartAnimation($"{nameof(Visual.Offset)}.{targetAxis}", animation);

            return animation;
        }
    }
}
