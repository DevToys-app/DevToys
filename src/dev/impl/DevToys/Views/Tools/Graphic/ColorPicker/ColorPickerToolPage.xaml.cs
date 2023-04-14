#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.Graphic.ColorPicker;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.ColorPicker
{
    public sealed partial class ColorPickerToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
          = DependencyProperty.Register(
              nameof(ViewModel),
              typeof(ColorPickerToolViewModel),
              typeof(ColorPickerToolPage),
              new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public ColorPickerToolViewModel ViewModel
        {
            get => (ColorPickerToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ColorPickerToolPage()
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
                ViewModel = (ColorPickerToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }
    }
}
