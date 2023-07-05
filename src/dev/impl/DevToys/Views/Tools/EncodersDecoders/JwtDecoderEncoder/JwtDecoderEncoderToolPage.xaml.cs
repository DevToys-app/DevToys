﻿#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.JwtDecoderEncoder;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.JwtDecoderEncoder
{
    public sealed partial class JwtDecoderEncoderToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(JwtDecoderEncoderToolViewModel),
                typeof(JwtDecoderEncoderToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JwtDecoderEncoderToolViewModel ViewModel
        {
            get => (JwtDecoderEncoderToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public JwtDecoderEncoderToolPage()
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
                ViewModel = (JwtDecoderEncoderToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                ViewModel.DecoderViewModel.Token = parameters.ClipBoardContent;
                ViewModel.JwtToolMode = false;
            }

            base.OnNavigatedTo(e);
        }

        private void JwtDecoderControl_ExpandedChanged(object sender, System.EventArgs e)
        {
            if (sender is UIElement uiElement)
            {
                if (JwtDecoderControl.IsExpanded)
                {
                    MainGrid.Visibility = Visibility.Collapsed;
                    ExpandedGrid.Children.Add(uiElement);
                }
                else
                {
                    ExpandedGrid.Children.Remove(uiElement);
                    MainGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void JwtEncoderControl_ExpandedChanged(object sender, System.EventArgs e)
        {
            if (sender is UIElement uiElement)
            {
                if (JwtEncoderControl.IsExpanded)
                {
                    MainGrid.Visibility = Visibility.Collapsed;
                    ExpandedGrid.Children.Add(uiElement);
                }
                else
                {
                    ExpandedGrid.Children.Remove(uiElement);
                    MainGrid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
