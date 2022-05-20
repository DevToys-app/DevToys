#nullable enable

using System;
using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.CronParser;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.CronParser
{
    public sealed partial class CronParserToolPage : Page
    {        
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(CronParserToolViewModel),
                typeof(CronParserToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public CronParserToolViewModel ViewModel
        {
            get => (CronParserToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public CronParserToolPage()
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
                ViewModel = (CronParserToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                if (long.TryParse(parameters.ClipBoardContent, out long timestamp))
                {
                    //ViewModel.CronExpression = timestamp;
                }
                else if (DateTime.TryParse(parameters.ClipBoardContent, out DateTime localDateTime))
                {
                    //ViewModel.CronExpression = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                }
            }

            base.OnNavigatedTo(e);
        }
    }
}
