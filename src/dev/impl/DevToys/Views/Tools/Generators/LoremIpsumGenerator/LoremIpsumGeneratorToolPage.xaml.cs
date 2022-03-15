#nullable enable

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.LoremIpsumGenerator;

namespace DevToys.Views.Tools.LoremIpsumGenerator
{
    public sealed partial class LoremIpsumGeneratorToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(LoremIpsumGeneratorToolViewModel),
                typeof(LoremIpsumGeneratorToolPage),
                new PropertyMetadata(null));

        public LoremIpsumGeneratorToolViewModel ViewModel
        {
            get => (LoremIpsumGeneratorToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public LoremIpsumGeneratorToolPage()
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
                ViewModel = (LoremIpsumGeneratorToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }

    }
}
