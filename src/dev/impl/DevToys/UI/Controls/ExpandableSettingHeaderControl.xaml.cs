#nullable enable

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace DevToys.UI.Controls
{
    [ContentProperty(Name = nameof(SettingActionableElement))]
    public sealed partial class ExpandableSettingHeaderControl : UserControl
    {
        public FrameworkElement? SettingActionableElement { get; set; }

        public static readonly DependencyProperty TitleProperty
           = DependencyProperty.Register(
               nameof(Title),
               typeof(string),
               typeof(ExpandableSettingHeaderControl),
               new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(ExpandableSettingHeaderControl),
                new PropertyMetadata(string.Empty));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(ExpandableSettingHeaderControl),
                new PropertyMetadata(null));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public ExpandableSettingHeaderControl()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, "NormalState", false);
        }

        private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width == e.PreviousSize.Width || ActionableElement == null)
            {
                return;
            }

            if (ActionableElement.ActualWidth > e.NewSize.Width / 3)
            {
                VisualStateManager.GoToState(this, "CompactState", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NormalState", false);
            }
        }
    }
}
