#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Helpers;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.StringEscapeUnescape;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DevToys.Views.Tools.StringEscapeUnescape
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StringEscapeUnescapeToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(StringEscapeUnescapeToolViewModel),
                typeof(StringEscapeUnescapeToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public StringEscapeUnescapeToolViewModel ViewModel
        {
            get => (StringEscapeUnescapeToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public StringEscapeUnescapeToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            if (ViewModel is null)
            {
                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (StringEscapeUnescapeToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                ViewModel.IsEscapeMode = !StringManipulationHelper.HasEscapeCharacters(parameters.ClipBoardContent!);
                ViewModel.InputValue = parameters.ClipBoardContent;
            }

            base.OnNavigatedTo(e);
        }
    }
}
