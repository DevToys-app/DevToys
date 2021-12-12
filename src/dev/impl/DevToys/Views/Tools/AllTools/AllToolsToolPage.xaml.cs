#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.AllTools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.AllTools
{
    public sealed partial class AllToolsToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(AllToolsToolViewModel),
                typeof(GroupToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public AllToolsToolViewModel ViewModel
        {
            get => (AllToolsToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public AllToolsToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            // Set the view model
            Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
            ViewModel = (AllToolsToolViewModel)parameters.ViewModel!;
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }
    }
}
