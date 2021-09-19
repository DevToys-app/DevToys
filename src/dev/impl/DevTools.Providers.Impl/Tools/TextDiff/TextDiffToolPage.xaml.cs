#nullable enable

using DevTools.Core.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Providers.Impl.Tools.TextDiff
{
    public sealed partial class TextDiffToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(TextDiffToolViewModel),
                typeof(TextDiffToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public TextDiffToolViewModel ViewModel
        {
            get => (TextDiffToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public TextDiffToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            // Set the view model
            ViewModel = (TextDiffToolViewModel)parameters.Parameter!;
            DataContext = ViewModel;

            ViewModel.OutputTextBlock = OuputTextEditor;

            base.OnNavigatedTo(e);
        }
    }
}
