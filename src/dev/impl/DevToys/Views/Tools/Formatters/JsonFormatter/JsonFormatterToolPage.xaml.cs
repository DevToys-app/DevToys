﻿#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.JsonFormatter;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.JsonFormatter
{
    public sealed partial class JsonFormatterToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(JsonFormatterToolViewModel),
                typeof(JsonFormatterToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JsonFormatterToolViewModel ViewModel
        {
            get => (JsonFormatterToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public JsonFormatterToolPage()
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
                ViewModel = (JsonFormatterToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
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
