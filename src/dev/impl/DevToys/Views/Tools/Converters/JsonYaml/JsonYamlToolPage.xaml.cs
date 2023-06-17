#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Helpers.JsonYaml;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.JsonYaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.JsonYaml
{
    public sealed partial class JsonYamlToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(JsonYamlToolViewModel),
                typeof(JsonYamlToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JsonYamlToolViewModel ViewModel
        {
            get => (JsonYamlToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public JsonYamlToolPage()
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
                ViewModel = (JsonYamlToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                if (JsonHelper.IsValid(parameters.ClipBoardContent!))
                {
                    ViewModel.ConversionMode = JsonYamlToolViewModel.JsonToYaml;
                }
                else
                {
                    ViewModel.ConversionMode = JsonYamlToolViewModel.YamlToJson;
                }

                ViewModel.InputValue = parameters.ClipBoardContent;
            }

            base.OnNavigatedTo(e);
        }

        private void OutputCodeEditor_ExpandedChanged(object sender, System.EventArgs e)
        {
            if (OutputCodeEditor.IsExpanded)
            {
                InputOutputGrid.Children.Remove(OutputCodeEditor);
                MainGrid.Visibility = Visibility.Collapsed;
                ExpandedGrid.Children.Add(OutputCodeEditor);
            }
            else
            {
                ExpandedGrid.Children.Remove(OutputCodeEditor);
                InputOutputGrid.Children.Add(OutputCodeEditor);
                MainGrid.Visibility = Visibility.Visible;
            }
        }
    }
}
