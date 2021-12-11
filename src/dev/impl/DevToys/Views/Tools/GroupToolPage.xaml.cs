#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools
{
    public sealed partial class GroupToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(GroupToolViewModel),
                typeof(GroupToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public GroupToolViewModel ViewModel
        {
            get => (GroupToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public GroupToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            // Set the view model
            Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
            ViewModel = (GroupToolViewModel)parameters.ViewModel!;
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }
    }
}
