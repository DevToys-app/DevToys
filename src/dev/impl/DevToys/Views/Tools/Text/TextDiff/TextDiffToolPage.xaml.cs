#nullable enable

using System;
using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.TextDiff;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.TextDiff
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
            if (ViewModel is null)
            {
                var parameters = (NavigationParameter)e.Parameter;

                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (TextDiffToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }

        private void OutputTextEditor_ExpandedChanged(object sender, EventArgs e)
        {
            if (OutputTextEditor.IsExpanded)
            {
                MainControl.Children.Remove(OutputTextEditor);
                MainControl.Visibility = Visibility.Collapsed;
                ExpandedControl.Children.Add(OutputTextEditor);
            }
            else
            {
                ExpandedControl.Children.Remove(OutputTextEditor);
                MainControl.Children.Add(OutputTextEditor);
                MainControl.Visibility = Visibility.Visible;
            }
        }
    }
}
